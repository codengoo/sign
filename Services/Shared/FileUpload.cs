namespace Signer.Services.Shared
{
    public class FileUpload(IWebHostEnvironment env):IFileUpload
    {
        private readonly IWebHostEnvironment _env = env;

        public async Task<string> SaveFileAsync(IFormFile file, string subFolder = "")
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("File is empty");
            // Tạo thư mục upload
            var uploadRoot = Path.Combine(_env.WebRootPath, "uploads");

            if (!Directory.Exists(uploadRoot))
                Directory.CreateDirectory(uploadRoot);

            // Subfolder nếu cần
            if (!string.IsNullOrEmpty(subFolder))
            {
                uploadRoot = Path.Combine(uploadRoot, subFolder);

                if (!Directory.Exists(uploadRoot))
                    Directory.CreateDirectory(uploadRoot);
            }

            // Tạo tên file UUID
            var extension = Path.GetExtension(file.FileName);
            var newFileName = $"{Guid.NewGuid()}{extension}";
            var fullPath = Path.Combine(uploadRoot, newFileName);

            // Lưu file
            using var stream = new FileStream(fullPath, FileMode.Create);
            await file.CopyToAsync(stream);

            // Trả về đường dẫn tương đối để dùng trên web
            var relativePath = Path.Combine("uploads", subFolder, newFileName).Replace("\\", "/");

            return relativePath;

        }
    }
}
