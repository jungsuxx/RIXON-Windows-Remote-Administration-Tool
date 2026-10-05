using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;

namespace RIXON.Crypto;

public static class CertManager
{
    private const string InternalPassword = "rat_srv_key_v1";

    /// <summary>
    /// Generate a 2048-bit RSA self-signed certificate valid for 10 years.
    /// </summary>
    public static X509Certificate2 GenerateSelfSigned(string cn = "RATServer")
    {
        if (string.IsNullOrWhiteSpace(cn)) cn = "RATServer";

        using var rsa = RSA.Create(2048);

        var req = new CertificateRequest(
            $"CN={cn}",
            rsa,
            HashAlgorithmName.SHA256,
            RSASignaturePadding.Pkcs1);

        req.CertificateExtensions.Add(
            new X509BasicConstraintsExtension(false, false, 0, false));
        req.CertificateExtensions.Add(
            new X509KeyUsageExtension(
                X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
                critical: false));

        var cert = req.CreateSelfSigned(
            DateTimeOffset.UtcNow.AddDays(-1),
            DateTimeOffset.UtcNow.AddYears(10));

        // Re-import so the private key is persistable/exportable on Windows
        var pfx = cert.Export(X509ContentType.Pfx, InternalPassword);
        return new X509Certificate2(pfx, InternalPassword,
            X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
    }

    /// <summary>Load a .pfx file the user selected.  Returns null on failure.</summary>
    public static X509Certificate2? LoadPfx(string path, string password)
    {
        try
        {
            return new X509Certificate2(path, password,
                X509KeyStorageFlags.PersistKeySet | X509KeyStorageFlags.Exportable);
        }
        catch { return null; }
    }

    /// <summary>Save cert to path using the internal password (for auto-load on next run).</summary>
    public static void SaveDefault(X509Certificate2 cert, string path)
        => File.WriteAllBytes(path, cert.Export(X509ContentType.Pfx, InternalPassword));

    /// <summary>Load the auto-saved cert.  Returns null if not found or corrupted.</summary>
    public static X509Certificate2? LoadDefault(string path)
    {
        if (!File.Exists(path)) return null;
        return LoadPfx(path, InternalPassword);
    }

    /// <summary>Returns the SHA-256 fingerprint formatted as space-separated hex bytes.</summary>
    public static string GetThumbprint(X509Certificate2 cert)
    {
        string raw = cert.GetCertHashString(HashAlgorithmName.SHA256);
        return string.Join(" ",
            Enumerable.Range(0, raw.Length / 2).Select(i => raw.Substring(i * 2, 2)));
    }
}
