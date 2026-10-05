using System.Collections.Concurrent;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace RIXON.Network;

public sealed class ListenerManager : IDisposable
{
    private readonly ConcurrentDictionary<int, ListenerEntry> _listeners = new();
    private X509Certificate2? _cert;

    public event EventHandler<ClientSession>? ClientConnected;

    // ── certificate ───────────────────────────────────────────────────────

    public void SetCertificate(X509Certificate2? cert) => _cert = cert;

    // ── port management ───────────────────────────────────────────────────

    /// <summary>Returns false if the port is already active or binding fails.</summary>
    public bool AddPort(int port)
    {
        if (_listeners.ContainsKey(port)) return false;

        var tcp = new TcpListener(IPAddress.Any, port);
        try { tcp.Start(); }
        catch { return false; }

        var cts   = new CancellationTokenSource();
        var entry = new ListenerEntry(tcp, cts);

        if (!_listeners.TryAdd(port, entry))
        {
            tcp.Stop();
            return false;
        }

        _ = Task.Run(() => AcceptLoopAsync(tcp, cts.Token));
        return true;
    }

    public bool RemovePort(int port)
    {
        if (!_listeners.TryRemove(port, out var entry)) return false;
        entry.Cts.Cancel();
        entry.Listener.Stop();
        return true;
    }

    // ── accept loop ───────────────────────────────────────────────────────

    private async Task AcceptLoopAsync(TcpListener listener, CancellationToken ct)
    {
        try
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient tcp = await listener.AcceptTcpClientAsync(ct);
                tcp.NoDelay = true;

                _ = Task.Run(() => HandshakeAndRegisterAsync(tcp), ct);
            }
        }
        catch { }
    }

    private async Task HandshakeAndRegisterAsync(TcpClient tcp)
    {
        Stream stream;
        var cert = _cert; // snapshot — cert may change between connections

        if (cert != null)
        {
            var ssl = new SslStream(tcp.GetStream(), leaveInnerStreamOpen: false);
            try
            {
                await ssl.AuthenticateAsServerAsync(new SslServerAuthenticationOptions
                {
                    ServerCertificate        = cert,
                    EnabledSslProtocols      = SslProtocols.Tls12,
                    ClientCertificateRequired = false,
                });
                stream = ssl;
            }
            catch
            {
                await ssl.DisposeAsync();
                tcp.Close();
                return; // Reject plain-TCP or failed-handshake clients
            }
        }
        else
        {
            stream = tcp.GetStream(); // No cert = unencrypted (dev mode)
        }

        var session = new ClientSession(tcp, stream);
        ClientConnected?.Invoke(this, session);
        session.Start();
    }

    // ── cleanup ───────────────────────────────────────────────────────────

    public void Dispose()
    {
        foreach (int port in _listeners.Keys.ToArray())
            RemovePort(port);
    }

    private sealed record ListenerEntry(TcpListener Listener, CancellationTokenSource Cts);
}
