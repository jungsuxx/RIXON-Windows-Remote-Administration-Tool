using System.Text;

namespace RIXON.Network;

// Packet layout: [4 bytes LE: payload_len][1 byte: type][payload...]
// String field:  [2 bytes LE: utf8_len][utf8 bytes]
//
// HELLO        (0x01) client→server: [OS][Hostname][ActiveWindow]
// UPDATE       (0x02) client→server: [ActiveWindow]
// PING         (0x03) server→client: empty
// PONG         (0x04) client→server: empty
// SCREEN_START (0x05) server→client: [quality:u8][fps:u8]
// SCREEN_STOP  (0x06) server→client: empty
// SCREEN_FRAME (0x07) client→server: [JPEG bytes]

public enum PacketType : byte
{
    Hello       = 0x01,
    Update      = 0x02,
    Ping        = 0x03,
    Pong        = 0x04,
    ScreenStart = 0x05,
    ScreenStop  = 0x06,
    ScreenFrame = 0x07,
}

public readonly struct HelloData
{
    public string Os           { get; init; }
    public string Hostname     { get; init; }
    public string ActiveWindow { get; init; }
}

public static class Protocol
{
    public static async Task<(PacketType Type, byte[] Payload)> ReadPacketAsync(
        Stream stream, CancellationToken ct)
    {
        byte[] header = await ReadExactAsync(stream, 5, ct);
        int len      = BitConverter.ToInt32(header, 0);
        byte type    = header[4];
        byte[] payload = len > 0
            ? await ReadExactAsync(stream, len, ct)
            : [];
        return ((PacketType)type, payload);
    }

    public static async Task WritePacketAsync(
        Stream stream, PacketType type, byte[] payload, CancellationToken ct)
    {
        byte[] packet = new byte[5 + payload.Length];
        BitConverter.GetBytes(payload.Length).CopyTo(packet, 0);
        packet[4] = (byte)type;
        payload.CopyTo(packet, 5);
        await stream.WriteAsync(packet, ct);
    }

    public static HelloData ParseHello(byte[] payload)
    {
        int offset = 0;
        return new HelloData
        {
            Os           = ReadString(payload, ref offset),
            Hostname     = ReadString(payload, ref offset),
            ActiveWindow = ReadString(payload, ref offset),
        };
    }

    public static string ParseUpdate(byte[] payload)
    {
        int offset = 0;
        return ReadString(payload, ref offset);
    }

    public static byte[] BuildPing()        => [];
    public static byte[] BuildScreenStart(byte quality, byte fps) => [quality, fps];
    public static byte[] BuildScreenStop() => [];

    // helpers

    private static string ReadString(byte[] buf, ref int offset)
    {
        ushort len = BitConverter.ToUInt16(buf, offset);
        offset += 2;
        string s = Encoding.UTF8.GetString(buf, offset, len);
        offset += len;
        return s;
    }

    private static async Task<byte[]> ReadExactAsync(Stream stream, int count, CancellationToken ct)
    {
        byte[] buf = new byte[count];
        int read = 0;
        while (read < count)
        {
            int n = await stream.ReadAsync(buf.AsMemory(read, count - read), ct);
            if (n == 0) throw new EndOfStreamException("Connection closed mid-read.");
            read += n;
        }
        return buf;
    }
}
