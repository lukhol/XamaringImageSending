using PCLStorage;
using Plugin.FileUploader;
using Plugin.FileUploader.Abstractions;
using System;
using System.IO;
using System.Net;
using System.Threading.Tasks;

namespace ImageTestApp.Services
{
    public class ImageService : IImageService
    {
        public async Task<bool> SendImageAsync(string uri, string filePath, string fileName)
        {
            var currentFileSystem = FileSystem.Current;
            var imageFileInfo = currentFileSystem.GetFileFromPathAsync(filePath);
            var result = imageFileInfo.Result;
            var imageStreamAsATask = result.OpenAsync(PCLStorage.FileAccess.Read);

            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            request.AllowWriteStreamBuffering = false;
            request.ContentType = "application/json";

            try
            {
                using (var imageStream = imageStreamAsATask.Result)
                {
                    request.ContentLength = imageStream.Length;

                    using (var requestStream = request.GetRequestStream())
                    {
                        imageStream.CopyTo(requestStream);
                    }
                }

                using (var response = request.GetResponse())
                using (var responseStream = response.GetResponseStream())
                using (var reader = new StreamReader(responseStream))
                {
                    var responseGuid = reader.ReadToEnd();
                    responseGuid = responseGuid.Replace("\"", "");
                    return responseGuid.Equals(fileName);
                }
            }
            catch (WebException ex)
            {
                if (ex.Status == WebExceptionStatus.Timeout)
                {
                    //LogError(ex, "Timeout while uploading '{0}'", fileName);
                }
                else
                {
                    //LogError(ex, "Error while uploading '{0}'", fileName);
                }

                return false;
            }
            catch (Exception ex2)
            {
                return false;
            }
        }

        public async Task SendImageWithPluginAsync(string url, string filePath, string fileName)
        {
            var currentCrossFileUploader = CrossFileUploader.Current;
            var imagePathItem = new FilePathItem("test", filePath);

            currentCrossFileUploader.FileUploadCompleted += (s,e) =>
            {

            };

            currentCrossFileUploader.FileUploadError += (s, e) =>
            {

            };

            await currentCrossFileUploader.UploadFileAsync(url, imagePathItem);
        }
    }
}
