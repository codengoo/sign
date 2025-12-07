namespace Signer.Services.Shared.FileUpload
{
    public interface IFileUpload
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder = "");
    }
}
