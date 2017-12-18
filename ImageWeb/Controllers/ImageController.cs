using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http;

namespace SpotsFinderWeb.Controllers
{
    public class ImageController : ApiController
    {
        private string ImageDirectoryPath { get; }
        public ImageController()
        {
            ImageDirectoryPath = @"C:\Projects\PBL\server-webapi\SpotsFinderWeb\SpotsFinderWeb\App_Data\Images\";
        }

        [Route("api/image")]
        public IHttpActionResult PostImage([FromUri]string guid)
        {
            if (string.IsNullOrEmpty(guid))
                return BadRequest("guid cannot be empty. It is neccessary for uploading image.");

            var task = Request.Content.ReadAsStreamAsync();
            task.Wait();
            Stream requestStream = task.Result;

            try
            {
                Stream fileStream = File.Create(ImageDirectoryPath + guid + ".jpg");
                requestStream.CopyTo(fileStream);

                fileStream.Close();
                requestStream.Close();
            }
            catch (Exception e)
            {
                //logger
                return InternalServerError(e);
            }

            return Ok<string>(guid);
        }
    }
}
