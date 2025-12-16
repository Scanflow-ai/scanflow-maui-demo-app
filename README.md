# Scanflow MAUI Demo App

This is a demo application built with **.NET MAUI** (Multi-platform App UI) showcasing the capabilities of the **Scanflow SDK**.

Scanflow is an AI Scanner on smart devices for data capture and workflow automation. It captures multiple data types including Barcodes, QR codes, Text, IDs, and Safety codes.

## Features

- **Cross-Platform Support**: Runs on Android and iOS (targeting .NET 9.0).
- **Scanner Selection**: Choose from various scanning modes (Barcode, QR Code, Batch Inventory, Any).
- **Scanflow SDK Integration**: Demonstrates how to integrate and use the `Scanflow.BarcodeCapture.Maui` package.
- **License Validation**: Handles license key validation (API check on first run, cached locally).
- **Camera Controls**: Includes flashlight toggle and overlay UI.
- **Modern UI**: Clean interface using .NET MAUI and CommunityToolkit.

## Technologies Used

- **.NET MAUI** (.NET 9.0)
- **Scanflow SDK** (`Scanflow.BarcodeCapture.Maui`)
- **CommunityToolkit.Maui**
- **Mopups**

## Prerequisites

- Visual Studio 2022
- .NET 9.0 SDK
- .NET MAUI Workload installed

## Getting Started

1.  **Clone the Repository**
    ```bash
    git clone <repository-url>
    cd ScanflowMauiDemoApp
    ```

2.  **Open the Solution**
    Open `ScanflowMauiDemoApp.sln` in Visual Studio 2022.


 3.  **Purchase a License Key**
    To use the Scanflow SDK, you need a valid license key.
    
    1.  Create a free test account: Visit https://console.scanflow.ai/ if you do not already have an account.
    2.  Login: Go to https://console.scanflow.ai/login to access your account.
    3.  Generate Key: Click "Create native SDK licensing key" and input the bundle ID for this project.eg :`com.scanflow.demo`.
    4.  Copy Key: Copy the generated license key.   

4.  **Configure License Keys**
    You need valid Scanflow SDK license keys to run the application.

    Open `ScanflowMauiDemoApp/Services/ScanflowService.cs` and replace the placeholder keys with your actual license keys:

    ```csharp
    // Platform-specific license keys
    private const string AndroidLicenseKey = "PLACE YOUR KEY";
    private const string IOSLicenseKey = "PLACE YOUR KEY";
    ```
5. **Install NuGet Packages**
   -Right-click on the solution in Solution Explorer and select Manage NuGet Packages.
   - Install the **Scanflow.Barcode.Maui** NuGet package.
   -Once it is installed, ensure that **Xamarin.AndroidX.Tracing.Tracing.Ktx 1.3.0.1** is also installed for Android as a dependency package.

6.  **Run the Application**
    - Select your target platform (Android Emulator/Device or iOS Simulator/Device) from the run dropdown.
    - Press `F5` or click the "Start Debugging" button.

## Project Structure

- **Services/**: Contains `ScanflowService.cs` which manages SDK initialization, license validation, and camera lifecycle.
- **Views/**: Contains the application pages:
    - `SplashPage`: Handles initial loading and service initialization.
    - `HomePage`: Displays available scanner types.
    - `ScanViewPage`: The main scanning interface with camera preview.
- **Resources/**: Contains app assets (images, fonts, raw files).

## Troubleshooting

- **License Errors**: Ensure your device has internet access for the initial license validation. Double-check that your license key is correct and matches the bundle ID/package name 
- **Camera Permissions**: The app should request camera permissions automatically. If scanning doesn't start, check the app permissions in your device settings.



