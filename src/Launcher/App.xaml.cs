using System.Globalization;
using System.Windows;

namespace LauncherV3;

/// <summary>
/// Interaction logic for App.xaml.
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
        CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;
        base.OnStartup(e);
    }
}
