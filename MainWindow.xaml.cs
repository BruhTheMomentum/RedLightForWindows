using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace RedLight;

public partial class MainWindow : Window
{
    private DispatcherTimer? _sliderTimer;

    public MainWindow()
    {
        InitializeComponent();
        InitializeSliderTimer();
        Deactivated += (_, _) => HidePopup();
        ApplyCurrentFilter();
    }

    public void ShowPopup()
    {
        var workArea = SystemParameters.WorkArea;
        Left = workArea.Right - Width - 12;
        Top = workArea.Bottom - Height - 24;

        Opacity = 0;
        Show();
        Activate();

        var fadeIn = new DoubleAnimation(0, 1, TimeSpan.FromMilliseconds(150));
        var slideUp = new DoubleAnimation(Top + 10, Top, TimeSpan.FromMilliseconds(150))
        {
            EasingFunction = new QuadraticEase()
        };
        BeginAnimation(OpacityProperty, fadeIn);
        BeginAnimation(TopProperty, slideUp);
    }

    public void HidePopup()
    {
        Hide();
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        e.Cancel = true;
        HidePopup();
    }

    private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        DragMove();
    }

    private void InitializeSliderTimer()
    {
        _sliderTimer = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(100)
        };
        _sliderTimer.Tick += (s, e) =>
        {
            ApplyCurrentFilter();
            _sliderTimer.Stop();
        };
    }

    private void IntensitySlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
    {
        if (_sliderTimer == null) return;
        _sliderTimer.Stop();
        _sliderTimer.Start();

        if (IntensityValueText != null)
            IntensityValueText.Text = $"{(int)(IntensitySlider.Value * 100)}%";
    }

    private void ApplyFilter(object? sender, RoutedEventArgs? e) => ApplyCurrentFilter();

    private void ApplyCurrentFilter()
    {
        if (EnableFilterCheckBox == null || FilterTypeComboBox == null || IntensitySlider == null)
            return;

        if (EnableFilterCheckBox.IsChecked != true)
        {
            NativeMethods.ResetColorEffect();
            return;
        }

        float intensity = (float)IntensitySlider.Value;
        string filterType = (FilterTypeComboBox.SelectedItem as ComboBoxItem)?.Content as string ?? "Red";

        float[] matrix = GetMatrixForFilter(filterType, intensity);
        var effect = new NativeMethods.MagColorEffect(matrix);
        NativeMethods.MagSetFullscreenColorEffect(ref effect);
    }

    private void QuitButton_Click(object sender, RoutedEventArgs e)
    {
        System.Windows.Application.Current.Shutdown();
    }

    private static float[] GetMatrixForFilter(string filterType, float intensity)
    {
        float[] matrix = filterType switch
        {
            "Grayscale" =>
            [
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0.2126f, 0.7152f, 0.0722f, 0, 0,
                0,       0,       0,       1, 0,
                0,       0,       0,       0, 1
            ],
            "Red/Green (Protanopia)" =>
            [
                0.1121f, 0.8853f, -0.0005f, 0, 0,
                0.1127f, 0.8897f, -0.0001f, 0, 0,
                0.0045f, 0.0000f, 1.0019f,  0, 0,
                0,       0,       0,        1, 0,
                0,       0,       0,        0, 1
            ],
            "Green/Red (Deuteranopia)" =>
            [
                0.2920f, 0.7054f, -0.0003f, 0, 0,
                0.2934f, 0.7089f, 0.0000f,  0, 0,
                -0.0210f,0.0256f, 1.0019f,  0, 0,
                0,       0,       0,        1, 0,
                0,       0,       0,        0, 1
            ],
            "Blue/Yellow (Tritanopia)" =>
            [
                1.0160f, 0.1351f, -0.1488f, 0, 0,
                -0.0154f,0.8683f, 0.1448f,  0, 0,
                0.1002f, 0.8168f, 0.1169f,  0, 0,
                0,       0,       0,        1, 0,
                0,       0,       0,        0, 1
            ],
            _ =>
            [
                1, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 0, 0,
                0, 0, 0, 1, 0,
                0, 0, 0, 0, 1
            ]
        };

        var identity = NativeMethods.IdentityMatrix;
        for (int i = 0; i < 25; i++)
        {
            matrix[i] = (identity[i] * (1 - intensity)) + (matrix[i] * intensity);
        }

        return matrix;
    }
}
