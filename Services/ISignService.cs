using Signer.Models;

namespace Signer.Services
{
    public interface ISignService
    {
        string Test();
        List<CertInfo> ListCerts(string userPin);
        CertSigned SignHash(string userPin, string thumbprint, string hashToSignBase64);
        String SignPdfFile(string userPin, string thumbprint, string inputPdfPath, string outputPdfPath, string signatureImage, Position position);
        List<ProviderInfo> ScanProvider();
    }
}
