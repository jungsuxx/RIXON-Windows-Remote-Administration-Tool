using System.Collections.Concurrent;
using RIXON.Models;

namespace RIXON.Managers;

public sealed class ClientManager
{
    private readonly ConcurrentDictionary<Guid, ClientInfo> _clients = new();
    private readonly Dictionary<Guid, ListViewItem>         _items   = new();
    private ListView? _view;

    public int Count => _clients.Count;

    public void Attach(ListView view) => _view = view;

    public void AddClient(ClientInfo info)
    {
        _clients[info.Id] = info;
        SafeInvoke(() =>
        {
            if (!_clients.ContainsKey(info.Id)) return;

            var item = CreateItem(info);
            _items[info.Id] = item;
            _view!.Items.Add(item);
        });
    }

    public void UpdateClient(Guid id, string activeWindow)
    {
        if (!_clients.TryGetValue(id, out var info)) return;
        info.ActiveWindow = activeWindow;

        SafeInvoke(() =>
        {
            if (_items.TryGetValue(id, out var item))
                item.SubItems[3].Text = activeWindow;
        });
    }

    public void RemoveClient(Guid id)
    {
        _clients.TryRemove(id, out _);
        SafeInvoke(() =>
        {
            if (_items.Remove(id, out var item))
                _view!.Items.Remove(item);
        });
    }

    // ── helpers ──────────────────────────────────────────────────────────

    private static ListViewItem CreateItem(ClientInfo info)
    {
        var item = new ListViewItem(info.Ip);
        item.SubItems.Add(info.Os);
        item.SubItems.Add(info.Hostname);
        item.SubItems.Add(info.ActiveWindow);
        item.Tag = info.Id;
        return item;
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
