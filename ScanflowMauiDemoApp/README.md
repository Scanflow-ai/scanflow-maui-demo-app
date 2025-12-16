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

- Visual Studio 2022 (latest version recommended)
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

3.  **Configure License Keys**
    You need valid Scanflow SDK license keys to run the application.

    Open `ScanflowMauiDemoApp/Services/ScanflowService.cs` and replace the placeholder keys with your actual license keys:

    ```csharp
    // Platform-specific license keys
    private const string AndroidLicenseKey = "PLACE YOUR KEY";
    private const string IOSLicenseKey = "PLACE YOUR KEY";
    ```

4.  **Restore Nuget Packages**
    Right-click on the solution in Solution Explorer and select **Restore NuGet Packages**.

    *Note: Ensure you have access to the Scanflow NuGet feed if the packages are not available publicly.*

5.  **Run the Application**
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

- **License Errors**: Ensure your device has internet access for the initial license validation. Double-check that your license key is correct and matches the bundle ID/package name (`com.scanflow.mauidemo`).
- **Camera Permissions**: The app should request camera permissions automatically. If scanning doesn't start, check the app permissions in your device settings.

## License

[Add your license information here]
