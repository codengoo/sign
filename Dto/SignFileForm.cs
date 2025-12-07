namespace Signer.Dto
{
    public class SignFileForm
    {
        public required string Pin { get; set; }
        public required string Thumbprint { get; set; }
        public required IFormFile File { get; set; }
        public required IFormFile Image { get; set; }
        public required int Page {  get; set; }
        public required int PosX { get; set; }
        public required int PosY { get; set; }
        public required int Height { get; set; }
        public required int Width { get; set; }
    }
}
