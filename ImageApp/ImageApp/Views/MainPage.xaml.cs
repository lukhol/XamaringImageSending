using ImageTestApp.Services;
using ImageTestApp.ViewModels;
using Xamarin.Forms;

namespace ImageTestApp.Views
{
    public partial class MainPage : ContentPage
	{
		public MainPage()
		{
			InitializeComponent();

            IImageService imageServie = new ImageService();
            URLRepository urlRepository = new URLRepository(@"http://3f2a9006.ngrok.io/api/image");

            BindingContext = new PhotoViewModel(imageServie, urlRepository);
		}
	}
}
