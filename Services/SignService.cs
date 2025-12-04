using Net.Pkcs11Interop.Common;
using Net.Pkcs11Interop.HighLevelAPI;
using System.Security.Cryptography.X509Certificates;

namespace Signer.Services
{
    public class SignService : ISignService
    {
        public string Test()
        {
            return "Hello from Service!";
        }

        public List<object> ListCerts(string userPin)
        {
            //string pkcs11LibPath = @"E:\te\fca_v1.dll";
            string pkcs11LibPath = Path.Combine(AppContext.BaseDirectory, "Native/fca_v1.dll");
            Console.WriteLine(pkcs11LibPath);
        
            var factories = new Pkcs11InteropFactories();
            using var library = factories.Pkcs11LibraryFactory.LoadPkcs11Library(
                factories,
                pkcs11LibPath,
                AppType.MultiThreaded
            );

            var slots = library.GetSlotList(SlotsType.WithTokenPresent);
            if (slots.Count == 0)
            {
                //return "Không tìm thấy slot";
                return [];
            }

            var slot = slots[0];
            using var session = slot.OpenSession(SessionType.ReadOnly);
            session.Login(CKU.CKU_USER, userPin);

            List<IObjectHandle> certs = session.FindAllObjects(new List<IObjectAttribute>()
            {
                factories.ObjectAttributeFactory.Create(CKA.CKA_CLASS, CKO.CKO_CERTIFICATE)
            });

            if (certs.Count == 0)
            {
                //return "Không tìm thấy cert";
                return [];
            }

            var list = new List<object>();
            foreach (var cert in certs)
            {
                var certAttrs = session.GetAttributeValue(cert, [CKA.CKA_LABEL, CKA.CKA_VALUE, CKA.CKA_ID, ]);
                var certBytes = certAttrs[1].GetValueAsByteArray();
                var x509 = X509CertificateLoader.LoadCertificate(certBytes);

                list.Add(new
                {
                    Label = certAttrs[0].GetValueAsString(),
                    Subject = x509.Subject,
                    Thumbprint = x509.Thumbprint,
                    NotBefore = x509.NotBefore,
                    NotAfter = x509.NotAfter,
                    CertBase64 = Convert.ToBase64String(certBytes)
                });
            }

            return list;
        }
    }
}
