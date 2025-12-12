using System;
using System.Threading.Tasks;
using ScanflowMauiDemoApp.Services;

namespace ScanflowMauiDemoApp.Views;

public partial class SplashPage : ContentPage
{
    private readonly IScanflowService _scanflowService;
    private bool _isInitializing;
    
    public SplashPage(IScanflowService scanflowService)
    {
        InitializeComponent();
        _scanflowService = scanflowService;
        
        // Hide status frame - show only logo during silent initialization
        statusFrame.IsVisible = false;
    }
    
    protected override async void OnAppearing()
    {
        base.OnAppearing();
        
        if (!_isInitializing)
        {
            await InitializeAppAsync();
        }
    }
    
    private async Task InitializeAppAsync()
    {
        _isInitializing = true;
        
        try
        {
            Console.WriteLine("[SplashPage] Starting initialization...");
            
            // Request camera permission first
            await RequestCameraPermissionAsync();
            
#if ANDROID
            // Android: Initialize Scanflow service with license validation
            bool success = await _scanflowService.InitializeAsync(cameraContainer);
            
            if (success)
            {
                Console.WriteLine("[SplashPage] Android: Initialization successful!");
                // Navigate directly to HomePage - no messages, no delay
                await NavigateToHome();
            }
            else
            {
                Console.WriteLine($"[SplashPage] Android: Initialization failed: {_scanflowService.LastError}");
                await ShowError(_scanflowService.LastError);
            }
#elif IOS
            // iOS: Skip license validation here - it happens in ScanViewPage
            // Just navigate directly after permission is granted
            Console.WriteLine("[SplashPage] iOS: Permission granted, navigating to camera...");
            
            // Mark service as ready (no license validation needed at this stage for iOS)
            _scanflowService.MarkAsInitialized();
            
            // Small delay to ensure UI is stable after permission prompt
            await Task.Delay(300);
            
            await NavigateToHome();
#else
            // Other platforms: Just navigate
            await NavigateToHome();
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SplashPage] ERROR: {ex.Message}");
            Console.WriteLine($"[SplashPage] Stack trace: {ex.StackTrace}");
            await ShowError(ex.Message);
        }
        finally
        {
            _isInitializing = false;
        }
    }
    
    private async Task RequestCameraPermissionAsync()
    {
        try
        {
            Console.WriteLine("[SplashPage] Checking camera permission...");
            var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
            
            if (status != PermissionStatus.Granted)
            {
                Console.WriteLine("[SplashPage] Requesting camera permission...");
                status = await Permissions.RequestAsync<Permissions.Camera>();
                
#if IOS
                // iOS needs time to process permission change and stabilize UI
                Console.WriteLine("[SplashPage] iOS: Waiting for permission processing...");
                await Task.Delay(800);
#endif
                
                Console.WriteLine($"[SplashPage] Permission result: {status}");
                
                if (status != PermissionStatus.Granted)
                {
                    Console.WriteLine("[SplashPage] Camera permission denied by user");
                    await ShowError("Camera permission is required for scanning.");
                    return;
                }
            }
            
            Console.WriteLine("[SplashPage] Camera permission granted");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[SplashPage] Permission request error: {ex.Message}");
            Console.WriteLine($"[SplashPage] Stack trace: {ex.StackTrace}");
            await ShowError($"Permission error: {ex.Message}");
            throw;
        }
    }
    
    private async Task ShowError(string error)
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
            // Show error state only when there's a problem
            statusFrame.IsVisible = true;
            loadingState.IsVisible = false;
            successState.IsVisible = false;
            errorState.IsVisible = true;
            
            errorLabel.Text = string.IsNullOrEmpty(error) 
                ? "An unknown error occurred" 
                : error;
            
            statusFrame.BackgroundColor = Color.FromArgb("#FFEBEE");
            statusFrame.BorderColor = Color.FromArgb("#F44336");
        });
        
        _isInitializing = false;
    }
    
    private async void OnRetryClicked(object sender, EventArgs e)
    {
        // Hide error and retry silently
        statusFrame.IsVisible = false;
        await InitializeAppAsync();
    }
    
    private async Task NavigateToHome()
    {
        await MainThread.InvokeOnMainThreadAsync(() =>
        {
#if IOS
            // iOS: Navigate directly to camera/scan page (skip HomePage)
            Console.WriteLine("[SplashPage] iOS: Navigating directly to scan page");
            
            // Create default scan configuration (Any mode - scans both QR and Barcodes)
            var defaultScanConfig = new Models.ScanSelect
            {
                Name = "Any",
                Image = "any"
            };
            
            var scanPage = new ScanViewPage(_scanflowService, defaultScanConfig);
            
            Application.Current!.MainPage = new NavigationPage(scanPage)
            {
                BarBackgroundColor = Color.FromArgb("#0C54C5"),
                BarTextColor = Colors.White
            };
#else
            // Android: Navigate to HomePage as usual
            var homePage = new HomePage(_scanflowService);
            
            Application.Current!.MainPage = new NavigationPage(homePage)
            {
                BarBackgroundColor = Color.FromArgb("#0C54C5"),
                BarTextColor = Colors.White
            };
#endif
        });
    }
}
