using ImageTestApp.Services;
using Plugin.Media;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Input;
using Xamarin.Forms;

namespace ImageTestApp.ViewModels
{
    public class PhotoViewModel : BaseViewModel
    {
        private IImageService ImageService { get; }
        private URLRepository URLRepository { get; }

        private IDictionary<string, string> pathDictionary = new Dictionary<string, string>();

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

        private string imageToSendCount;
        public string ImageToSendCount
        {
            get => "Images to send count: " + imageToSendCount;
            set
            {
                imageToSendCount = value;
                OnPropertyChanged();
            }
        }

        private double uploadingProgress;
        public double UploadingProgress
        {
            get => uploadingProgress;
            set
            {
                uploadingProgress = value;
                OnPropertyChanged();
            }
        }

        private void UpdateProgress(double value)
        {
            UploadingProgress = value;
        }

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
                Name = Guid.NewGuid().ToString(),
                PhotoSize = Plugin.Media.Abstractions.PhotoSize.Medium
            });

            if (file == null)
                return;

            ImagePath = file.Path;

            pathDictionary.Add(file.Path, GetImageNameFromPath(file.Path));
            ImageToSendCount = pathDictionary.Count.ToString();
        });

        public ICommand SendPhotoByStreamCommand => new Command(() =>
        {
            IsBusy = true;

            string guid;

            try
            {
                guid = GetImageNameFromPath(imagePath);
            }
            catch (Exception e)
            {
                App.Current.MainPage.DisplayAlert("Ups...", "File path is probably incorrect. Plese try again after take photo.", "Ok");
                IsBusy = false;
                return;
            }

            Task.Run(async () =>
            {
                var imageService = (ImageService)ImageService;
                imageService.ProgressOnSingleImage += UpdateProgress;

                var result = await imageService.SendImageAsync(URLRepository.GetImageUrl(guid), imagePath, guid);

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

        public ICommand SendMultiplePhotoByMultipartCommand => new Command(() =>
        {
            IsBusy = true;

            Task.Run(async () =>
            {
                var result = await ImageService.SendMultipleImagesWithMultipartAsync(URLRepository.GetMultipartImageUrl(), pathDictionary);

                Device.BeginInvokeOnMainThread(async () =>
                {
                    if (result)
                    {
                        await App.Current.MainPage.DisplayAlert("SUCCESS", "Image uploaded succesfully.", "Ok");
                        pathDictionary.Clear();
                        ImageToSendCount = pathDictionary.Count.ToString();
                    }
                    else
                    {
                        await App.Current.MainPage.DisplayAlert("ERROR", "Error occured during uploading image.", "Ok");
                    }

                    IsBusy = false;
                });
            });
        });

        private string GetImageNameFromPath(string imagePath)
        {
            var fileName = imagePath.Split('/').Last();
            var guid = fileName.Substring(0, fileName.LastIndexOf("."));
            return guid;
        }
    }
}
