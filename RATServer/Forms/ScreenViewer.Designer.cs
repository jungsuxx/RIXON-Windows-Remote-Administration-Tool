namespace RIXON.Forms;

partial class ScreenViewer
{
    private System.ComponentModel.IContainer components = null;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
            components.Dispose();
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        _toolPanel       = new Panel();
        _startStopBtn    = new Button();
        _qualityLabel    = new Label();
        _qualityBar      = new TrackBar();
        _qualityValLabel = new Label();
        _fpsLabel        = new Label();
        _fpsBar          = new TrackBar();
        _fpsValLabel     = new Label();
        _actualFpsLabel  = new Label();
        _saveBtn         = new Button();
        _scaleChk        = new CheckBox();
        _screen          = new PictureBox();
        _toolPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)_qualityBar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_fpsBar).BeginInit();
        ((System.ComponentModel.ISupportInitialize)_screen).BeginInit();
        SuspendLayout();
        //
        // _startStopBtn
        //
        _startStopBtn.Font     = new Font("Segoe UI", 9F, FontStyle.Bold);
        _startStopBtn.Location = new Point(6, 6);
        _startStopBtn.Name     = "_startStopBtn";
        _startStopBtn.Size     = new Size(88, 28);
        _startStopBtn.TabIndex = 0;
        _startStopBtn.Text     = "▶  Start";
        _startStopBtn.UseVisualStyleBackColor = true;
        _startStopBtn.Click   += StartStopBtn_Click;
        //
        // _qualityLabel
        //
        _qualityLabel.AutoSize = false;
        _qualityLabel.Font     = new Font("Segoe UI", 9F);
        _qualityLabel.Location = new Point(104, 11);
        _qualityLabel.Name     = "_qualityLabel";
        _qualityLabel.Size     = new Size(50, 18);
        _qualityLabel.TabIndex = 1;
        _qualityLabel.Text     = "Quality:";
        //
        // _qualityBar
        //
        _qualityBar.Location      = new Point(156, 3);
        _qualityBar.Maximum       = 95;
        _qualityBar.Minimum       = 10;
        _qualityBar.Name          = "_qualityBar";
        _qualityBar.Size          = new Size(110, 34);
        _qualityBar.TabIndex      = 2;
        _qualityBar.TickFrequency = 10;
        _qualityBar.Value         = 50;
        _qualityBar.Scroll       += QualityBar_Scroll;
        //
        // _qualityValLabel
        //
        _qualityValLabel.AutoSize = false;
        _qualityValLabel.Font     = new Font("Segoe UI", 9F);
        _qualityValLabel.Location = new Point(268, 11);
        _qualityValLabel.Name     = "_qualityValLabel";
        _qualityValLabel.Size     = new Size(32, 18);
        _qualityValLabel.TabIndex = 3;
        _qualityValLabel.Text     = "50%";
        //
        // _fpsLabel
        //
        _fpsLabel.AutoSize = false;
        _fpsLabel.Font     = new Font("Segoe UI", 9F);
        _fpsLabel.Location = new Point(304, 11);
        _fpsLabel.Name     = "_fpsLabel";
        _fpsLabel.Size     = new Size(32, 18);
        _fpsLabel.TabIndex = 4;
        _fpsLabel.Text     = "FPS:";
        //
        // _fpsBar
        //
        _fpsBar.Location      = new Point(338, 3);
        _fpsBar.Maximum       = 30;
        _fpsBar.Minimum       = 1;
        _fpsBar.Name          = "_fpsBar";
        _fpsBar.Size          = new Size(90, 34);
        _fpsBar.TabIndex      = 5;
        _fpsBar.TickFrequency = 5;
        _fpsBar.Value         = 10;
        _fpsBar.Scroll       += FpsBar_Scroll;
        //
        // _fpsValLabel
        //
        _fpsValLabel.AutoSize = false;
        _fpsValLabel.Font     = new Font("Segoe UI", 9F);
        _fpsValLabel.Location = new Point(430, 11);
        _fpsValLabel.Name     = "_fpsValLabel";
        _fpsValLabel.Size     = new Size(24, 18);
        _fpsValLabel.TabIndex = 6;
        _fpsValLabel.Text     = "10";
        //
        // _actualFpsLabel
        //
        _actualFpsLabel.AutoSize  = false;
        _actualFpsLabel.Font      = new Font("Segoe UI", 9F);
        _actualFpsLabel.ForeColor = SystemColors.GrayText;
        _actualFpsLabel.Location  = new Point(456, 11);
        _actualFpsLabel.Name      = "_actualFpsLabel";
        _actualFpsLabel.Size      = new Size(64, 18);
        _actualFpsLabel.TabIndex  = 7;
        _actualFpsLabel.Text      = "0.0 fps";
        //
        // _saveBtn
        //
        _saveBtn.Location = new Point(526, 6);
        _saveBtn.Name     = "_saveBtn";
        _saveBtn.Size     = new Size(90, 28);
        _saveBtn.TabIndex = 8;
        _saveBtn.Text     = "Save Frame";
        _saveBtn.UseVisualStyleBackColor = true;
        _saveBtn.Click   += SaveBtn_Click;
        //
        // _scaleChk
        //
        _scaleChk.AutoSize        = true;
        _scaleChk.Checked         = true;
        _scaleChk.CheckState      = CheckState.Checked;
        _scaleChk.Font            = new Font("Segoe UI", 9F);
        _scaleChk.Location        = new Point(624, 10);
        _scaleChk.Name            = "_scaleChk";
        _scaleChk.TabIndex        = 9;
        _scaleChk.Text            = "Scale to Fit";
        _scaleChk.UseVisualStyleBackColor = true;
        _scaleChk.CheckedChanged += ScaleChk_CheckedChanged;
        //
        // _toolPanel
        //
        _toolPanel.Controls.Add(_startStopBtn);
        _toolPanel.Controls.Add(_qualityLabel);
        _toolPanel.Controls.Add(_qualityBar);
        _toolPanel.Controls.Add(_qualityValLabel);
        _toolPanel.Controls.Add(_fpsLabel);
        _toolPanel.Controls.Add(_fpsBar);
        _toolPanel.Controls.Add(_fpsValLabel);
        _toolPanel.Controls.Add(_actualFpsLabel);
        _toolPanel.Controls.Add(_saveBtn);
        _toolPanel.Controls.Add(_scaleChk);
        _toolPanel.Dock     = DockStyle.Top;
        _toolPanel.Location = new Point(0, 0);
        _toolPanel.Name     = "_toolPanel";
        _toolPanel.Size     = new Size(800, 40);
        _toolPanel.TabIndex = 0;
        //
        // _screen
        //
        _screen.BackColor = Color.Black;
        _screen.Dock      = DockStyle.Fill;
        _screen.Location  = new Point(0, 40);
        _screen.Name      = "_screen";
        _screen.Size      = new Size(800, 500);
        _screen.SizeMode  = PictureBoxSizeMode.Zoom;
        _screen.TabStop   = false;
        //
        // ScreenViewer
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode       = AutoScaleMode.Font;
        ClientSize          = new Size(800, 540);
        Controls.Add(_screen);
        Controls.Add(_toolPanel);
        Font          = new Font("Segoe UI", 9F);
        MinimumSize   = new Size(500, 400);
        Name          = "ScreenViewer";
        StartPosition = FormStartPosition.CenterParent;
        Text          = "Screen Viewer";
        _toolPanel.ResumeLayout(false);
        _toolPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)_qualityBar).EndInit();
        ((System.ComponentModel.ISupportInitialize)_fpsBar).EndInit();
        ((System.ComponentModel.ISupportInitialize)_screen).EndInit();
        ResumeLayout(false);
    }

    #endregion

    private Panel      _toolPanel;
    private Button     _startStopBtn;
    private Label      _qualityLabel;
    private TrackBar   _qualityBar;
    private Label      _qualityValLabel;
    private Label      _fpsLabel;
    private TrackBar   _fpsBar;
    private Label      _fpsValLabel;
    private Label      _actualFpsLabel;
    private Button     _saveBtn;
    private CheckBox   _scaleChk;
    private PictureBox _screen;
}
