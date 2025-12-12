using System;
using System.Threading.Tasks;
using Microsoft.Maui.Storage;
using Microsoft.Maui.Networking;
using Microsoft.Maui.ApplicationModel;
#if ANDROID
using Scanflow.BarcodeCapture.Maui;
using Scanflow.BarcodeCapture.Maui.Models;
#endif

namespace ScanflowMauiDemoApp.Services
{
    /// <summary>
    /// Singleton service that manages Scanflow SDK operations.
    /// Implements proper lifecycle management and state persistence.
    /// </summary>
    public class ScanflowService : IScanflowService
    {
        #region Constants
        
        private const string LicenseValidatedKey = "ScanflowLicenseValidated";
        private const string SavedLicenseKey = "ScanflowSavedLicenseKey";
        
        // Platform-specific license keys
        private const string AndroidLicenseKey = "PLACE YOUR KEY";
        private const string IOSLicenseKey = "PLACE YOUR KEY";
        
        #endregion
        
        #region Private Fields
        
        private bool _isInitialized;
        private bool _isValidationInProgress;
        private string _lastError = string.Empty;
        private TaskCompletionSource<bool>? _validationTaskSource;
        
#if ANDROID
        private CameraPreview? _cameraPreview;
        private Layout? _currentContainer;
#endif
        
        #endregion
        
        #region Properties
        
        public string LicenseKey
        {
            get
            {
#if IOS
                return IOSLicenseKey;
#elif ANDROID
                return AndroidLicenseKey;
#else
                return AndroidLicenseKey; // Default to Android key
#endif
            }
        }
        
        public bool IsLicenseValidated { get; private set; }
        
        public bool IsFirstTimeUse => !Preferences.Default.Get(LicenseValidatedKey, false);
        
        public bool IsCameraReady { get; private set; }
        
        public bool IsFullyInitialized => _isInitialized && IsLicenseValidated && IsCameraReady;
        
        public bool IsValidationInProgress => _isValidationInProgress;
        
        public string LastError => _lastError;
        
#if ANDROID
        public CameraPreview? CameraPreview => _cameraPreview;
#endif
        
        #endregion
        
        #region Events
        
        public event Action<string>? OnLicenseValidated;
        public event Action<string>? OnLicenseValidationFailed;
        public event Action? OnCameraReady;
        
        #endregion
        
        #region Constructor
        
        public ScanflowService()
        {
            Console.WriteLine("[ScanflowService] Service created");
        }
        
        #endregion
        
        #region Public Methods
        
        public async Task<bool> InitializeAsync(Layout containerForCamera)
        {
            Console.WriteLine("[ScanflowService] ============================================");
            Console.WriteLine($"[ScanflowService] InitializeAsync called");
            Console.WriteLine($"[ScanflowService] IsFirstTimeUse: {IsFirstTimeUse}");
            Console.WriteLine($"[ScanflowService] Already initialized: {_isInitialized}");
            Console.WriteLine("[ScanflowService] ============================================");
            
            // Already initialized
            if (_isInitialized && IsCameraReady)
            {
                Console.WriteLine("[ScanflowService] Already initialized, returning true");
                return true;
            }
            
            // Validation in progress
            if (_isValidationInProgress && _validationTaskSource != null)
            {
                Console.WriteLine("[ScanflowService] Validation in progress, waiting...");
                return await _validationTaskSource.Task;
            }
            
#if ANDROID
            _currentContainer = containerForCamera;
            _isValidationInProgress = true;
            _validationTaskSource = new TaskCompletionSource<bool>();
            
            try
            {
                bool isFirstTime = IsFirstTimeUse;
                string savedKey = Preferences.Default.Get(SavedLicenseKey, string.Empty);
                
                // Check if license key changed
                if (!isFirstTime && savedKey != LicenseKey)
                {
                    Console.WriteLine("[ScanflowService] License key changed, treating as first time");
                    isFirstTime = true;
                }
                
                // ========== SUBSEQUENT OPENS: Use saved license ==========
                if (!isFirstTime)
                {
                    Console.WriteLine("[ScanflowService] >>> Using SAVED license - no validation <<<");
                    
                    await InitializeCameraWithoutValidation(savedKey);
                    
                    IsLicenseValidated = true;
                    IsCameraReady = true;
                    _isInitialized = true;
                    _isValidationInProgress = false;
                    _validationTaskSource.TrySetResult(true);
                    
                    OnCameraReady?.Invoke();
                    
                    Console.WriteLine("[ScanflowService] Camera ready with saved license");
                    return true;
                }
                
                // ========== FIRST TIME: Validate with server ==========
                Console.WriteLine("[ScanflowService] >>> FIRST TIME - validating with server <<<");
                
                // Check internet connectivity
                if (Connectivity.Current.NetworkAccess != NetworkAccess.Internet)
                {
                    _lastError = "No internet connection. Please connect to the internet for first-time activation.";
                    _isValidationInProgress = false;
                    _validationTaskSource.TrySetResult(false);
                    OnLicenseValidationFailed?.Invoke(_lastError);
                    return false;
                }
                
                await InitializeCameraWithValidation();
                
                // Wait for validation callback
                return await _validationTaskSource.Task;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScanflowService] ERROR: {ex.Message}");
                _lastError = ex.Message;
                _isValidationInProgress = false;
                _validationTaskSource?.TrySetResult(false);
                OnLicenseValidationFailed?.Invoke(ex.Message);
                return false;
            }
#else
            // Non-Android platforms
            IsLicenseValidated = true;
            IsCameraReady = true;
            _isInitialized = true;
            return true;
#endif
        }
        
        public View? GetCameraForScanning()
        {
#if ANDROID
            if (_cameraPreview == null)
            {
                Console.WriteLine("[ScanflowService] GetCameraForScanning: Camera not initialized!");
                return null;
            }
            
            // Reset size constraints for full-screen display
            _cameraPreview.ClearValue(View.HeightRequestProperty);
            _cameraPreview.ClearValue(View.WidthRequestProperty);
            _cameraPreview.VerticalOptions = LayoutOptions.FillAndExpand;
            _cameraPreview.HorizontalOptions = LayoutOptions.FillAndExpand;
            
            Console.WriteLine("[ScanflowService] Returning camera for scanning (full-screen)");
            return _cameraPreview;
#else
            return null;
#endif
        }
        
        public void ReturnCamera()
        {
#if ANDROID
            Console.WriteLine("[ScanflowService] Camera returned to service");
            // Camera instance is retained, just stopped
            StopCamera();
#endif
        }
        
        public void ClearSavedLicense()
        {
            Preferences.Default.Remove(LicenseValidatedKey);
            Preferences.Default.Remove(SavedLicenseKey);
            IsLicenseValidated = false;
            IsCameraReady = false;
            _isInitialized = false;
            Console.WriteLine("[ScanflowService] Saved license cleared");
        }
        
        /// <summary>
        /// Mark service as initialized without doing license validation
        /// Used by iOS where license validation happens at ScanViewPage level
        /// </summary>
        public void MarkAsInitialized()
        {
            _isInitialized = true;
            IsLicenseValidated = true;
            IsCameraReady = true;
            Console.WriteLine("[ScanflowService] Service marked as initialized (iOS - validation deferred to ScanViewPage)");
        }
        
        public void StopCamera()
        {
#if ANDROID
            try
            {
                _cameraPreview?.StopScanning();
                Console.WriteLine("[ScanflowService] Camera stopped");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScanflowService] Error stopping camera: {ex.Message}");
            }
#endif
        }
        
        public void StartCamera()
        {
#if ANDROID
            try
            {
                _cameraPreview?.StartScanning();
                Console.WriteLine("[ScanflowService] Camera started");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ScanflowService] Error starting camera: {ex.Message}");
            }
#endif
        }
        
        #endregion
        
        #region Private Methods
        
#if ANDROID
        private async Task InitializeCameraWithoutValidation(string licenseKey)
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _cameraPreview = new CameraPreview
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = 100,
                    WidthRequest = 100
                };
                
                // Create scan session without waiting for validation callback
                _cameraPreview.CreateScanSession(licenseKey, DecodeConfig.Any, 0.2f);
                
                // Add to container to trigger native handler
                _currentContainer?.Children.Clear();
                _currentContainer?.Children.Add(_cameraPreview);
            });
            
            // Small delay to ensure native handler is created
            await Task.Delay(100);
        }
        
        private async Task InitializeCameraWithValidation()
        {
            await MainThread.InvokeOnMainThreadAsync(() =>
            {
                _cameraPreview = new CameraPreview
                {
                    VerticalOptions = LayoutOptions.FillAndExpand,
                    HorizontalOptions = LayoutOptions.FillAndExpand,
                    HeightRequest = 100,
                    WidthRequest = 100
                };
                
                // Subscribe to validation callbacks
                _cameraPreview.OnLicenceOnSuccessWithResponse += OnValidationSuccess;
                _cameraPreview.OnLicenceOnFailureWithError += OnValidationFailure;
                
                // Create scan session - triggers validation
                _cameraPreview.CreateScanSession(LicenseKey, DecodeConfig.Any, 0.2f);
                
                // Add to container to trigger native handler
                _currentContainer?.Children.Clear();
                _currentContainer?.Children.Add(_cameraPreview);
            });
        }
        
        private void OnValidationSuccess(string response)
        {
            Console.WriteLine($"[ScanflowService] ========== LICENSE VALIDATED ==========");
            Console.WriteLine($"[ScanflowService] Response: {response}");
            
            // Save license to preferences
            Preferences.Default.Set(LicenseValidatedKey, true);
            Preferences.Default.Set(SavedLicenseKey, LicenseKey);
            
            IsLicenseValidated = true;
            IsCameraReady = true;
            _isInitialized = true;
            _isValidationInProgress = false;
            
            _validationTaskSource?.TrySetResult(true);
            
            OnLicenseValidated?.Invoke(response);
            OnCameraReady?.Invoke();
            
            // Unsubscribe to avoid memory leaks
            if (_cameraPreview != null)
            {
                _cameraPreview.OnLicenceOnSuccessWithResponse -= OnValidationSuccess;
                _cameraPreview.OnLicenceOnFailureWithError -= OnValidationFailure;
            }
        }
        
        private void OnValidationFailure(string error)
        {
            Console.WriteLine($"[ScanflowService] ========== LICENSE VALIDATION FAILED ==========");
            Console.WriteLine($"[ScanflowService] Error: {error}");
            
            _lastError = error;
            IsLicenseValidated = false;
            IsCameraReady = false;
            _isValidationInProgress = false;
            
            // Clear any saved license on failure
            ClearSavedLicense();
            
            _validationTaskSource?.TrySetResult(false);
            
            OnLicenseValidationFailed?.Invoke(error);
            
            // Unsubscribe to avoid memory leaks
            if (_cameraPreview != null)
            {
                _cameraPreview.OnLicenceOnSuccessWithResponse -= OnValidationSuccess;
                _cameraPreview.OnLicenceOnFailureWithError -= OnValidationFailure;
            }
        }
#endif
        
        #endregion
    }
}

