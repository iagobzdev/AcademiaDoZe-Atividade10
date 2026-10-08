// Autor: Iago Barboza Ceccatto
using AcademiaDoZe.Application.Interfaces;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.ViewModels;

public partial class DashboardListViewModel : BaseViewModel
{
    private readonly ILogradouroService _logradouroService;
    private readonly IAlunoService _alunoService;
    private readonly IColaboradorService _colaboradorService;
    private readonly IMatriculaService _matriculaService;

    private int _totalLogradouros;
    public int TotalLogradouros { get => _totalLogradouros; set => SetProperty(ref _totalLogradouros, value); }

    private int _totalAlunos;
    public int TotalAlunos { get => _totalAlunos; set => SetProperty(ref _totalAlunos, value); }

    private int _totalColaboradores;
    public int TotalColaboradores { get => _totalColaboradores; set => SetProperty(ref _totalColaboradores, value); }

    private int _totalMatriculas;
    public int TotalMatriculas { get => _totalMatriculas; set => SetProperty(ref _totalMatriculas, value); }

    private string _ultimaAtualizacao = string.Empty;
    public string UltimaAtualizacao { get => _ultimaAtualizacao; set => SetProperty(ref _ultimaAtualizacao, value); }

    public DashboardListViewModel(ILogradouroService logradouroService, IAlunoService alunoService, IColaboradorService colaboradorService, IMatriculaService matriculaService)
    {
        _logradouroService = logradouroService;
        _alunoService = alunoService;
        _colaboradorService = colaboradorService;
        _matriculaService = matriculaService;
        Title = "Dashboard";
    }

    [RelayCommand]
    private async Task LoadDashboardDataAsync()
    {
        if (IsBusy)
            return;

        try
        {
            IsBusy = true;

            using var cts = new CancellationTokenSource(TimeSpan.FromSeconds(5));

            var logradourosTask = _logradouroService.ObterTodosAsync(cts.Token);
            var alunosTask = _alunoService.ObterTodosAsync(cts.Token);
            var colaboradoresTask = _colaboradorService.ObterTodosAsync(cts.Token);
            var matriculasTask = _matriculaService.ObterTodasAsync(cts.Token);

            await Task.WhenAll(logradourosTask, alunosTask, colaboradoresTask, matriculasTask);

            TotalLogradouros = (await logradourosTask).Count();
            TotalAlunos = (await alunosTask).Count();
            TotalColaboradores = (await colaboradoresTask).Count();
            TotalMatriculas = (await matriculasTask).Count();

            UltimaAtualizacao = $"Atualizado às {DateTime.Now:HH:mm:ss}";
        }
        catch (OperationCanceledException)
        {
            await Shell.Current.DisplayAlertAsync(
                "Tempo Esgotado",
                "A conexão com o banco de dados expirou (timeout). Verifique se o caminho ou os dados de conexão estão corretos.",
                "OK");
        }
        catch (Exception ex)
        {
            await Shell.Current.DisplayAlertAsync("Erro", $"Erro ao carregar dados do painel: {ex.Message}", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private static async Task NavigateToLogradourosAsync() => await NavegarAsync("//logradouros", "Logradouros");

    [RelayCommand]
    private static async Task NavigateToAlunosAsync() => await NavegarAsync("//alunos", "Alunos");

    [RelayCommand]
    private static async Task NavigateToColaboradoresAsync() => await NavegarAsync("//colaboradores", "Colaboradores");

    [RelayCommand]
    private static async Task NavigateToMatriculasAsync() => await NavegarAsync("//matriculas", "Matrículas");

    [RelayCommand]
    private static void AlternarTema()
    {
        if (Microsoft.Maui.Controls.Application.Current is not { } app)
            return;

        Preferences.Set("Tema", app.RequestedTheme == AppTheme.Dark ? "light" : "dark");
        WeakReferenceMessenger.Default.Send(new TemaPreferencesUpdatedMessage("TemaAlterado"));
    }

    private static async Task NavegarAsync(string rota, string funcionalidade)
    {
        try
        {
            await Shell.Current.GoToAsync(rota);
        }
        catch (Exception)
        {
            await Shell.Current.DisplayAlertAsync("Em breve", $"A funcionalidade {funcionalidade} será disponibilizada nas próximas etapas do projeto.", "OK");
        }
    }
}
