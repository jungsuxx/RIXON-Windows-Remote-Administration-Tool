namespace RIXON.Managers;

public sealed class LogManager
{
    private const int MaxEntries = 2000;
    private ListView? _view;

    public void Attach(ListView view) => _view = view;

    public void Log(string message)
    {
        SafeInvoke(() =>
        {
            var item = new ListViewItem(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
            item.SubItems.Add(message);
            _view!.Items.Insert(0, item); // newest first

            if (_view.Items.Count > MaxEntries)
                _view.Items.RemoveAt(_view.Items.Count - 1);
        });
    }

    private void SafeInvoke(Action action)
    {
        if (_view == null || _view.IsDisposed) return;
        try
        {
            if (_view.InvokeRequired) _view.Invoke(action);
            else action();
        }
        catch (ObjectDisposedException) { }
    }
}
