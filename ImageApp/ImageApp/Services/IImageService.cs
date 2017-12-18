using System.Collections.Generic;
using System.Threading.Tasks;

namespace ImageTestApp.Services
{
    public interface IImageService
    {
        Task<bool> SendImageAsync(string uri, string filePath, string fileName);
        Task<bool> SendImageWithPluginAsync(string uri, string filePath, string fileName);
        Task<bool> SendMultipleImagesWithMultipartAsync(string uri, IDictionary<string, string> pathAndFileNameDictionary);
    }
}
