using System.IO;
using System.Threading.Tasks;

namespace FleetMind.Api.Services
{
    public interface IFileStorageService
    {
        Task<string> SaveAsync(Stream fileStream, string originalFileName);
        Task<Stream> GetFileStreamAsync(string storedFileName);
        Task DeleteAsync(string storedFileName);
    }
}
