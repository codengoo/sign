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

        public String SignFile(string userPin, string thumbprint, string inputFile, string placeImage)
        {
            using var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint);
            if (cert == null) return "";

            string inputPdf = @"E:\te\test.pdf";
            string outputPdf = @"E:\te\test_signed.pdf";

            var pdfReader = new PdfReader(inputPdf);
            var pdfWriter = new PdfWriter(outputPdf);
            var properties = new StampingProperties();
            PdfSigner signer = new PdfSigner(pdfReader, pdfWriter, properties);

            return "";
        }
    }
}
