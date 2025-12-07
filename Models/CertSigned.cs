namespace Signer.Models
{
    public record CertSigned(String SignatureBase64, String CertificateBase64);
}
