using Microsoft.Maui.Handlers;
using Microsoft.Maui;
using UIKit;
using Scanflow.BarcodeCapture.Maui.iOS;
using Foundation;
using ObjCRuntime;

namespace ScanflowMauiDemoApp.Platforms.iOS
{
    internal static class ScanflowIosLog
    {
        public static void Info(string component, string step, string message)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Scanflow.iOS][{component}] {step} | {message}");
        }

        public static void LicenseKey(string component, string step, string? licenseKey)
        {
            if (string.IsNullOrEmpty(licenseKey))
            {
                Info(component, step, "LicenseKey: EMPTY or NULL");
                return;
            }

            string masked = licenseKey.Length <= 8
                ? licenseKey
                : $"{licenseKey.Substring(0, 4)}...{licenseKey.Substring(licenseKey.Length - 4)} (len={licenseKey.Length})";

            Info(component, step, $"LicenseKey (masked): {masked}");
            Info(component, step, $"LicenseKey (full): {licenseKey}");
        }

        public static void Error(string component, string step, string message, Exception? ex = null)
        {
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Scanflow.iOS][{component}] ERROR {step} | {message}");
            if (ex != null)
            {
                Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [Scanflow.iOS][{component}] ERROR {step} | Exception: {ex}");
            }
        }
    }

    /// <summary>
    /// Logs whether a native instance responds to ObjC selectors.
    /// Use device console output to compare binding vs bundled framework.
    /// </summary>
    internal static class ScanflowSelectorDiagnostics
    {
        public static void LogAll(NSObject manager, string label)
        {
            Log(manager, label, "setLicenceDelegate:");
            Log(manager, label, "licenceDelegate");
            Log(manager, label, "setLicenseDelegate:");
            Log(manager, label, "licenseDelegate");
            Log(manager, label, "validateLicense:");
            Log(manager, label, "setDelegate:");
            Log(manager, label, "delegate");
            Log(manager, label, "startSession");
            Log(manager, label, "retryLicenceValidation:");
            Log(manager, label, "setCaptureDelegate:");
            Log(manager, label, "captureDelegate");
        }

        public static bool Log(NSObject manager, string label, string selector)
        {
            bool responds = manager.RespondsToSelector(new Selector(selector));
            Console.WriteLine($"[{DateTime.Now:HH:mm:ss.fff}] [SelectorCheck][{label}] {selector} => {responds}");
            return responds;
        }
    }

    [Register("IOSCameraDelegate")]
    internal class IOSCameraDelegate : NSObject
    {
        private IOSCameraView? _mauiView;

        public IOSCameraDelegate()
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "Ctor", "Default constructor called");
        }

        protected IOSCameraDelegate(NativeHandle handle) : base(handle)
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "Ctor", "NativeHandle constructor called");
        }

        public IOSCameraDelegate(IOSCameraView mauiView)
        {
            _mauiView = mauiView;
            ScanflowIosLog.Info("IOSCameraDelegate", "Ctor", "Created with MAUI view reference");
        }

        [Export("capturedOutput:::::")]
        public void CapturedOutput(string result, ScannerType codeType, string[]? results, UIKit.UIImage? processedImage, global::CoreLocation.CLLocation? location)
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "CapturedOutput",
                $"result={result}, codeType={codeType}, resultsCount={results?.Length ?? 0}, hasImage={processedImage != null}, hasLocation={location != null}");
            _mauiView?.TriggerScanResult(result);
        }

        [Export("presentCameraPermissionsDeniedAlert")]
        public void PresentCameraPermissionsDeniedAlert()
        {
            ScanflowIosLog.Error("IOSCameraDelegate", "PresentCameraPermissionsDeniedAlert", "Camera permission denied by user or system");
        }

        [Export("locationAccessDeniedAlert")]
        public void LocationAccessDeniedAlert()
        {
            ScanflowIosLog.Error("IOSCameraDelegate", "LocationAccessDeniedAlert", "Location permission denied");
        }

        [Export("presentVideoConfigurationErrorAlert")]
        public void PresentVideoConfigurationErrorAlert()
        {
            ScanflowIosLog.Error("IOSCameraDelegate", "PresentVideoConfigurationErrorAlert", "Video configuration failed");
        }

        [Export("sessionRunTimeErrorOccurred")]
        public void SessionRunTimeErrorOccurred()
        {
            ScanflowIosLog.Error("IOSCameraDelegate", "SessionRunTimeErrorOccurred", "AVCapture session runtime error");
        }

        [Export("sessionWasInterrupted:")]
        public void SessionWasInterrupted(bool resumeManually)
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "SessionWasInterrupted(resumeManually)",
                $"resumeManually={resumeManually}");
        }

        [Export("sessionWasInterrupted")]
        public void SessionWasInterrupted()
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "SessionWasInterrupted", "Session interrupted (no resume flag)");
        }

        [Export("captured:::")]
        public void Captured(global::CoreVideo.CVPixelBuffer originalframe, global::CoreGraphics.CGRect overlayFrame, UIKit.UIImage croppedImage)
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "Captured",
                $"overlayFrame={overlayFrame.Width}x{overlayFrame.Height}, croppedImage={(croppedImage != null)}");
        }

        [Export("showAlert::")]
        public void ShowAlert(string? title, string message)
        {
            ScanflowIosLog.Info("IOSCameraDelegate", "ShowAlert", $"title={title ?? "null"}, message={message}");
        }
    }

    [Register("IOSLicenseDelegate")]
    internal class IOSLicenseDelegate : NSObject
    {
        private IOSCameraView? _mauiView;
        private ScanflowBarCodeManager? _barcodeManager;

        public IOSLicenseDelegate()
        {
            ScanflowIosLog.Info("IOSLicenseDelegate", "Ctor", "Default constructor called");
        }

        protected IOSLicenseDelegate(NativeHandle handle) : base(handle)
        {
            ScanflowIosLog.Info("IOSLicenseDelegate", "Ctor", "NativeHandle constructor called");
        }

        public IOSLicenseDelegate(IOSCameraView mauiView, ScanflowBarCodeManager barcodeManager)
        {
            _mauiView = mauiView;
            _barcodeManager = barcodeManager;
            ScanflowIosLog.Info("IOSLicenseDelegate", "Ctor", "Created with MAUI view and barcode manager references");
        }

        [Export("licenceOnSuccessWithResponse:")]
        public void LicenceOnSuccessWithResponse(string response)
        {
            ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "======== LICENSE VALIDATION SUCCESS ========");
            ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", $"Server response: {response}");

            if (_barcodeManager == null)
            {
                ScanflowIosLog.Error("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "Barcode manager is null — cannot start session");
            }
            else
            {
                ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "Calling StartSession on barcode manager...");
                _barcodeManager.StartSession();
                ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "StartSession completed");
            }

            ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "Raising OnLicenseSuccess to MAUI layer...");
            _mauiView?.TriggerLicenseSuccess(response);
            ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnSuccessWithResponse", "======== LICENSE FLOW COMPLETE (SUCCESS) ========");
        }

        [Export("licenceOnFailureWithError:")]
        public void LicenceOnFailureWithError(string error)
        {
            ScanflowIosLog.Error("IOSLicenseDelegate", "LicenceOnFailureWithError", "======== LICENSE VALIDATION FAILED ========");
            ScanflowIosLog.Error("IOSLicenseDelegate", "LicenceOnFailureWithError", $"Error from SDK: {error}");
            ScanflowIosLog.Info("IOSLicenseDelegate", "LicenceOnFailureWithError", "Raising OnLicenseFailure to MAUI layer...");
            _mauiView?.TriggerLicenseFailure(error);
            ScanflowIosLog.Error("IOSLicenseDelegate", "LicenceOnFailureWithError", "======== LICENSE FLOW COMPLETE (FAILURE) ========");
        }
    }

    public class IOSCameraView : View
    {
        public static readonly BindableProperty LicenseKeyProperty =
            BindableProperty.Create(
                nameof(LicenseKey),
                typeof(string),
                typeof(IOSCameraView),
                string.Empty,
                propertyChanged: OnLicenseKeyChanged);

        public static readonly BindableProperty ScannerModeProperty =
            BindableProperty.Create(
                nameof(ScannerMode),
                typeof(int),
                typeof(IOSCameraView),
                2,
                propertyChanged: OnScannerModeChanged);

        private static void OnLicenseKeyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ScanflowIosLog.Info("IOSCameraView", "LicenseKeyChanged",
                $"old={(oldValue as string) ?? "null"}, new={(newValue as string) ?? "null"}");
            ScanflowIosLog.LicenseKey("IOSCameraView", "LicenseKeyChanged", newValue as string);
        }

        private static void OnScannerModeChanged(BindableObject bindable, object oldValue, object newValue)
        {
            ScanflowIosLog.Info("IOSCameraView", "ScannerModeChanged", $"old={oldValue}, new={newValue}");
        }

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

        public event EventHandler<string>? OnScanResult;
        public event EventHandler<string>? OnLicenseSuccess;
        public event EventHandler<string>? OnLicenseFailure;

        internal void TriggerScanResult(string result)
        {
            ScanflowIosLog.Info("IOSCameraView", "TriggerScanResult", $"Forwarding scan result: {result}");
            OnScanResult?.Invoke(this, result);
        }

        internal void TriggerLicenseSuccess(string response)
        {
            ScanflowIosLog.Info("IOSCameraView", "TriggerLicenseSuccess", $"Forwarding license success: {response}");
            OnLicenseSuccess?.Invoke(this, response);
        }

        internal void TriggerLicenseFailure(string error)
        {
            ScanflowIosLog.Error("IOSCameraView", "TriggerLicenseFailure", $"Forwarding license failure: {error}");
            OnLicenseFailure?.Invoke(this, error);
        }
    }

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
            ScanflowIosLog.Info("ScanflowCameraContainer", "Ctor", "Container created");
            ScanflowIosLog.LicenseKey("ScanflowCameraContainer", "Ctor", mauiView.LicenseKey);
            ScanflowIosLog.Info("ScanflowCameraContainer", "Ctor", $"ScannerMode={mauiView.ScannerMode}");
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            ScanflowIosLog.Info("ScanflowCameraContainer", "LayoutSubviews",
                $"Bounds={Bounds.Width}x{Bounds.Height}, initialized={_isInitialized}");

            if (!_isInitialized && Bounds.Width > 0 && Bounds.Height > 0)
            {
                _isInitialized = true;
                ScanflowIosLog.Info("ScanflowCameraContainer", "LayoutSubviews", "Valid bounds detected — starting InitializeCamera");
                InitializeCamera();
            }
        }

        private void InitializeCamera()
        {
            ScanflowIosLog.Info("ScanflowCameraContainer", "InitializeCamera", "======== CAMERA INIT START ========");

            try
            {
                // 1️⃣ Read scanner configuration
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 1/5", "Reading scanner configuration");
                ScannerType scannerType = GetScannerType(_mauiView.ScannerMode);
                OveylayViewApperance overlayApperance = OveylayViewApperance.Square;
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 1/5",
                    $"scannerType={scannerType}, overlay={overlayApperance}, scannerModeIndex={_mauiView.ScannerMode}");

                // 2️⃣ Create ScanflowBarCodeManager
                //    The underlying native runtime type is ScanflowCameraManager, which exposes
                //    ALL selectors: captureDelegate, delegate, licenceDelegate, validateLicense:, etc.
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 2/5", "Creating ScanflowBarCodeManager");
                _barcodeManager = new ScanflowBarCodeManager(
                    this,
                    scannerType,
                    overlayApperance,
                    overCropNeed: false,
                    leftTopArc: UIColor.Green,
                    leftDownArc: UIColor.Green,
                    rightTopArc: UIColor.Green,
                    rightDownArc: UIColor.Green,
                    locationNeed: false
                );
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 2/5", "ScanflowBarCodeManager created");

                // 3️⃣ Create delegates
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 3/5", "Creating delegates");
                _cameraDelegate = new IOSCameraDelegate(_mauiView);
                _licenseDelegate = new IOSLicenseDelegate(_mauiView, _barcodeManager);

                // 4️⃣ Assign all delegates to the barcode manager
                //    All three delegate properties exist on the native object at runtime.
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 4/5", "Assigning delegates");
                _barcodeManager.WeakDelegate = _cameraDelegate;

                Console.WriteLine("Class = " + _barcodeManager.GetType().FullName);
                bool hasLicenceDelegate = _barcodeManager.RespondsToSelector(new Selector("setLicenceDelegate:"));
                Console.WriteLine("setLicenceDelegate: = " + hasLicenceDelegate);
                Console.WriteLine(
                    "retryLicenceValidation: = " +
                    _barcodeManager.RespondsToSelector(new Selector("retryLicenceValidation:")));
                Console.WriteLine(
                    "validateLicense: = " +
                    _barcodeManager.RespondsToSelector(new Selector("validateLicense:")));

                if (hasLicenceDelegate)
                {
                    _barcodeManager.WeakLicenceDelegate = _licenseDelegate;
                    ScanflowIosLog.Info("ScanflowCameraContainer", "Step 4/5", "WeakLicenceDelegate assigned");
                }
                else
                {
                    ScanflowIosLog.Error("ScanflowCameraContainer", "Step 4/5",
                        "SKIPPED WeakLicenceDelegate — setLicenceDelegate: not in native framework");
                }

                // 5️⃣ Validate licence — StartSession() is called inside the success callback
                ScanflowIosLog.Info("ScanflowCameraContainer", "Step 5/5", "Validating licence");
                ScanflowIosLog.LicenseKey("ScanflowCameraContainer", "Step 5/5", _mauiView.LicenseKey);

                if (!string.IsNullOrEmpty(_mauiView.LicenseKey))
                {
                    _barcodeManager.ValidateLicense(_mauiView.LicenseKey);
                    if (hasLicenceDelegate)
                    {
                        ScanflowIosLog.Info("ScanflowCameraContainer", "Step 5/5",
                            "ValidateLicense invoked — awaiting IOSLicenseDelegate callback");
                    }
                    else
                    {
                        _barcodeManager.StartSession();
                        ScanflowIosLog.Info("ScanflowCameraContainer", "Step 5/5",
                            "No licenceDelegate — StartSession after ValidateLicense");
                    }
                }
                else
                {
                    ScanflowIosLog.Error("ScanflowCameraContainer", "Step 5/5",
                        "License key is empty — ValidateLicense NOT called.");
                }

                ScanflowIosLog.Info("ScanflowCameraContainer", "InitializeCamera",
                    "======== CAMERA INIT END (awaiting licence callback) ========");
            }
            catch (Exception ex)
            {
                ScanflowIosLog.Error("ScanflowCameraContainer", "InitializeCamera",
                    "Camera initialization failed", ex);
            }
        }

        private ScannerType GetScannerType(int mode)
        {
            var scannerType = mode switch
            {
                0 => ScannerType.Qrcode,
                1 => ScannerType.Barcode,
                2 => ScannerType.Any,
                3 => ScannerType.OneOfMany,
                4 => ScannerType.BatchInventory,
                _ => ScannerType.Any
            };
            ScanflowIosLog.Info("ScanflowCameraContainer", "GetScannerType", $"mode={mode} -> {scannerType}");
            return scannerType;
        }

        public void StopCamera()
        {
            ScanflowIosLog.Info("ScanflowCameraContainer", "StopCamera", "Stopping session and releasing delegates...");
            _barcodeManager?.StopSession();
            _barcodeManager = null;
            _cameraDelegate  = null;
            _licenseDelegate = null;
            ScanflowIosLog.Info("ScanflowCameraContainer", "StopCamera", "Camera stopped");
        }

        public void StartCamera()
        {
            ScanflowIosLog.Info("ScanflowCameraContainer", "StartCamera", "Starting session...");
            _barcodeManager?.StartSession();
            ScanflowIosLog.Info("ScanflowCameraContainer", "StartCamera", "StartSession called");
        }

        public void ToggleFlashlight(bool enable)
        {
            ScanflowIosLog.Info("ScanflowCameraContainer", "ToggleFlashlight", $"enable={enable}");
            _barcodeManager?.FlashLight(enable);
        }
    }

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
            ScanflowIosLog.Info("IOSCameraViewHandler", "Ctor", "Handler created");
        }

        protected override ScanflowCameraContainer CreatePlatformView()
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "CreatePlatformView", "Creating ScanflowCameraContainer...");
            ScanflowIosLog.LicenseKey("IOSCameraViewHandler", "CreatePlatformView", VirtualView.LicenseKey);
            return new ScanflowCameraContainer(VirtualView);
        }

        protected override void ConnectHandler(ScanflowCameraContainer platformView)
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "ConnectHandler", "Handler connected to platform view");
            base.ConnectHandler(platformView);
        }

        protected override void DisconnectHandler(ScanflowCameraContainer platformView)
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "DisconnectHandler", "Handler disconnecting...");

            try
            {
                platformView?.StopCamera();
            }
            catch (Exception ex)
            {
                ScanflowIosLog.Error("IOSCameraViewHandler", "DisconnectHandler", "Error stopping camera", ex);
            }

            base.DisconnectHandler(platformView);
            ScanflowIosLog.Info("IOSCameraViewHandler", "DisconnectHandler", "Handler disconnected");
        }

        public void StartSession()
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "StartSession", "Manual start session requested");
            PlatformView?.StartCamera();
        }

        public void StopSession()
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "StopSession", "Manual stop session requested");
            PlatformView?.StopCamera();
        }

        public void EnableFlashlight(bool enable)
        {
            ScanflowIosLog.Info("IOSCameraViewHandler", "EnableFlashlight", $"enable={enable}");
            PlatformView?.ToggleFlashlight(enable);
        }
    }
}
