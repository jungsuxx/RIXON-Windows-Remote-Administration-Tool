namespace RIXON.Models;

public sealed class ClientInfo
{
    public Guid Id { get; } = Guid.NewGuid();
    public string Ip { get; set; } = string.Empty;
    public string Os { get; set; } = string.Empty;
    public string Hostname { get; set; } = string.Empty;
    public string ActiveWindow { get; set; } = string.Empty;
    public DateTime ConnectedAt { get; } = DateTime.Now;
}
