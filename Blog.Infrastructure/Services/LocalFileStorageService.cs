using Blog.Application.Files.Dtos;
using Blog.Application.Interfaces;
using FluentResults;
using Microsoft.AspNetCore.Hosting;

namespace Blog.Infrastructure.Services
{
    internal class LocalFileStorageService : IFileStorageService
    {
        private readonly IWebHostEnvironment _environment;
        private readonly string _uploadFolder = "uploads"; // relative to wwwroot

        public LocalFileStorageService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<Result<string>> UploadFileAsync(FileUploadRequest fileUploadRequest)
        {
            var uploadsPath = Path.Combine(_environment.WebRootPath, _uploadFolder);

            if (!Directory.Exists(uploadsPath))
            {
                Directory.CreateDirectory(uploadsPath);
            }

            var fileExtension = Path.GetExtension(fileUploadRequest.FileName);
            var fileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadsPath, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await stream.WriteAsync(fileUploadRequest.Content, 0, fileUploadRequest.Content.Length);

            var fullFilePath = Path.Combine(_uploadFolder, fileName).Replace("\\", "/");
            return Result.Ok(fullFilePath);
        }

        public Task<Result> DeleteFileAsync(string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                return Task.FromResult(Result.Fail(new Error("Provided file path is empty.")));
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                return Task.FromResult(Result.Ok());
            }
            else
            {
                return Task.FromResult(Result.Fail(new Error("Cannot delete file because it does not exist.")));
            }
        }
    }
}
