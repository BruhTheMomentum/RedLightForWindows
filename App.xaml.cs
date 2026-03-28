using System.Drawing;
using WinForms = System.Windows.Forms;
using System.Windows;
using Application = System.Windows.Application;
using MessageBox = System.Windows.MessageBox;

namespace RedLight;

public partial class App : Application
{
    private WinForms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        ThemeManager.ApplySystemTheme();

        try
        {
            NativeMethods.MagInitialize();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Failed to initialize Magnification API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        _mainWindow = new MainWindow();

        _notifyIcon = new WinForms.NotifyIcon();
        using var iconStream = typeof(App).Assembly
            .GetManifestResourceStream("RedLightForWindows.icon.ico");
        _notifyIcon.Icon = new Icon(iconStream!);
        _notifyIcon.Text = "Red Light";
        _notifyIcon.Visible = true;

        _notifyIcon.MouseClick += (s, args) =>
        {
            if (_mainWindow!.IsVisible)
                _mainWindow.HidePopup();
            else
                _mainWindow.ShowPopup();
        };
    }

    protected override void OnExit(ExitEventArgs e)
    {
        if (_notifyIcon != null)
        {
            _notifyIcon.Dispose();
            _notifyIcon = null;
        }

        try
        {
            NativeMethods.ResetColorEffect();
            NativeMethods.MagUninitialize();
        }
        catch
        {
            // Suppress errors on exit
        }
        base.OnExit(e);
    }
}
