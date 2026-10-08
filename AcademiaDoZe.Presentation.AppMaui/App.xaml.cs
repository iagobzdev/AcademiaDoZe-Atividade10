// Autor: Iago Barboza Ceccatto
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui;

public partial class App : Microsoft.Maui.Controls.Application
{
    public App()
    {
        InitializeComponent();

        AplicarTema();

        WeakReferenceMessenger.Default.Register<TemaPreferencesUpdatedMessage>(this, (r, m) =>
        {
            AplicarTema();
        });
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell())
        {
            Title = "Academia do Iago - Iago Barboza Ceccatto",
            Width = 1280,
            Height = 820,
            MinimumWidth = 720,
            MinimumHeight = 560,
            TitleBar = new TitleBar
            {
                Title = "Academia do Iago",
                Subtitle = "Iago Barboza Ceccatto",
                Icon = "emblema.png",
                HeightRequest = 44
            }
        };
    }

    private void AplicarTema()
    {
        UserAppTheme = Preferences.Get("Tema", "system") switch
        {
            "light" => AppTheme.Light,
            "dark" => AppTheme.Dark,
            _ => AppTheme.Unspecified,
        };
    }
}
