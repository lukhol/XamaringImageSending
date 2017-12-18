using PCLStorage;
using Plugin.FileUploader;
using Plugin.FileUploader.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace ImageTestApp.Services
{
    public class ImageService : IImageService
    {
        public Action<double> ProgressOnSingleImage;

        private HttpClient httpClient;
        public ImageService(HttpClient httpClient)
        {
            this.httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
        }

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
                    Device.StartTimer(TimeSpan.FromMilliseconds(100), () =>
                    {
                        if (!imageStream.CanRead)
                            return false;

                        double percentageUploadingPosition = (double)imageStream.Position / (double)imageStream.Length;
                        ProgressOnSingleImage?.Invoke(percentageUploadingPosition);

                        if (imageStream.Position >= imageStream.Length - 1)
                            return false;

                        return true;
                    });

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

        public async Task<bool> SendImageWithPluginAsync(string url, string filePath, string fileName)
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

            return false;
        }

        public async Task<bool> SendMultipleImagesWithMultipartAsync(string uri, IDictionary<string, string> pathAndFileNameDictionary)
        {
            try
            {
                MultipartFormDataContent form = new MultipartFormDataContent();

                foreach(var filePath in pathAndFileNameDictionary)
                {
                    var currentFileSystem = FileSystem.Current;
                    var imageFileInfo = currentFileSystem.GetFileFromPathAsync(filePath.Key);
                    var result = imageFileInfo.Result;
                    var imageStreamAsATask = result.OpenAsync(PCLStorage.FileAccess.Read);

                    form.Add(new StreamContent(imageStreamAsATask.Result), filePath.Value);
                }

                HttpResponseMessage response = await httpClient.PostAsync(uri, form);

                response.EnsureSuccessStatusCode();

                if (response.StatusCode.ToString() != "OK")
                    return false;
                else
                    return true;
            }
            catch (Exception e)
            {

                return false;
            }
        }
    }
}
