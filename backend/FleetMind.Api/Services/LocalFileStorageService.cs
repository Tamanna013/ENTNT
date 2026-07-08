using System;
using System.IO;
using System.Threading.Tasks;
using FleetMind.Api.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Options;

namespace FleetMind.Api.Services
{
    public class LocalFileStorageService : IFileStorageService
    {
        private readonly FileStorageOptions _options;
        private readonly string _basePath;

        public LocalFileStorageService(IOptions<FileStorageOptions> options, IWebHostEnvironment env)
        {
            _options = options.Value;
            _basePath = Path.Combine(env.ContentRootPath, _options.LocalStoragePath);
            
            if (!Directory.Exists(_basePath))
            {
                Directory.CreateDirectory(_basePath);
            }
        }

        public async Task<string> SaveAsync(Stream fileStream, string originalFileName)
        {
            var extension = Path.GetExtension(originalFileName);
            var storedFileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(_basePath, storedFileName);

            using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None);
            await fileStream.CopyToAsync(stream);

            return storedFileName;
        }

        public Task<Stream> GetFileStreamAsync(string storedFileName)
        {
            var filePath = Path.Combine(_basePath, storedFileName);
            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException("The requested file was not found on disk.", filePath);
            }

            Stream stream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            return Task.FromResult(stream);
        }

        public Task DeleteAsync(string storedFileName)
        {
            var filePath = Path.Combine(_basePath, storedFileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
            return Task.CompletedTask;
        }
    }
}
