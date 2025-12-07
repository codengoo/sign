using iText.Kernel.Geom;
using iText.Signatures;
using Signer.Domain;
using Signer.Models;

namespace Signer.Services
{
    public class SignService : ISignService
    {
        public string Test()
        {
            return "Hello from Service!";
        }

        public List<CertInfo> ListCerts(string userPin)
        {
            using var pkcsKey = new PKCSKey(userPin);
            return pkcsKey.LoadCert();
        }

        public CertSigned SignHash(string userPin, string thumbprint, string hashToSignBase64)
        {
            using var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint) ?? throw new Exception("Cert not found");
            var privateKey = pkcsKey.GetPrivateKey(cert.KeyId) ?? throw new Exception("Private key not found");

            var signature = pkcsKey.SignHash(hashToSignBase64, privateKey);
            return new CertSigned(SignatureBase64: signature, CertificateBase64: cert.CertBase64);
        }

        public String SignPdfFile(string userPin, string thumbprint, string inputPdfPath, string outputPdfPath, string placeImage)
        {
            using var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint) ?? throw new  Exception("Cert not found");

            float posX = 200;
            float posY = 150;
            float width = 200;
            float height = 50;
            SignerProperties signerProps = new SignerProperties()
                .SetFieldName("Signature1")
                .SetPageRect(new Rectangle(posX, posY, width, height))
                .SetPageNumber(1)
                .SetReason("Tôi đồng ý với nội dung tài liệu")
                .SetLocation("Việt Nam");

            pkcsKey.SignPdfFile(cert, inputPdfPath, outputPdfPath, signerProps);
            return outputPdfPath;
        }
    }
}
