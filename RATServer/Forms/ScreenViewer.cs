using System.Diagnostics;
using System.Drawing.Imaging;
using RIXON.Network;

namespace RIXON.Forms;

public sealed partial class ScreenViewer : Form
{
    private readonly ClientSession _session;
    private bool _streaming;
    private int  _pendingFrame;
    private byte[]?  _latestFrame;
    private Bitmap?  _currentBmp;
    private int  _frameCount;
    private long _fpsTimestamp = Stopwatch.GetTimestamp();

    public ScreenViewer(ClientSession session)
    {
        _session = session;
        InitializeComponent();
        Text = $"Screen  —  {session.Info.Ip}  [{session.Info.Hostname}]";
        _session.Disconnected += OnSessionDisconnected;
    }

    // ── toolbar events ────────────────────────────────────────────────────

    private void StartStopBtn_Click(object? sender, EventArgs e)
    {
        if (_streaming) StopStreaming();
        else            StartStreaming();
    }

    private void QualityBar_Scroll(object? sender, EventArgs e)
    {
        _qualityValLabel.Text = $"{_qualityBar.Value}%";
        if (_streaming) ResendStart();
    }

    private void FpsBar_Scroll(object? sender, EventArgs e)
    {
        _fpsValLabel.Text = $"{_fpsBar.Value}";
        if (_streaming) ResendStart();
    }

    private void ScaleChk_CheckedChanged(object? sender, EventArgs e)
    {
        _screen.SizeMode = _scaleChk.Checked
            ? PictureBoxSizeMode.Zoom
            : PictureBoxSizeMode.Normal;
    }

    private void SaveBtn_Click(object? sender, EventArgs e)
    {
        var bmp = _currentBmp;
        if (bmp == null) return;

        using var dlg = new SaveFileDialog
        {
            Filter     = "PNG Image|*.png",
            FileName   = $"screen_{DateTime.Now:yyyyMMdd_HHmmss}.png",
            DefaultExt = "png",
        };
        if (dlg.ShowDialog(this) != DialogResult.OK) return;
        try   { bmp.Save(dlg.FileName, ImageFormat.Png); }
        catch (Exception ex) { MessageBox.Show(ex.Message, "Save Failed"); }
    }

    protected override void OnFormClosed(FormClosedEventArgs e)
    {
        _session.Disconnected -= OnSessionDisconnected;
        if (_streaming) StopStreaming();
        _currentBmp?.Dispose();
        base.OnFormClosed(e);
    }

    // ── streaming control ─────────────────────────────────────────────────

    private void StartStreaming()
    {
        _streaming = true;
        _startStopBtn.Text = "■  Stop";
        _session.ScreenFrameReceived += OnScreenFrame;
        ResendStart();
    }

    private void StopStreaming()
    {
        _streaming = false;
        _startStopBtn.Text = "▶  Start";
        _session.ScreenFrameReceived -= OnScreenFrame;
        _ = _session.SendScreenStopAsync();
    }

    private void ResendStart()
    {
        byte quality = (byte)_qualityBar.Value;
        byte fps     = (byte)_fpsBar.Value;
        _ = _session.SendScreenStartAsync(quality, fps);
    }

    // ── frame handling ────────────────────────────────────────────────────

    private void OnScreenFrame(object? sender, byte[] jpeg)
    {
        Interlocked.Exchange(ref _latestFrame, jpeg);
        if (Interlocked.CompareExchange(ref _pendingFrame, 1, 0) == 0)
            BeginInvoke(DisplayLatestFrame);
    }

    private void DisplayLatestFrame()
    {
        Interlocked.Exchange(ref _pendingFrame, 0);
        var data = Interlocked.Exchange(ref _latestFrame, null);
        if (data == null || IsDisposed) return;

        try
        {
            using var ms = new MemoryStream(data);
            var newBmp   = new Bitmap(ms);
            var old      = Interlocked.Exchange(ref _currentBmp, newBmp);
            _screen.Image = newBmp;
            old?.Dispose();
            TickFps();
        }
        catch { }
    }

    private void TickFps()
    {
        _frameCount++;
        long now    = Stopwatch.GetTimestamp();
        double secs = (now - _fpsTimestamp) / (double)Stopwatch.Frequency;
        if (secs >= 1.0)
        {
            _actualFpsLabel.Text  = $"{_frameCount / secs:F1} fps";
            _frameCount           = 0;
            _fpsTimestamp         = now;
        }
    }

    // ── session events ────────────────────────────────────────────────────

    private void OnSessionDisconnected(object? sender, EventArgs e)
    {
        if (IsDisposed) return;
        try
        {
            BeginInvoke(() =>
            {
                if (_streaming)
                {
                    _streaming = false;
                    _startStopBtn.Text = "▶  Start";
                    _session.ScreenFrameReceived -= OnScreenFrame;
                }
                _startStopBtn.Enabled = false;
                Text = $"Screen  —  {_session.Info.Ip}  [Disconnected]";
            });
        }
        catch { }
    }
}
