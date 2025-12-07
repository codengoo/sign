using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using Signer.Models;
using System.Security.Cryptography.X509Certificates;

namespace Signer.Domain
{
    public class PKCSKey: IDisposable
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
                    CertBase64: Convert.ToBase64String(certBytes))
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
}
