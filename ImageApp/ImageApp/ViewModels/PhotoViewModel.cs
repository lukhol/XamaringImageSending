using ImageTestApp.Services;
using Plugin.Media;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ImageTestApp.ViewModels
{
    public class PhotoViewModel : BaseViewModel
    {
        private IImageService ImageService { get; }
        private URLRepository URLRepository { get; }

        public PhotoViewModel(IImageService imageService, URLRepository urlRepository)
        {
            ImageService = imageService ?? throw new ArgumentNullException(nameof(ImageService));
            URLRepository = urlRepository ?? throw new ArgumentException(nameof(URLRepository));
        }

        public ICommand GetPhotoCommand => new Command(async () => 
        {
            await CrossMedia.Current.Initialize();

            if (!CrossMedia.Current.IsCameraAvailable || !CrossMedia.Current.IsTakePhotoSupported)
            {
                await App.Current.MainPage.DisplayAlert("No Camera", ":( No camera available.", "OK");
                return;
            }

            var file = await CrossMedia.Current.TakePhotoAsync(new Plugin.Media.Abstractions.StoreCameraMediaOptions
            {
                Directory = "image_tests",
                Name = Guid.NewGuid().ToString()
            });

            if (file == null)
                return;

            ImagePath = file.Path;
        });

        public ICommand SendPhotoCommand => new Command(() =>
        {
            IsBusy = true;

            string guid;

            try
            {
                var fileName = imagePath.Split('/').Last();
                guid = fileName.Substring(0, fileName.LastIndexOf("."));
            }
            catch (Exception e)
            {
                App.Current.MainPage.DisplayAlert("Ups...", "File path is probably incorrect. Plese try again after take photo.", "Ok");
                IsBusy = false;
                return;
            }


            Task.Run(async () =>
            {
                //var result = await ImageService.SendImageAsync(URLRepository.GetImageUrl(guid), imagePath, guid);
                var result = true;
                await ImageService.SendImageWithPluginAsync(URLRepository.GetImageUrl(guid), imagePath, guid);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (result)
                        await App.Current.MainPage.DisplayAlert("SUCCESS", "Image uploaded succesfully.", "Ok");
                    else
                        await App.Current.MainPage.DisplayAlert("ERROR", "Error occured during uploading image.", "Ok");

                    IsBusy = false;
                });
            });
        });

        private string imagePath;
        public string ImagePath
        {
            get => imagePath;
            set
            {
                imagePath = value;
                OnPropertyChanged();
            }
        }
    }
}
