using ScanflowMauiDemoApp.Services;
using ScanflowMauiDemoApp.Views;

namespace ScanflowMauiDemoApp
{
    public partial class App : Application
    {
        public App(IScanflowService scanflowService)
        {
            InitializeComponent();

            // Start with SplashPage for initialization
            MainPage = new SplashPage(scanflowService);
        }
    }
}
