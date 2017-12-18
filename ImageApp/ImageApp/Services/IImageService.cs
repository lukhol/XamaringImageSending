using System.Threading.Tasks;

namespace ImageTestApp.Services
{
    public interface IImageService
    {
        Task<bool> SendImageAsync(string uri, string filePath, string fileName);
        Task SendImageWithPluginAsync(string uri, string filePath, string fileName);
    }
}
