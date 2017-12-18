using ImageTestApp.Services;
using ImageTestApp.ViewModels;
using System.Net.Http;
using Xamarin.Forms;

namespace ImageTestApp.Views
{
    public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

            HttpClient httpClient = new HttpClient();
            IImageService imageServie = new ImageService(httpClient);
            URLRepository urlRepository = new URLRepository(@"http://3f2a9006.ngrok.io/");

            BindingContext = new PhotoViewModel(imageServie, urlRepository);
		}
	}
}
