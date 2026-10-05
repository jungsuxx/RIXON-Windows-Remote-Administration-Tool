namespace RIXON;

internal sealed class AddPortDialog : Form
{
    private readonly NumericUpDown _portInput;
    private readonly Button        _okBtn;
    private readonly Button        _cancelBtn;

    public int Port => (int)_portInput.Value;

    public AddPortDialog()
    {
        Text            = "Add Listening Port";
        FormBorderStyle = FormBorderStyle.FixedDialog;
        ClientSize      = new Size(260, 90);
        StartPosition   = FormStartPosition.CenterParent;
        MaximizeBox     = false;
        MinimizeBox     = false;
        ShowInTaskbar   = false;

        var label = new Label
        {
            Text     = "Port number:",
            Location = new Point(12, 14),
            AutoSize = true,
        };

        _portInput = new NumericUpDown
        {
            Location  = new Point(100, 11),
            Size      = new Size(80, 23),
            Minimum   = 1,
            Maximum   = 65535,
            Value     = 4444,
            TabIndex  = 0,
        };

        _okBtn = new Button
        {
            Text         = "OK",
            DialogResult = DialogResult.OK,
            Location     = new Point(80, 52),
            Size         = new Size(75, 26),
            TabIndex     = 1,
        };

        _cancelBtn = new Button
        {
            Text         = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location     = new Point(162, 52),
            Size         = new Size(75, 26),
            TabIndex     = 2,
        };

        AcceptButton = _okBtn;
        CancelButton = _cancelBtn;
        Controls.AddRange([label, _portInput, _okBtn, _cancelBtn]);
    }
}
