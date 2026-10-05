using System.Net;
using System.Net.Sockets;
using RIXON.Models;

namespace RIXON.Network;

public sealed class ClientSession : IDisposable
{
    private static readonly TimeSpan PingInterval = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan PongTimeout  = TimeSpan.FromSeconds(15);

    private readonly TcpClient              _tcp;
    private readonly Stream                 _stream; // NetworkStream or SslStream
    private readonly CancellationTokenSource _cts = new();
    private long _lastPongTicks;
    private int  _disposed;

    public ClientInfo Info { get; }

    public event EventHandler<HelloData>? HelloReceived;
    public event EventHandler<string>?   UpdateReceived;
    public event EventHandler<byte[]>?   ScreenFrameReceived;
    public event EventHandler?           Disconnected;

    public ClientSession(TcpClient tcp, Stream stream)
    {
        _tcp    = tcp;
        _stream = stream;
        Interlocked.Exchange(ref _lastPongTicks, DateTime.UtcNow.Ticks);
        string ip = (tcp.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString() ?? "Unknown";
        Info = new ClientInfo { Ip = ip };
    }

    public void Start()
    {
        _ = Task.Run(() => ReceiveLoopAsync(_cts.Token));
        _ = Task.Run(() => PingLoopAsync(_cts.Token));
    }

    // ── receive ──────────────────────────────────────────────────────────

    private async Task ReceiveLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                var (type, payload) = await Protocol.ReadPacketAsync(_stream, ct);
                switch (type)
                {
                    case PacketType.Hello:
                        var hello = Protocol.ParseHello(payload);
                        Info.Os           = hello.Os;
                        Info.Hostname     = hello.Hostname;
                        Info.ActiveWindow = hello.ActiveWindow;
                        HelloReceived?.Invoke(this, hello);
                        break;

                    case PacketType.Update:
                        string aw = Protocol.ParseUpdate(payload);
                        Info.ActiveWindow = aw;
                        UpdateReceived?.Invoke(this, aw);
                        break;

                    case PacketType.Pong:
                        Interlocked.Exchange(ref _lastPongTicks, DateTime.UtcNow.Ticks);
                        break;

                    case PacketType.ScreenFrame:
                        ScreenFrameReceived?.Invoke(this, payload);
                        break;
                }
            }
        }
        catch { }
        finally { SignalDisconnected(); }
    }

    // ── ping ─────────────────────────────────────────────────────────────

    private async Task PingLoopAsync(CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(PingInterval, ct);

                long ticks = Interlocked.Read(ref _lastPongTicks);
                if (DateTime.UtcNow - new DateTime(ticks, DateTimeKind.Utc) > PongTimeout)
                {
                    _cts.Cancel();
                    break;
                }

                await Protocol.WritePacketAsync(_stream, PacketType.Ping, Protocol.BuildPing(), ct);
            }
        }
        catch { }
    }

    // ── screen commands ──────────────────────────────────────────────────────

    public Task SendScreenStartAsync(byte quality, byte fps)
        => Protocol.WritePacketAsync(_stream, PacketType.ScreenStart,
               Protocol.BuildScreenStart(quality, fps), _cts.Token);

    public Task SendScreenStopAsync()
        => Protocol.WritePacketAsync(_stream, PacketType.ScreenStop,
               Protocol.BuildScreenStop(), _cts.Token);

    // ── cleanup ──────────────────────────────────────────────────────────

    private void SignalDisconnected()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 0)
        {
            Disconnected?.Invoke(this, EventArgs.Empty);
            Dispose();
        }
    }

    public void Dispose()
    {
        _cts.Cancel();
        try { _stream.Close(); } catch { }
        try { _tcp.Close();   } catch { }
    }
}
