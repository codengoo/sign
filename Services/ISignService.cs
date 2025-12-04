namespace Signer.Services
{
    public interface ISignService
    {
        string Test();
        List<object> ListCerts(string pin);
        Boolean SignHash(string Pin, string Thumprint, string hashToSignBase64);
    }
}
