using System;
using System.Collections.Generic;
using System.Text;

namespace ImageTestApp.Services
{
    public class URLRepository
    {
        private string ImageUri { get; }

        public URLRepository(string imageUri)
        {
            ImageUri = imageUri ?? throw new ArgumentNullException(nameof(ImageUri));
        }

        public string GetImageUrl(string guid)
        {
            return string.Format("{0}?guid={1}", ImageUri, guid);
        }
    }
}
