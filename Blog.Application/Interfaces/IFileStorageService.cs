using Blog.Application.Files.Dtos;
using FluentResults;

namespace Blog.Application.Interfaces
{
    public interface IFileStorageService
    {
        Task<Result<string>> UploadFileAsync(FileUploadRequest fileUploadRequest);
        Task<Result> DeleteFileAsync(string filePath);
    }
}
