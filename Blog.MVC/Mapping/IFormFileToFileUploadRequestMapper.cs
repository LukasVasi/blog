using Blog.Application.Files.Dtos;

namespace Blog.MVC.Mapping
{
    internal static class IFormFileToFileUploadRequestMapper
    {
        /// <summary>
        /// Maps a <see cref="IFormFile"/> object used only in MVC to the application layer
        /// <see cref="FileUploadRequest"/> object.
        /// </summary>
        /// <param name="formFile">
        /// The original form file.
        /// </param>
        /// <returns>
        /// The file information converted to <see cref="FileUploadRequest"/>.
        /// </returns>
        public static async Task<FileUploadRequest> ToFileUploadRequestAsync(this IFormFile formFile)
        {
            using var ms = new MemoryStream();
            await formFile.CopyToAsync(ms);

            return new FileUploadRequest
            {
                FileName = formFile.FileName,
                ContentType = formFile.ContentType,
                Content = ms.ToArray()
            };
        }
    }
}
