namespace Signer.Dto
{
    public class SignFileForm
    {
        public required string Pin { get; set; }
        public required string Thumbprint { get; set; }
        public required IFormFile File { get; set; }
    }
}
