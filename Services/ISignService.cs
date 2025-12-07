using Signer.Models;

namespace Signer.Services
{
    public interface ISignService
    {
        string Test();
        List<CertInfo> ListCerts(string userPin);
        CertSigned SignHash(string userPin, string thumbprint, string hashToSignBase64);
        String SignFile(string userPin, string thumbprint, string inputFilePath, string placeImage);
    }
}
