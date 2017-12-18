using ImageTestApp.Views;
using Plugin.Media;
using Xamarin.Forms;

namespace ImageTestApp
{
    public partial class App : Application
	{
		public App ()
		{
			InitializeComponent();
            CrossMedia.Current.Initialize();

            MainPage = new MainPage();
		}

		protected override void OnStart ()
		{
			// Handle when your app starts
		}

		protected override void OnSleep ()
		{
			// Handle when your app sleeps
		}

		protected override void OnResume ()
		{
			// Handle when your app resumes
		}
	}
}
