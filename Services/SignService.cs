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
            var pkcsKey = new PKCSKey(userPin);
            return pkcsKey.LoadCert();
        }

        public Boolean SignHash(string userPin, string thumbprint, string hashToSignBase64) {
            var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint);
            if (cert == null) return false;
            var privateKey = pkcsKey.GetPrivateKey(cert.KeyId);
            if (privateKey == null) return false;
            pkcsKey.SignHash(hashToSignBase64, privateKey);
            return true;
        }

        public String SignFile(string userPin, string thumbprint,string inputFile, string placeImage)
        {
            var pkcsKey = new PKCSKey(userPin);
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
