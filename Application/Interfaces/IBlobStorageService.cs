namespace Application.Interfaces
{
    public interface IBlobStorageService
    {
        Task<Uri> UploadFileAsync(string containerName, string fileName, Stream fileStream, string contentType, CancellationToken cancellationToken = default);
        Task DeleteFileAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
        Task<Uri> GetFileUriAsync(string containerName, string fileName, CancellationToken cancellationToken = default);
    }
}
