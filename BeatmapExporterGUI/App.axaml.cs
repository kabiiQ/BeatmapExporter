using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using BeatmapExporterGUI.Exporter;
using BeatmapExporterGUI.Utilities;
using BeatmapExporterGUI.ViewModels;
using BeatmapExporterGUI.Views;
using System.Linq;

namespace BeatmapExporterGUI;

public partial class App : Application
{
    private ExporterApp? exporter;
    private DialogService? dialogs;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        // Line below is needed to remove Avalonia data validation.
        // Without this line you will get duplicate validations from both Avalonia and CT
        BindingPlugins.DataValidators.RemoveAt(0);

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            exporter = new();
            var outer = new OuterViewModel();

            desktop.MainWindow = new MainWindow
            {
                DataContext = outer
            };
            dialogs = new(desktop.MainWindow);

            var userDirectory = desktop.Args?.FirstOrDefault();

            outer.Home.SetLoading();
            outer.Home.CheckForUpdate();
            _ = outer.Home.LoadDatabase(userDirectory);
        }

        base.OnFrameworkInitializationCompleted();
    }

    public ExporterApp Exporter => exporter!;

    public DialogService DialogService => dialogs!;

    // Skipping full DI model for static access, really not needed here
    public static new App Current => (App)Application.Current;
}
