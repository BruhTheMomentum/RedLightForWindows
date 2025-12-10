using System;
using System.Windows;
using WinForms = System.Windows.Forms;
using System.Drawing;

namespace RedLight;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : System.Windows.Application
{
    private WinForms.NotifyIcon? _notifyIcon;
    private MainWindow? _mainWindow;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        try
        {
            NativeMethods.MagInitialize();
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Failed to initialize Magnification API: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
            return;
        }

        _mainWindow = new MainWindow();
        _mainWindow.Show();

        _notifyIcon = new WinForms.NotifyIcon();
        _notifyIcon.Icon = new Icon("icon.ico");
        _notifyIcon.Text = "Red Light";
        _notifyIcon.Visible = true;

        var contextMenu = new WinForms.ContextMenuStrip();
        contextMenu.Items.Add("Show", null, Show_Click);
        contextMenu.Items.Add("Exit", null, Exit_Click);
        _notifyIcon.ContextMenuStrip = contextMenu;
        _notifyIcon.DoubleClick += Show_Click;
    }

    private void Show_Click(object? sender, EventArgs e)
    {
        if (_mainWindow == null)
        {
            _mainWindow = new MainWindow();
        }
        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
    }

    private void Exit_Click(object? sender, EventArgs e)
    {
        Shutdown();
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
            // Reset to identity matrix to remove any color effect
            var identity = new float[]
            {
                1, 0, 0, 0, 0,
                0, 1, 0, 0, 0,
                0, 0, 1, 0, 0,
                0, 0, 0, 1, 0,
                0, 0, 0, 0, 1
            };
            var effect = new NativeMethods.MagColorEffect(identity);
            NativeMethods.MagSetFullscreenColorEffect(ref effect);

            NativeMethods.MagUninitialize();
        }
        catch (Exception)
        {
            // Suppress errors on exit
        }
        base.OnExit(e);
    }
}

