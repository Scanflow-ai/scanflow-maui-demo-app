using ScanflowMauiDemoApp.Models;
using ScanflowMauiDemoApp.Services;
using ScanflowMauiDemoApp.ViewModels;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Maui.Networking;
using Microsoft.Maui.Storage;
#if ANDROID
using Scanflow.BarcodeCapture.Maui;
#endif

namespace ScanflowMauiDemoApp.Views;

/// <summary>
/// Home page that displays scanner options.
/// Uses IScanflowService for camera and license management.
/// </summary>
public partial class HomePage : ContentPage
{
    private readonly IScanflowService _scanflowService;
    
    public HomePage(IScanflowService scanflowService)
    {
        InitializeComponent();
        _scanflowService = scanflowService;
        BindingContext = new HomePageViewModel();
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine("[HomePage] OnAppearing");
        
        // Guard: Ensure initialization happened via splash screen
        if (!_scanflowService.IsFullyInitialized)
        {
            Console.WriteLine("[HomePage] NOT INITIALIZED - Redirecting to SplashPage");
            await RedirectToSplash();
        }
    }
    
    private async Task RedirectToSplash()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Force restart through splash screen
            Application.Current!.MainPage = new SplashPage(_scanflowService);
        });
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        Console.WriteLine("[HomePage] OnDisappearing");
    }

    private async void MainListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        try
        {
            if (e.CurrentSelection == null || e.CurrentSelection.Count == 0) 
                return;
            
            var result = e.CurrentSelection.FirstOrDefault() as ScanSelect;
            ((CollectionView)sender).SelectedItem = null;
            
            if (result == null) 
                return;
            
#if ANDROID
            // Check if camera is ready
            if (!_scanflowService.IsCameraReady)
            {
                await DisplayAlert("Not Ready", 
                    "Camera is not ready. Please wait or restart the app.", 
                    "OK");
                return;
            }
#endif
            
            // Navigate to scan page
            await Navigation.PushAsync(new ScanViewPage(_scanflowService, result));
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[HomePage] Navigation error: {ex.Message}");
            await DisplayAlert("Error", "Failed to open scanner.", "OK");
        }
    }
}
