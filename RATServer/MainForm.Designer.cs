namespace RIXON;

partial class MainForm
{
    /// <summary>
    /// Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    /// Clean up any resources being used.
    /// </summary>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    /// Required method for Designer support - do not modify
    /// the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MainForm));
        _tabs = new TabControl();
        _tabClients = new TabPage();
        _clientsView = new ListView();
        _colClientIp = new ColumnHeader();
        _colClientOs = new ColumnHeader();
        _colClientHostname = new ColumnHeader();
        _colClientActiveWindow = new ColumnHeader();
        _clientMenu = new ContextMenuStrip(components);
        _screenMenuItem = new ToolStripMenuItem();
        _tabLogs = new TabPage();
        _logsView = new ListView();
        _colLogTime = new ColumnHeader();
        _colLogEvent = new ColumnHeader();
        _tabListening = new TabPage();
        _listeningView = new ListView();
        _colPort = new ColumnHeader();
        _colStatus = new ColumnHeader();
        _toolbarPanel = new Panel();
        _addPortBtn = new Button();
        _removePortBtn = new Button();
        _keyLabel = new Label();
        _keyTextBox = new TextBox();
        _certThumbLabel = new Label();
        _genCertBtn = new Button();
        _statusStrip = new StatusStrip();
        _statusLabel = new ToolStripStatusLabel();
        tabPage1 = new TabPage();
        pictureBox1 = new PictureBox();
        linkLabel1 = new LinkLabel();
        _tabs.SuspendLayout();
        _tabClients.SuspendLayout();
        _clientMenu.SuspendLayout();
        _tabLogs.SuspendLayout();
        _tabListening.SuspendLayout();
        _toolbarPanel.SuspendLayout();
        _statusStrip.SuspendLayout();
        tabPage1.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).BeginInit();
        SuspendLayout();
        // 
        // _tabs
        // 
        _tabs.Controls.Add(_tabClients);
        _tabs.Controls.Add(_tabLogs);
        _tabs.Controls.Add(_tabListening);
        _tabs.Controls.Add(tabPage1);
        _tabs.Dock = DockStyle.Fill;
        _tabs.Location = new Point(0, 0);
        _tabs.Name = "_tabs";
        _tabs.SelectedIndex = 0;
        _tabs.Size = new Size(960, 544);
        _tabs.TabIndex = 0;
        // 
        // _tabClients
        // 
        _tabClients.Controls.Add(_clientsView);
        _tabClients.Location = new Point(4, 24);
        _tabClients.Name = "_tabClients";
        _tabClients.Padding = new Padding(3);
        _tabClients.Size = new Size(952, 516);
        _tabClients.TabIndex = 0;
        _tabClients.Text = "Clients";
        _tabClients.UseVisualStyleBackColor = true;
        // 
        // _clientsView
        // 
        _clientsView.Columns.AddRange(new ColumnHeader[] { _colClientIp, _colClientOs, _colClientHostname, _colClientActiveWindow });
        _clientsView.ContextMenuStrip = _clientMenu;
        _clientsView.Dock = DockStyle.Fill;
        _clientsView.FullRowSelect = true;
        _clientsView.GridLines = true;
        _clientsView.Location = new Point(3, 3);
        _clientsView.MultiSelect = false;
        _clientsView.Name = "_clientsView";
        _clientsView.Size = new Size(946, 510);
        _clientsView.TabIndex = 0;
        _clientsView.UseCompatibleStateImageBehavior = false;
        _clientsView.View = View.Details;
        // 
        // _colClientIp
        // 
        _colClientIp.Text = "IP";
        _colClientIp.Width = 120;
        // 
        // _colClientOs
        // 
        _colClientOs.Text = "OS";
        _colClientOs.Width = 200;
        // 
        // _colClientHostname
        // 
        _colClientHostname.Text = "Hostname";
        _colClientHostname.Width = 160;
        // 
        // _colClientActiveWindow
        // 
        _colClientActiveWindow.Text = "Active Window";
        _colClientActiveWindow.Width = 380;
        // 
        // _clientMenu
        // 
        _clientMenu.Items.AddRange(new ToolStripItem[] { _screenMenuItem });
        _clientMenu.Name = "_clientMenu";
        _clientMenu.Size = new Size(111, 26);
        // 
        // _screenMenuItem
        // 
        _screenMenuItem.Name = "_screenMenuItem";
        _screenMenuItem.Size = new Size(110, 22);
        _screenMenuItem.Text = "Screen";
        // 
        // _tabLogs
        // 
        _tabLogs.Controls.Add(_logsView);
        _tabLogs.Location = new Point(4, 24);
        _tabLogs.Name = "_tabLogs";
        _tabLogs.Padding = new Padding(3);
        _tabLogs.Size = new Size(952, 516);
        _tabLogs.TabIndex = 1;
        _tabLogs.Text = "Logs";
        _tabLogs.UseVisualStyleBackColor = true;
        // 
        // _logsView
        // 
        _logsView.Columns.AddRange(new ColumnHeader[] { _colLogTime, _colLogEvent });
        _logsView.Dock = DockStyle.Fill;
        _logsView.FullRowSelect = true;
        _logsView.Location = new Point(3, 3);
        _logsView.Name = "_logsView";
        _logsView.Size = new Size(946, 510);
        _logsView.TabIndex = 0;
        _logsView.UseCompatibleStateImageBehavior = false;
        _logsView.View = View.Details;
        // 
        // _colLogTime
        // 
        _colLogTime.Text = "Time";
        _colLogTime.Width = 160;
        // 
        // _colLogEvent
        // 
        _colLogEvent.Text = "Event";
        _colLogEvent.Width = 740;
        // 
        // _tabListening
        // 
        _tabListening.Controls.Add(_listeningView);
        _tabListening.Controls.Add(_toolbarPanel);
        _tabListening.Location = new Point(4, 24);
        _tabListening.Name = "_tabListening";
        _tabListening.Size = new Size(952, 516);
        _tabListening.TabIndex = 2;
        _tabListening.Text = "Listening";
        _tabListening.UseVisualStyleBackColor = true;
        // 
        // _listeningView
        // 
        _listeningView.Columns.AddRange(new ColumnHeader[] { _colPort, _colStatus });
        _listeningView.Dock = DockStyle.Fill;
        _listeningView.FullRowSelect = true;
        _listeningView.GridLines = true;
        _listeningView.Location = new Point(0, 40);
        _listeningView.MultiSelect = false;
        _listeningView.Name = "_listeningView";
        _listeningView.Size = new Size(952, 476);
        _listeningView.TabIndex = 1;
        _listeningView.UseCompatibleStateImageBehavior = false;
        _listeningView.View = View.Details;
        // 
        // _colPort
        // 
        _colPort.Text = "Port";
        _colPort.Width = 80;
        // 
        // _colStatus
        // 
        _colStatus.Text = "Status";
        _colStatus.Width = 860;
        // 
        // _toolbarPanel
        // 
        _toolbarPanel.Controls.Add(_addPortBtn);
        _toolbarPanel.Controls.Add(_removePortBtn);
        _toolbarPanel.Controls.Add(_keyLabel);
        _toolbarPanel.Controls.Add(_keyTextBox);
        _toolbarPanel.Controls.Add(_certThumbLabel);
        _toolbarPanel.Controls.Add(_genCertBtn);
        _toolbarPanel.Dock = DockStyle.Top;
        _toolbarPanel.Location = new Point(0, 0);
        _toolbarPanel.Name = "_toolbarPanel";
        _toolbarPanel.Size = new Size(952, 40);
        _toolbarPanel.TabIndex = 0;
        // 
        // _addPortBtn
        // 
        _addPortBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _addPortBtn.Location = new Point(6, 6);
        _addPortBtn.Name = "_addPortBtn";
        _addPortBtn.Size = new Size(32, 28);
        _addPortBtn.TabIndex = 0;
        _addPortBtn.Text = "+";
        _addPortBtn.UseVisualStyleBackColor = true;
        // 
        // _removePortBtn
        // 
        _removePortBtn.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
        _removePortBtn.Location = new Point(42, 6);
        _removePortBtn.Name = "_removePortBtn";
        _removePortBtn.Size = new Size(32, 28);
        _removePortBtn.TabIndex = 1;
        _removePortBtn.Text = "−";
        _removePortBtn.UseVisualStyleBackColor = true;
        // 
        // _keyLabel
        // 
        _keyLabel.Font = new Font("Segoe UI", 9F);
        _keyLabel.Location = new Point(84, 11);
        _keyLabel.Name = "_keyLabel";
        _keyLabel.Size = new Size(52, 18);
        _keyLabel.TabIndex = 2;
        _keyLabel.Text = "TLS Key:";
        // 
        // _keyTextBox
        // 
        _keyTextBox.Font = new Font("Segoe UI", 9F);
        _keyTextBox.Location = new Point(138, 8);
        _keyTextBox.Name = "_keyTextBox";
        _keyTextBox.Size = new Size(180, 23);
        _keyTextBox.TabIndex = 3;
        _keyTextBox.Text = "RATServer";
        // 
        // _certThumbLabel
        // 
        _certThumbLabel.Font = new Font("Segoe UI", 8.5F);
        _certThumbLabel.ForeColor = SystemColors.GrayText;
        _certThumbLabel.Location = new Point(328, 11);
        _certThumbLabel.Name = "_certThumbLabel";
        _certThumbLabel.Size = new Size(380, 18);
        _certThumbLabel.TabIndex = 4;
        _certThumbLabel.Text = "No certificate";
        // 
        // _genCertBtn
        // 
        _genCertBtn.Location = new Point(718, 6);
        _genCertBtn.Name = "_genCertBtn";
        _genCertBtn.Size = new Size(118, 28);
        _genCertBtn.TabIndex = 5;
        _genCertBtn.Text = "Apply Key";
        _genCertBtn.UseVisualStyleBackColor = true;
        // 
        // _statusStrip
        // 
        _statusStrip.Items.AddRange(new ToolStripItem[] { _statusLabel });
        _statusStrip.Location = new Point(0, 544);
        _statusStrip.Name = "_statusStrip";
        _statusStrip.Size = new Size(960, 22);
        _statusStrip.TabIndex = 1;
        // 
        // _statusLabel
        // 
        _statusLabel.Name = "_statusLabel";
        _statusLabel.Size = new Size(945, 17);
        _statusLabel.Spring = true;
        _statusLabel.Text = "Clients: 0  |  Ports: 0";
        _statusLabel.TextAlign = ContentAlignment.MiddleLeft;
        // 
        // tabPage1
        // 
        tabPage1.Controls.Add(linkLabel1);
        tabPage1.Controls.Add(pictureBox1);
        tabPage1.Location = new Point(4, 24);
        tabPage1.Name = "tabPage1";
        tabPage1.Padding = new Padding(3);
        tabPage1.Size = new Size(952, 516);
        tabPage1.TabIndex = 3;
        tabPage1.Text = "About";
        tabPage1.UseVisualStyleBackColor = true;
        // 
        // pictureBox1
        // 
        pictureBox1.Image = (Image)resources.GetObject("pictureBox1.Image");
        pictureBox1.Location = new Point(35, 29);
        pictureBox1.Name = "pictureBox1";
        pictureBox1.Size = new Size(248, 246);
        pictureBox1.SizeMode = PictureBoxSizeMode.Zoom;
        pictureBox1.TabIndex = 0;
        pictureBox1.TabStop = false;
        // 
        // linkLabel1
        // 
        linkLabel1.AutoSize = true;
        linkLabel1.Location = new Point(35, 278);
        linkLabel1.Name = "linkLabel1";
        linkLabel1.Size = new Size(160, 15);
        linkLabel1.TabIndex = 1;
        linkLabel1.TabStop = true;
        linkLabel1.Text = "https://github.com/jungsuxx";
        linkLabel1.LinkClicked += linkLabel1_LinkClicked;
        // 
        // MainForm
        // 
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(960, 566);
        Controls.Add(_tabs);
        Controls.Add(_statusStrip);
        Font = new Font("Segoe UI", 9F);
        MinimumSize = new Size(700, 450);
        Name = "MainForm";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "RIXON";
        _tabs.ResumeLayout(false);
        _tabClients.ResumeLayout(false);
        _clientMenu.ResumeLayout(false);
        _tabLogs.ResumeLayout(false);
        _tabListening.ResumeLayout(false);
        _toolbarPanel.ResumeLayout(false);
        _toolbarPanel.PerformLayout();
        _statusStrip.ResumeLayout(false);
        _statusStrip.PerformLayout();
        tabPage1.ResumeLayout(false);
        tabPage1.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)pictureBox1).EndInit();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion

    private TabControl           _tabs;
    private TabPage              _tabClients;
    private TabPage              _tabLogs;
    private TabPage              _tabListening;
    private ListView             _clientsView;
    private ColumnHeader         _colClientIp;
    private ColumnHeader         _colClientOs;
    private ColumnHeader         _colClientHostname;
    private ColumnHeader         _colClientActiveWindow;
    private ListView             _logsView;
    private ColumnHeader         _colLogTime;
    private ColumnHeader         _colLogEvent;
    private Panel                _toolbarPanel;
    private Button               _addPortBtn;
    private Button               _removePortBtn;
    private Label                _keyLabel;
    private TextBox              _keyTextBox;
    private Label                _certThumbLabel;
    private Button               _genCertBtn;
    private ListView             _listeningView;
    private ColumnHeader         _colPort;
    private ColumnHeader         _colStatus;
    private StatusStrip          _statusStrip;
    private ToolStripStatusLabel _statusLabel;
    private ContextMenuStrip     _clientMenu;
    private ToolStripMenuItem    _screenMenuItem;
    private TabPage tabPage1;
    private LinkLabel linkLabel1;
    private PictureBox pictureBox1;
}
