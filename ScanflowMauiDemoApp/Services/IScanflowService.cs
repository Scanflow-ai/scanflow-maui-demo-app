using System;
using System.Threading.Tasks;
#if ANDROID
using Scanflow.BarcodeCapture.Maui;
using Scanflow.BarcodeCapture.Maui.Models;
#endif

namespace ScanflowMauiDemoApp.Services
{
    /// <summary>
    /// Service interface for managing Scanflow SDK operations.
    /// Handles license validation, camera initialization, and state management.
    /// </summary>
    public interface IScanflowService
    {
        #region Properties
        
        /// <summary>
        /// The license key used for Scanflow SDK.
        /// </summary>
        string LicenseKey { get; }
        
        /// <summary>
        /// Indicates whether the license has been validated (either from saved preferences or API).
        /// </summary>
        bool IsLicenseValidated { get; }
        
        /// <summary>
        /// Indicates whether this is the first time the app is being used (license not yet saved).
        /// </summary>
        bool IsFirstTimeUse { get; }
        
        /// <summary>
        /// Indicates whether the camera is initialized and ready for scanning.
        /// </summary>
        bool IsCameraReady { get; }
        
        /// <summary>
        /// Indicates whether the service has been fully initialized from splash screen.
        /// Pages should check this and redirect to splash if false.
        /// </summary>
        bool IsFullyInitialized { get; }
        
        /// <summary>
        /// Indicates whether license validation is currently in progress.
        /// </summary>
        bool IsValidationInProgress { get; }
        
        /// <summary>
        /// The last error message from license validation, if any.
        /// </summary>
        string LastError { get; }
        
#if ANDROID
        /// <summary>
        /// The shared CameraPreview instance used across pages.
        /// </summary>
        CameraPreview? CameraPreview { get; }
#endif
        
        #endregion
        
        #region Events
        
        /// <summary>
        /// Raised when license validation succeeds.
        /// </summary>
        event Action<string>? OnLicenseValidated;
        
        /// <summary>
        /// Raised when license validation fails.
        /// </summary>
        event Action<string>? OnLicenseValidationFailed;
        
        /// <summary>
        /// Raised when the camera is ready for scanning.
        /// </summary>
        event Action? OnCameraReady;
        
        #endregion
        
        #region Methods
        
        /// <summary>
        /// Initializes the Scanflow SDK and validates the license.
        /// For first-time use, validates via API and saves to preferences.
        /// For subsequent uses, uses saved license (no API call).
        /// </summary>
        /// <param name="containerForCamera">The container where the hidden camera will be added for initialization.</param>
        /// <returns>Task that completes when initialization is done.</returns>
        Task<bool> InitializeAsync(Layout containerForCamera);
        
        /// <summary>
        /// Gets the camera preview for use in a scan page.
        /// The camera should be removed from any previous container before calling this.
        /// </summary>
        /// <returns>The CameraPreview instance configured for full-screen scanning.</returns>
        View? GetCameraForScanning();
        
        /// <summary>
        /// Returns the camera to the service after scanning is complete.
        /// Call this when navigating away from the scan page.
        /// </summary>
        void ReturnCamera();
        
        /// <summary>
        /// Clears the saved license, forcing re-validation on next app start.
        /// </summary>
        void ClearSavedLicense();
        
        /// <summary>
        /// Stops the camera. Call when navigating away from scan page.
        /// </summary>
        void StopCamera();
        
        /// <summary>
        /// Restarts the camera. Call when returning to scan page.
        /// </summary>
        void StartCamera();
        
        /// <summary>
        /// Mark service as initialized without doing license validation.
        /// Used by iOS where license validation happens at ScanViewPage level.
        /// </summary>
        void MarkAsInitialized();
        
        #endregion
    }
}

