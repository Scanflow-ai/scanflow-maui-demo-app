using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using UIKit;
using Scanflow.BarcodeCapture.Maui.iOS;
using Foundation;
using ObjCRuntime;

namespace ScanflowMauiDemoApp.Platforms.iOS
{
    // Delegate for camera events - Implements the protocol using NSObject
    [Register("IOSCameraDelegate")]
    internal class IOSCameraDelegate : NSObject
    {
        private IOSCameraView? _mauiView;

        // Default constructor required for NSObject
        public IOSCameraDelegate()
        {
        }

        // Constructor with NativeHandle for Objective-C runtime
        protected IOSCameraDelegate(NativeHandle handle) : base(handle)
        {
        }

        public IOSCameraDelegate(IOSCameraView mauiView)
        {
            _mauiView = mauiView;
        }

        [Export("capturedOutput:::::")]
        public void CapturedOutput(string result, ScannerType codeType, string[]? results, UIKit.UIImage? processedImage, global::CoreLocation.CLLocation? location)
        {
            Console.WriteLine($"[IOSCameraDelegate] Scan result: {result}");
            _mauiView?.TriggerScanResult(result);
        }

        [Export("presentCameraPermissionsDeniedAlert")]
        public void PresentCameraPermissionsDeniedAlert()
        {
            Console.WriteLine("[IOSCameraDelegate] Camera permission denied");
        }

        [Export("locationAccessDeniedAlert")]
        public void LocationAccessDeniedAlert()
        {
            Console.WriteLine("[IOSCameraDelegate] Location permission denied");
        }

        [Export("presentVideoConfigurationErrorAlert")]
        public void PresentVideoConfigurationErrorAlert()
        {
            Console.WriteLine("[IOSCameraDelegate] Video configuration error");
        }

        [Export("sessionRunTimeErrorOccurred")]
        public void SessionRunTimeErrorOccurred()
        {
            Console.WriteLine("[IOSCameraDelegate] Session runtime error");
        }

        [Export("sessionWasInterrupted:")]
        public void SessionWasInterrupted(bool resumeManually)
        {
            Console.WriteLine($"[IOSCameraDelegate] Session interrupted (resume manually: {resumeManually})");
        }

        [Export("sessionWasInterrupted")]
        public void SessionWasInterrupted()
        {
            Console.WriteLine("[IOSCameraDelegate] Session interrupted");
        }

        [Export("captured:::")]
        public void Captured(global::CoreVideo.CVPixelBuffer originalframe, global::CoreGraphics.CGRect overlayFrame, UIKit.UIImage croppedImage)
        {
            // Frame captured
        }

        [Export("showAlert::")]
        public void ShowAlert(string? title, string message)
        {
            Console.WriteLine($"[IOSCameraDelegate] Alert: {title ?? "Alert"} - {message}");
        }
    }

    // Delegate for license validation - Implements the protocol using NSObject
    [Register("IOSLicenseDelegate")]
    internal class IOSLicenseDelegate : NSObject
    {
        private IOSCameraView? _mauiView;
        private ScanflowBarCodeManager? _barcodeManager;

        // Default constructor required for NSObject
        public IOSLicenseDelegate()
        {
        }

        // Constructor with NativeHandle for Objective-C runtime
        protected IOSLicenseDelegate(NativeHandle handle) : base(handle)
        {
        }

        public IOSLicenseDelegate(IOSCameraView mauiView, ScanflowBarCodeManager barcodeManager)
        {
            _mauiView = mauiView;
            _barcodeManager = barcodeManager;
        }

        [Export("licenceOnSuccessWithResponse:")]
        public void LicenceOnSuccessWithResponse(string response)
        {
            Console.WriteLine($"[IOSLicenseDelegate] License success: {response}");
            
            // Start camera session after successful validation
            _barcodeManager?.StartSession();
            Console.WriteLine("[IOSLicenseDelegate] Camera session started");
            
            _mauiView?.TriggerLicenseSuccess(response);
        }

        [Export("licenceOnFailureWithError:")]
        public void LicenceOnFailureWithError(string error)
        {
            Console.WriteLine($"[IOSLicenseDelegate] License failure: {error}");
            _mauiView?.TriggerLicenseFailure(error);
        }
    }

    /// <summary>
    /// Custom MAUI view that hosts the iOS Scanflow camera
    /// Similar to Android CameraPreview
    /// </summary>
    public class IOSCameraView : View
    {
        public static readonly BindableProperty LicenseKeyProperty =
            BindableProperty.Create(nameof(LicenseKey), typeof(string), typeof(IOSCameraView), string.Empty);

        public static readonly BindableProperty ScannerModeProperty =
            BindableProperty.Create(nameof(ScannerMode), typeof(int), typeof(IOSCameraView), 2); // Default: Any

        public string LicenseKey
        {
            get => (string)GetValue(LicenseKeyProperty);
            set => SetValue(LicenseKeyProperty, value);
        }

        public int ScannerMode
        {
            get => (int)GetValue(ScannerModeProperty);
            set => SetValue(ScannerModeProperty, value);
        }

        // Events for scan results
        public event EventHandler<string>? OnScanResult;
        public event EventHandler<string>? OnLicenseSuccess;
        public event EventHandler<string>? OnLicenseFailure;

        // Internal method to trigger scan result
        internal void TriggerScanResult(string result)
        {
            OnScanResult?.Invoke(this, result);
        }

        internal void TriggerLicenseSuccess(string response)
        {
            OnLicenseSuccess?.Invoke(this, response);
        }

        internal void TriggerLicenseFailure(string error)
        {
            OnLicenseFailure?.Invoke(this, error);
        }
    }

    /// <summary>
    /// Custom UIView wrapper that initializes the camera when laid out
    /// </summary>
    public class ScanflowCameraContainer : UIView
    {
        private ScanflowBarCodeManager? _barcodeManager;
        private IOSCameraDelegate? _cameraDelegate;
        private IOSLicenseDelegate? _licenseDelegate;
        private readonly IOSCameraView _mauiView;
        private bool _isInitialized;

        public ScanflowCameraContainer(IOSCameraView mauiView)
        {
            _mauiView = mauiView;
            BackgroundColor = UIColor.Black;
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            
            // Initialize camera only once and only when we have proper bounds
            if (!_isInitialized && Bounds.Width > 0 && Bounds.Height > 0)
            {
                _isInitialized = true;
                Console.WriteLine($"[ScanflowCameraContainer] LayoutSubviews - Bounds: {Bounds}");
                InitializeCamera();
            }
        }

        private void InitializeCamera()
        {
            try
            {
                Console.WriteLine("[ScanflowCameraContainer] Initializing camera");

                // Get scanner type
                ScannerType scannerType = GetScannerType(_mauiView.ScannerMode);
                Console.WriteLine($"[ScanflowCameraContainer] Scanner type: {scannerType}");

                // Create overlay appearance - Square provides a bigger scanning area
                OveylayViewApperance overlayApperance = OveylayViewApperance.Square;

                // Create the barcode manager with this view as the container
                // All corners in green for standard scanner look
                _barcodeManager = new ScanflowBarCodeManager(
                    this,
                    scannerType,
                    overlayApperance,
                    overCropNeed: false,
                    leftTopArc: UIColor.Green,       // Top-left corner
                    leftDownArc: UIColor.Green,      // Bottom-left corner  
                    rightTopArc: UIColor.Green,      // Top-right corner
                    rightDownArc: UIColor.Green,     // Bottom-right corner
                    locationNeed: false
                );

                Console.WriteLine("[ScanflowCameraContainer] Barcode manager created");

                // Create and set delegates
                _cameraDelegate = new IOSCameraDelegate(_mauiView);
                _licenseDelegate = new IOSLicenseDelegate(_mauiView, _barcodeManager);

                // Use WeakDelegate properties for protocol-based delegates
                _barcodeManager.WeakDelegate = _cameraDelegate;
                _barcodeManager.WeakLicenceDelegate = _licenseDelegate;

                Console.WriteLine("[ScanflowCameraContainer] Delegates set");

                // Start license validation
                if (!string.IsNullOrEmpty(_mauiView.LicenseKey))
                {
                    _barcodeManager.ValidateLicense(_mauiView.LicenseKey);
                    Console.WriteLine("[ScanflowCameraContainer] License validation started");
                }
                else
                {
                    Console.WriteLine("[ScanflowCameraContainer] WARNING: No license key provided!");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScanflowCameraContainer] Error initializing camera: {ex.Message}");
                Console.WriteLine($"[ScanflowCameraContainer] Stack trace: {ex.StackTrace}");
            }
        }

        private ScannerType GetScannerType(int mode)
        {
            return mode switch
            {
                0 => ScannerType.Qrcode,
                1 => ScannerType.Barcode,
                2 => ScannerType.Any,
                3 => ScannerType.OneOfMany,
                4 => ScannerType.BatchInventory,
                _ => ScannerType.Any
            };
        }

        public void StopCamera()
        {
            _barcodeManager?.StopSession();
            _barcodeManager = null;
            _cameraDelegate = null;
            _licenseDelegate = null;
        }

        public void StartCamera()
        {
            _barcodeManager?.StartSession();
        }

        public void ToggleFlashlight(bool enable)
        {
            _barcodeManager?.FlashLight(enable);
        }
    }

    /// <summary>
    /// Handler for IOSCameraView - creates and manages the native iOS barcode scanner
    /// </summary>
    public class IOSCameraViewHandler : ViewHandler<IOSCameraView, ScanflowCameraContainer>
    {
        public static IPropertyMapper<IOSCameraView, IOSCameraViewHandler> PropertyMapper =
            new PropertyMapper<IOSCameraView, IOSCameraViewHandler>(ViewHandler.ViewMapper)
            {
            };

        public static CommandMapper<IOSCameraView, IOSCameraViewHandler> CommandMapper =
            new CommandMapper<IOSCameraView, IOSCameraViewHandler>(ViewHandler.ViewCommandMapper)
            {
            };

        public IOSCameraViewHandler() : base(PropertyMapper, CommandMapper)
        {
        }

        protected override ScanflowCameraContainer CreatePlatformView()
        {
            Console.WriteLine("[IOSCameraViewHandler] Creating native view");
            return new ScanflowCameraContainer(VirtualView);
        }

        protected override void ConnectHandler(ScanflowCameraContainer platformView)
        {
            base.ConnectHandler(platformView);
            Console.WriteLine("[IOSCameraViewHandler] ConnectHandler called");
        }

        protected override void DisconnectHandler(ScanflowCameraContainer platformView)
        {
            Console.WriteLine("[IOSCameraViewHandler] DisconnectHandler called");
            
            try
            {
                platformView?.StopCamera();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[IOSCameraViewHandler] Error in DisconnectHandler: {ex.Message}");
            }

            base.DisconnectHandler(platformView);
        }

        public void StartSession()
        {
            PlatformView?.StartCamera();
            Console.WriteLine("[IOSCameraViewHandler] Session started");
        }

        public void StopSession()
        {
            PlatformView?.StopCamera();
            Console.WriteLine("[IOSCameraViewHandler] Session stopped");
        }

        public void EnableFlashlight(bool enable)
        {
            PlatformView?.ToggleFlashlight(enable);
            Console.WriteLine($"[IOSCameraViewHandler] Flashlight {(enable ? "ON" : "OFF")}");
        }
    }
}

