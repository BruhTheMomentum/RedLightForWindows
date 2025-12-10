using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System;
using System.Windows.Media;
using WinForms = System.Windows.Forms;


namespace RedLight;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private DispatcherTimer? _sliderTimer;
    private System.Windows.Media.Color _selectedTintColor = System.Windows.Media.Colors.Red;

    public MainWindow()
    {
        InitializeComponent();
        InitializeSliderTimer();
        // Apply the default filter on startup if the checkbox is checked.
        ApplyFilter(null, null);
    }

    private void InitializeSliderTimer()
    {
        _sliderTimer = new DispatcherTimer();
        _sliderTimer.Interval = TimeSpan.FromMilliseconds(100); // 100ms delay
        _sliderTimer.Tick += (s, e) =>
        {
            ApplyFilter(null, null);
            _sliderTimer?.Stop();
        };
    }

    private void IntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        // Debounce slider changes
        if (_sliderTimer != null)
        {
            _sliderTimer.Stop();
            _sliderTimer.Start();
        }
    }

    private void ApplyFilter(object? sender, RoutedEventArgs? e)
    {
        if (EnableFilterCheckBox == null || FilterTypeComboBox == null || IntensitySlider == null)
        {
            return; // Controls not yet initialized
        }

        // Also handle visibility change of the color picker
        if (sender == FilterTypeComboBox)
        {
            bool isColorTint = (FilterTypeComboBox.SelectedItem as ComboBoxItem)?.Content as string == "Color Tint";
            // The DataTrigger in XAML handles visibility, but we might need to apply the filter again.
            if (isColorTint)
            {
                 ApplyFilter(null, null);
            }
        }

        bool isEnabled = EnableFilterCheckBox.IsChecked ?? false;
        if (!isEnabled)
        {
            ResetFilter();
            return;
        }

        float intensity = (float)IntensitySlider.Value;
        string filterType = (FilterTypeComboBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Red";

        float[] matrix = GetMatrixForFilter(filterType, intensity);
        var effect = new NativeMethods.MagColorEffect(matrix);
        NativeMethods.MagSetFullscreenColorEffect(ref effect);
    }

    private void ColorPickerButton_Click(object sender, RoutedEventArgs e)
    {
        var colorDialog = new WinForms.ColorDialog();
        if (colorDialog.ShowDialog() == WinForms.DialogResult.OK)
        {
            var newColor = colorDialog.Color;
            _selectedTintColor = System.Windows.Media.Color.FromArgb(newColor.A, newColor.R, newColor.G, newColor.B);
            ColorPreview.Fill = new SolidColorBrush(_selectedTintColor);
            ApplyFilter(null, null);
        }
    }

    private void ResetFilter()
    {
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
    }

    private float[] GetMatrixForFilter(string filterType, float intensity)
    {
        float[] matrix;
        switch (filterType)
        {
            case "Grayscale":
                matrix = new float[]
                {
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0.2126f, 0.7152f, 0.0722f, 0, 0,
                    0,       0,       0,       1, 0,
                    0,       0,       0,       0, 1
                };
                break;
            case "Red/Green (Protanopia)":
                matrix = new float[]
                {
                    0.1121f, 0.8853f, -0.0005f, 0, 0,
                    0.1127f, 0.8897f, -0.0001f, 0, 0,
                    0.0045f, 0.0000f, 1.0019f,  0, 0,
                    0,       0,       0,        1, 0,
                    0,       0,       0,        0, 1
                };
                break;
            case "Green/Red (Deuteranopia)":
                matrix = new float[]
                {
                    0.2920f, 0.7054f, -0.0003f, 0, 0,
                    0.2934f, 0.7089f, 0.0000f,  0, 0,
                    -0.0210f,0.0256f, 1.0019f,  0, 0,
                    0,       0,       0,        1, 0,
                    0,       0,       0,        0, 1
                };
                break;
            case "Blue/Yellow (Tritanopia)":
                matrix = new float[]
                {
                    1.0160f, 0.1351f, -0.1488f, 0, 0,
                    -0.0154f,0.8683f, 0.1448f,  0, 0,
                    0.1002f, 0.8168f, 0.1169f,  0, 0,
                    0,       0,       0,        1, 0,
                    0,       0,       0,        0, 1
                };
                break;
            case "Color Tint":
                // This matrix creates a solid color screen, which is then blended via the intensity slider.
                matrix = new float[]
                {
                    0, 0, 0, 0, _selectedTintColor.R / 255f,
                    0, 0, 0, 0, _selectedTintColor.G / 255f,
                    0, 0, 0, 0, _selectedTintColor.B / 255f,
                    0, 0, 0, 1, 0,
                    0, 0, 0, 0, 1
                };
                break;
            case "Red":
            default:
                matrix = new float[]
                {
                    1, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 0, 0,
                    0, 0, 0, 1, 0,
                    0, 0, 0, 0, 1
                };
                break;
        }

        // Apply intensity by interpolating with the identity matrix
        var identity = new float[]
        {
            1, 0, 0, 0, 0,
            0, 1, 0, 0, 0,
            0, 0, 1, 0, 0,
            0, 0, 0, 1, 0,
            0, 0, 0, 0, 1
        };

        for (int i = 0; i < 25; i++)
        {
            matrix[i] = (identity[i] * (1 - intensity)) + (matrix[i] * intensity);
        }

        return matrix;
    }

    private void Window_Closing(object? sender, System.ComponentModel.CancelEventArgs e)
    {
        // Instead of closing, hide the window
        e.Cancel = true;
        this.Hide();
    }
}