using System.Security.Cryptography.X509Certificates;

namespace Signer.Models
{
    public record CertInfo(
        string Label,
        byte[] KeyId,
        string Subject,
        string Thumbprint,
        DateTime NotBefore,
        DateTime NotAfter,
        string CertBase64,
        X509Certificate2 Cert
    );
}
