using System.Linq;
using CommunityToolkit.Maui.Alerts;
using ScanflowMauiDemoApp.Models;
using ScanflowMauiDemoApp.Services;
#if ANDROID
using Scanflow.BarcodeCapture.Maui;
using Scanflow.BarcodeCapture.Maui.Models;
#elif IOS
using ScanflowMauiDemoApp.Platforms.iOS;
#endif

namespace ScanflowMauiDemoApp.Views;

/// <summary>
/// Scan view page that displays the camera and handles barcode scanning.
/// Uses IScanflowService for camera management.
/// </summary>
public partial class ScanViewPage : ContentPage
{
    private readonly IScanflowService _scanflowService;
    private readonly ScanSelect _scanConfig;
    private bool _isTorch = false;
    private bool _isScanProcessing = false;
    
#if ANDROID
    private CameraPreview? _cameraPreview;
#elif IOS
    private IOSCameraView? _iosCameraView;
#endif

    public ScanViewPage(IScanflowService scanflowService, ScanSelect scanConfig)
    {
        try
        {
            InitializeComponent();
            
            _scanflowService = scanflowService;
            _scanConfig = scanConfig;
            scanTitle.Text = scanConfig.Name;
            
#if ANDROID
            SetupCamera();
#elif IOS
            SetupIOSCamera();
#endif
        }
        catch (Exception e)
        {
            Console.WriteLine($"[ScanViewPage] Constructor error: {e.Message}");
        }
    }

#if ANDROID
    private void SetupCamera()
    {
        try
        {
            // Get camera from service
            var camera = _scanflowService.GetCameraForScanning();
            
            if (camera is CameraPreview cameraPreview)
            {
                _cameraPreview = cameraPreview;
                
                Console.WriteLine("[ScanViewPage] Setting up camera from service");
                
                // Subscribe to scan result events
                _cameraPreview.OnScanResult += OnScanResult;
                _cameraPreview.OnLicenceOnSuccessWithResponse += OnLicenseSuccess;
                _cameraPreview.OnLicenceOnFailureWithError += OnLicenseFailure;
                
                // Add camera to the backdrop (full screen)
                backdrop.Children.Insert(0, _cameraPreview);
                
                // Ensure torch is off initially
                _cameraPreview.EnableTorch(false);
                
                Console.WriteLine("[ScanViewPage] Camera setup complete");
            }
            else
            {
                Console.WriteLine("[ScanViewPage] ERROR: Camera not available from service!");
                MainThread.BeginInvokeOnMainThread(async () =>
                {
                    await DisplayAlert("Error", "Camera is not available. Please restart the app.", "OK");
                    await Navigation.PopAsync();
                });
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] SetupCamera error: {ex.Message}");
        }
    }
#endif

#if IOS
    private void SetupIOSCamera()
    {
        try
        {
            Console.WriteLine("[ScanViewPage] iOS: Setting up camera...");
            
            // Create iOS camera view (similar to Android CameraPreview)
            _iosCameraView = new IOSCameraView
            {
                LicenseKey = _scanflowService.LicenseKey,
                ScannerMode = GetIOSScannerMode(),
                HorizontalOptions = LayoutOptions.Fill,
                VerticalOptions = LayoutOptions.Fill,
                BackgroundColor = Colors.Black
            };
            
            // Subscribe to events
            _iosCameraView.OnScanResult += OnIOSScanResult;
            _iosCameraView.OnLicenseSuccess += OnIOSLicenseSuccess;
            _iosCameraView.OnLicenseFailure += OnIOSLicenseFailure;
            
            // Add camera to backdrop (just like Android!)
            backdrop.Children.Insert(0, _iosCameraView);
            
            Console.WriteLine("[ScanViewPage] iOS: Camera view added to backdrop");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] iOS SetupCamera error: {ex.Message}");
            Console.WriteLine($"[ScanViewPage] iOS Stack trace: {ex.StackTrace}");
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                await DisplayAlert("Error", $"Failed to initialize camera: {ex.Message}", "OK");
                await Navigation.PopAsync();
            });
        }
    }
    
    private int GetIOSScannerMode()
    {
        // Map scan config to iOS scanner mode
        string scanName = _scanConfig.Name.ToLower();
        
        if (scanName.Contains("qr"))
            return 0; // Qrcode
        else if (scanName.Contains("barcode"))
            return 1; // Barcode
        else if (scanName.Contains("any"))
            return 2; // Any
        else if (scanName.Contains("batch"))
            return 4; // BatchInventory
        else
            return 2; // Default: Any
    }
    
    private void OnIOSScanResult(object? sender, string result)
    {
        try
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (!_isScanProcessing)
                {
                    _isScanProcessing = true;
                    Console.WriteLine($"[ScanViewPage] iOS: Scan result: {result}");
                    
                    await Toast.Make(result, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                    
                    _isScanProcessing = false;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] iOS: OnScanResult error: {ex.Message}");
        }
    }
    
    private void OnIOSLicenseSuccess(object? sender, string response)
    {
        Console.WriteLine($"[ScanViewPage] iOS: License success: {response}");
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make(response, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
        });
    }
    
    private void OnIOSLicenseFailure(object? sender, string error)
    {
        Console.WriteLine($"[ScanViewPage] iOS: License failure: {error}");
        MainThread.BeginInvokeOnMainThread(async () =>
        {
            await Toast.Make(error, CommunityToolkit.Maui.Core.ToastDuration.Long).Show();
            await DisplayAlert("License Error", error, "OK");
        });
    }
#endif

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        Console.WriteLine("[ScanViewPage] OnAppearing");
        
        // Guard: Ensure initialization happened via splash screen
        if (!_scanflowService.IsFullyInitialized)
        {
            Console.WriteLine("[ScanViewPage] NOT INITIALIZED - Redirecting to SplashPage");
            await RedirectToSplash();
            return;
        }
        
#if ANDROID
        // Restart camera when returning to this page
        _scanflowService.StartCamera();
#elif IOS
        // iOS camera auto-starts via handler, no action needed
        Console.WriteLine("[ScanViewPage] iOS: Camera session active");
#endif
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
        Console.WriteLine("[ScanViewPage] OnDisappearing");
        
#if ANDROID
        CleanupCamera();
#elif IOS
        CleanupIOSCamera();
#endif
    }

#if ANDROID
    private void CleanupCamera()
    {
        try
        {
            if (_cameraPreview != null)
            {
                // Unsubscribe from events
                _cameraPreview.OnScanResult -= OnScanResult;
                _cameraPreview.OnLicenceOnSuccessWithResponse -= OnLicenseSuccess;
                _cameraPreview.OnLicenceOnFailureWithError -= OnLicenseFailure;
                
                // Remove from visual tree
                if (backdrop.Children.Contains(_cameraPreview))
                {
                    backdrop.Children.Remove(_cameraPreview);
                }
                
                // Return camera to service
                _scanflowService.ReturnCamera();
                
                Console.WriteLine("[ScanViewPage] Camera cleaned up");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] CleanupCamera error: {ex.Message}");
        }
    }
#endif

#if IOS
    private void CleanupIOSCamera()
    {
        try
        {
            Console.WriteLine("[ScanViewPage] iOS: Cleaning up camera");
            
            if (_iosCameraView != null)
            {
                // Unsubscribe from events
                _iosCameraView.OnScanResult -= OnIOSScanResult;
                _iosCameraView.OnLicenseSuccess -= OnIOSLicenseSuccess;
                _iosCameraView.OnLicenseFailure -= OnIOSLicenseFailure;
                
                // Remove from backdrop
                if (backdrop.Children.Contains(_iosCameraView))
                {
                    backdrop.Children.Remove(_iosCameraView);
                }
                
                _iosCameraView = null;
                
                Console.WriteLine("[ScanViewPage] iOS: Camera cleaned up");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] iOS CleanupCamera error: {ex.Message}");
        }
    }
#endif

#if ANDROID
    private async void OnScanResult(object result)
    {
        try
        {
            var scanResult = result as ScanResult;
            if (scanResult == null) return;
            
            await MainThread.InvokeOnMainThreadAsync(async () =>
            {
                if (!_isScanProcessing)
                {
                    _isScanProcessing = true;
                    
                    await Toast.Make(scanResult.Text, CommunityToolkit.Maui.Core.ToastDuration.Short).Show();
                    
                    _isScanProcessing = false;
                }
            });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] OnScanResult error: {ex.Message}");
        }
    }
    
    private async void OnLicenseSuccess(string response)
    {
        Console.WriteLine($"[ScanViewPage] License success: {response}");
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make(response).Show();
        });
    }
    
    private async void OnLicenseFailure(string error)
    {
        Console.WriteLine($"[ScanViewPage] License failure: {error}");
        await MainThread.InvokeOnMainThreadAsync(async () =>
        {
            await Toast.Make(error).Show();
        });
    }
#endif

    private void FlashLight_Tapped(object sender, EventArgs e)
    {
        try
        {
            _isTorch = !_isTorch;
            torchImage.Source = _isTorch ? "flashon" : "flashoff";
            
#if ANDROID
            if (_cameraPreview != null)
            {
                _cameraPreview.EnableTorch(_isTorch);
            }
#elif IOS
            if (_iosCameraView?.Handler is IOSCameraViewHandler handler)
            {
                handler.EnableFlashlight(_isTorch);
                Console.WriteLine($"[ScanViewPage] iOS: Flashlight {(_isTorch ? "ON" : "OFF")}");
            }
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] Torch error: {ex.Message}");
        }
    }

    private void OnSettingsTapped(object sender, EventArgs e)
    {
        // Settings functionality can be added here
    }

    private void OnRetryValidation(object sender, EventArgs e)
    {
        try
        {
#if ANDROID
            _cameraPreview?.RetryValidationResult(_scanflowService.LicenseKey);
#elif IOS
            // iOS: Validation retry handled by the handler/SDK
            Console.WriteLine("[ScanViewPage] iOS: License retry not needed - auto-handled");
#endif
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ScanViewPage] Retry validation error: {ex.Message}");
        }
    }
}
