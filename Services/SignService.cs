using iText.Kernel.Geom;
using iText.Kernel.Pdf;
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

        public String SignFile(string userPin, string thumbprint, string inputFilePath, string placeImage)
        {
            using var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint) ?? throw new  Exception("Cert not found");
            var privateKey = pkcsKey.GetPrivateKey(cert.KeyId) ?? throw new Exception("Private key not found");

            string outputPdf = @"E:\te\test_signed.pdf";
            float posX = 200;
            float posY = 150;
            float width = 200;
            float height = 50;

            var pdfReader = new PdfReader(inputFilePath);
            var pdfWriter = new PdfWriter(outputPdf);
            var properties = new StampingProperties();
            PdfSigner signer = new PdfSigner(pdfReader, pdfWriter, properties);

            SignerProperties signerProps = new SignerProperties()
               .SetFieldName("Signature1")
               .SetPageRect(new Rectangle(posX, posY, width, height))
               .SetPageNumber(1)
               .SetReason("Tôi đồng ý với nội dung tài liệu")
               .SetLocation("Việt Nam");
            
            signer.SetSignerProperties(signerProps);

            return "";
        }
    }
}
