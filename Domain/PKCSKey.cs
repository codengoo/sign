using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Crypto;
using iText.Kernel.Pdf;
using iText.Signatures;
using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Net.Pkcs11Interop.HighLevelAPI.Factories;
using Org.BouncyCastle.Security;
using Signer.Models;
using System.Security.Cryptography.X509Certificates;
using ISession = Net.Pkcs11Interop.HighLevelAPI.ISession;

namespace Signer.Domain
{
    public class PKCSKey : IDisposable
    {
        private readonly String PKCSLibPath = Path.Combine(AppContext.BaseDirectory, "Native/fca_v1.dll");
        private readonly Pkcs11InteropFactories _factories;
        private Net.Pkcs11Interop.HighLevelAPI.ISession? _session;
        private IPkcs11Library? _library;

        public PKCSKey(String pin)
        {
            _factories = new Pkcs11InteropFactories();
            _library = _factories.Pkcs11LibraryFactory.LoadPkcs11Library(
               _factories,
               PKCSLibPath,
               AppType.MultiThreaded
           );

            var slots = _library.GetSlotList(SlotsType.WithTokenPresent);
            if (slots.Count == 0)
            {
                throw new Exception("Slot not found");
            }

            var slot = slots[0];
            _session = slot.OpenSession(SessionType.ReadOnly);
            _session.Login(CKU.CKU_USER, pin);
        }

        public List<CertInfo> LoadCert()
        {
            if (_session == null) throw new Exception("No session init.");
            List<IObjectHandle> certs = _session.FindAllObjects(
            [
                _factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            ]);

            if (certs.Count == 0)
            {
                //return "Không tìm thấy cert";
                return [];
            }

            var list = new List<CertInfo>();
            foreach (var cert in certs)
            {
                var certAttrs = _session.GetAttributeValue(cert, [CKA.CKA_LABEL, CKA.CKA_VALUE, CKA.CKA_ID,]);
                var certBytes = certAttrs[1].GetValueAsByteArray();
                var x509 = X509CertificateLoader.LoadCertificate(certBytes);

                list.Add(new CertInfo(
                    Label: certAttrs[0].GetValueAsString(),
                    KeyId: certAttrs[2].GetValueAsByteArray(),
                    Subject: x509.Subject,
                    Thumbprint: x509.Thumbprint,
                    NotBefore: x509.NotBefore,
                    NotAfter: x509.NotAfter,
                    CertBase64: Convert.ToBase64String(certBytes),
                    Cert: x509
                    )
                );
            }

            return list;
        }

        public CertInfo? GetCertByThumprint(String thumbprint)
        {
            var certList = this.LoadCert();
            foreach (var cert in certList)
            {
                if (String.Equals(cert.Thumbprint, thumbprint, StringComparison.OrdinalIgnoreCase))
                {
                    return cert;
                }
            }

            return null;
        }

        public IObjectHandle GetPrivateKey(Byte[] keyId)
        {
            if (_session == null) throw new Exception("No session init.");

            var privateKeys = _session.FindAllObjects(
           [
               _factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_PRIVATE_KEY),
               _factories.ObjectAttributeFactory.Create(CKA.CKA_ID, keyId)
           ]);

            if (privateKeys.Count == 0) throw new Exception("Private key not found.");
            return privateKeys[0];
        }

        public String SignHash(String hashBase64, IObjectHandle privateKey)
        {
            if (_session == null) throw new Exception("No session init.");
            var mechanism = _factories.MechanismFactory.Create(CKM.CKM_RSA_PKCS);
            byte[] hashToSign = Convert.FromBase64String(hashBase64);

            byte[] signature = _session.Sign(
                mechanism,
                privateKey,
                hashToSign
            );

            return Convert.ToBase64String(signature);
        }

        public void SignPdfFile(CertInfo cert, String inputPdfPath, String outputPdfPath, SignerProperties signProperties)
        {
            if (_session == null) throw new Exception("No session init.");

            IX509Certificate[] chain = [new X509CertificateBC(DotNetUtilities.FromX509Certificate(cert.Cert))];
            IObjectHandle privateKey = this.GetPrivateKey(cert.KeyId);

            Console.WriteLine(inputPdfPath);
            Console.WriteLine(outputPdfPath);
            var pdfReader = new PdfReader(inputPdfPath);
            var pdfWriter = new PdfWriter(outputPdfPath);
            var properties = new StampingProperties();

            PdfSigner signer = new PdfSigner(pdfReader, pdfWriter, properties);
            signer.SetSignerProperties(signProperties);

            IExternalSignature externalSignature = new Pkcs11Signature(
                _session,
                privateKey,
                DigestAlgorithms.SHA256,
                _factories.MechanismFactory
            );

            signer.SignDetached(externalSignature, chain, null, null, null, 0, PdfSigner.CryptoStandard.CMS);
        }

        public void Dispose()
        {
            try
            {
                _session?.Logout();
            }
            catch { }

            _session?.Dispose();
            _session = null;

            _library?.Dispose();
            _library = null;
        }
    }

    public class Pkcs11Signature(ISession session, IObjectHandle privateKey, string digestAlgorithm, IMechanismFactory mechanismFactory) : IExternalSignature
    {
        public string GetDigestAlgorithmName()
        {
            return digestAlgorithm;
        }

        public string GetSignatureAlgorithmName()
        {
            return "RSA";
        }

        public ISignatureMechanismParams? GetSignatureMechanismParameters()
        {
            return null;
        }

        public byte[] Sign(byte[] message)
        {
            // Dùng cơ chế SHA256+RSA PKCS#1 (token sẽ hash message rồi padding)
            //var mechanism = _mechanismFactory.Create(CKM.CKM_SHA256_RSA_PKCS);
            var mechanism = mechanismFactory.Create(CKM.CKM_RSA_PKCS);
            var signature = session.Sign(mechanism, privateKey, message);
            return signature;
        }
    }
}
