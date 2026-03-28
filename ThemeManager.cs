using Microsoft.Win32;
using Application = System.Windows.Application;
using ResourceDictionary = System.Windows.ResourceDictionary;

namespace RedLight;

public static class ThemeManager
{
    public static void ApplySystemTheme()
    {
        bool isLight = IsSystemLightTheme();
        string source = isLight ? "Themes/LightColors.xaml" : "Themes/DarkColors.xaml";

        var colorDict = new ResourceDictionary
        {
            Source = new Uri(source, UriKind.Relative)
        };

        var merged = Application.Current.Resources.MergedDictionaries;

        for (int i = merged.Count - 1; i >= 0; i--)
        {
            var uri = merged[i].Source?.OriginalString ?? "";
            if (uri.Contains("Colors.xaml"))
                merged.RemoveAt(i);
        }

        merged.Add(colorDict);
    }

    private static bool IsSystemLightTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(
                @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            var value = key?.GetValue("AppsUseLightTheme");
            return value is int i && i == 1;
        }
        catch
        {
            return false;
        }
    }
}
