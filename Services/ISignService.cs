namespace Signer.Services
{
    public interface ISignService
    {
        string Test();
        List<object> ListCerts(string pin);
    }
}
