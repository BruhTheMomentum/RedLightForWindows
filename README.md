# RedLight

RedLight is a lightweight Windows utility that applies a customizable full-screen color filter. It is designed to help reduce eye strain (via a red filter for night use) or assist with accessibility by providing various color simulations.

## 📥 Download

You can download the latest standalone executable (`RedLightForWindows.exe`) from the **[Releases](../../releases)** page.

## Features

*   **Full-Screen Filters:** Apply various color matrices to the entire screen using the Windows Magnification API.
*   **Filter Modes:**
    *   **Red:** Reduces blue light, ideal for night usage.
    *   **Grayscale:** Converts the screen to black and white.
    *   **Color Blindness Simulations:** Protanopia (Red/Green), Deuteranopia (Green/Red), and Tritanopia (Blue/Yellow).
    *   **Color Tint:** Choose any custom color to tint the screen.
*   **Adjustable Intensity:** Fine-tune the strength of the filter using a slider.
*   **System Tray Integration:** Runs quietly in the background; double-click the tray icon to open settings.

## Requirements

*   Windows 10 or Windows 11
*   [.NET 10.0 Runtime](https://dotnet.microsoft.com/download/dotnet/10.0)

## Development

### Building from Source

1.  Clone the repository.
2.  Ensure you have the .NET 10.0 SDK installed.
3.  Open the solution `RedLightForWindows.slnx` in Visual Studio or VS Code.
4.  Build the project:
    ```bash
    dotnet build
    ```

### Usage

1.  Run the application (`dotnet run` or via the executable).
2.  Check **Enable Filter** to start.
3.  Select a **Filter Type** from the dropdown.
4.  Adjust the **Intensity** slider to control the effect strength.
5.  If using "Color Tint", click the color box to choose a custom color.
6.  Closing the window minimizes the application to the system tray. Right-click the tray icon to Exit completely.

## Technology Stack

*   **Framework:** .NET 10 (WPF & Windows Forms components)
*   **API:** Windows Magnification API (`MagSetFullscreenColorEffect`)
