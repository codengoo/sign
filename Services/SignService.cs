using iText.Forms.Form.Element;
using iText.IO.Image;
using iText.Kernel.Colors;
using iText.Kernel.Geom;
using iText.Layout.Borders;
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

        public String SignPdfFile(string userPin, string thumbprint, string inputPdfPath, string outputPdfPath, string signatureImagePath, Position position)
        {
            using var pkcsKey = new PKCSKey(userPin);
            var cert = pkcsKey.GetCertByThumprint(thumbprint) ?? throw new Exception("Cert not found");

            SignatureFieldAppearance appearance = new SignatureFieldAppearance("signature-field");
            appearance.SetWidth(position.Width);
            appearance.SetHeight(position.Height);
            appearance.SetBorder(new SolidBorder(ColorConstants.DARK_GRAY, 2));
            appearance.SetContent(ImageDataFactory.Create(signatureImagePath));

            SignerProperties signerProps = new SignerProperties()
                .SetFieldName("signature-field")
                .SetPageRect(new Rectangle(position.PosX, position.PosY, position.Width, position.Height))
                .SetPageNumber(position.Page)
                .SetReason("Tôi đồng ý với nội dung tài liệu")
                .SetLocation("Việt Nam")
                .SetSignatureAppearance(appearance);

            pkcsKey.SignPdfFile(cert, inputPdfPath, outputPdfPath, signerProps);
            return outputPdfPath;
        }
    }
}
