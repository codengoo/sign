namespace Signer.Models
{
    public record CertInfo(
        string Label,
        byte[] KeyId,
        string Subject,
        string Thumbprint,
        DateTime NotBefore,
        DateTime NotAfter,
        string CertBase64
    );
}
