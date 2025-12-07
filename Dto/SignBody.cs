namespace Signer.Dto
{
    public class SignBody
    {
        public required string Pin { get; set; }
        public required string Thumbprint { get; set; }
        public required string HashToSignBase64 { get; set; }
    }
}
