using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace SpotsFinderWeb.Controllers
{
    public class ImageController : ApiController
    {
        private string ImageDirectoryPath { get; }
        public ImageController()
        {
            ImageDirectoryPath = @"C:\Users\Lukasz\source\repos\ImageTestApp\ImageWeb\App_Data\Images\";
        }

        [Route("api/image")]
        public IHttpActionResult PostImage([FromUri]string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return BadRequest("guid cannot be empty. It is neccessary for uploading image.");

            var task = Request.Content.ReadAsStreamAsync();
            task.Wait();

            try
            {
                using (var fileStream = File.Create(ImageDirectoryPath + guid + ".jpg"))
                using ( var requestStream = task.Result)
                {
                    requestStream.CopyTo(fileStream);
                }
            }
            catch (Exception e)
            {
                //logger
                return InternalServerError(e);
            }

            return Ok<string>(guid);
        }

        [Route("api/image/multipart")]
        public async Task<IHttpActionResult> PostImageMultipart()
        {
            if (!Request.Content.IsMimeMultipartContent("form-data"))
            {
                return BadRequest();
            }

            try
            {
                var provider = new MultipartFormDataStreamProvider(ImageDirectoryPath);
                await Request.Content.ReadAsMultipartAsync(provider);

                foreach (var content in provider.Contents)
                {
                    var taskImage = content.ReadAsStreamAsync();
                    var imageName = content.Headers.ContentDisposition.Name;

                    var uploadPath = string.Format("{0}{1}{2}", ImageDirectoryPath, imageName, ".jpg");

                    using (var fileStream = File.Create(uploadPath))
                    using (var requestStream = taskImage.Result)
                    {
                        requestStream.CopyTo(fileStream);
                    }
                }

                return Ok();
            }
            catch (Exception e)
            {

                return InternalServerError(e);
            }
        }
    }
}
