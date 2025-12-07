namespace Signer.Services.Shared
{
    public interface IFileUpload
    {
        Task<string> SaveFileAsync(IFormFile file, string subFolder = "");
    }
}
