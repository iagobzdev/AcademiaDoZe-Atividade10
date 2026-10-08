// Autor: Iago Barboza Ceccatto
using AcademiaDoZe.Application.DependencyInjection;
using AcademiaDoZe.Application.Enums;
using AcademiaDoZe.Application.Mappings;
using AcademiaDoZe.Presentation.AppMaui.Message;
using CommunityToolkit.Mvvm.Messaging;

namespace AcademiaDoZe.Presentation.AppMaui.Configuration;

public static class ConfigurationHelper
{
    public static string CaminhoSqlitePadrao => DeviceInfo.Platform == DevicePlatform.WinUI
        ? @"C:\DEV\AcademiaDoZe\db_academia_do_ze.db"
        : Path.Combine(FileSystem.AppDataDirectory, "db_academia_do_ze.db");

    public static void ConfigureServices(IServiceCollection services)
    {
        var (connectionString, databaseType) = ObterConfiguracaoAtual();

        var repoConfig = new RepositoryConfig
        {
            ConnectionString = connectionString,
            DatabaseType = databaseType.ToInfrastructure()
        };

        services.AddSingleton(repoConfig);

        WeakReferenceMessenger.Default.Register<RepositoryConfig, BancoPreferencesUpdatedMessage>(repoConfig, (r, m) =>
        {
            var (novaConnStr, novoDbType) = ObterConfiguracaoAtual();
            r.ConnectionString = novaConnStr;
            r.DatabaseType = novoDbType.ToInfrastructure();
        });

        services.AddApplicationServices();
    }

    public static (string ConnectionString, AppDatabaseType DatabaseType) ObterConfiguracaoAtual()
    {
        var databaseTypeStr = Preferences.Get("DatabaseType", AppDatabaseType.Sqlite.ToString());
        if (!Enum.TryParse<AppDatabaseType>(databaseTypeStr, out var databaseType))
        {
            databaseType = AppDatabaseType.Sqlite;
        }

        string connectionString;
        if (databaseType == AppDatabaseType.Sqlite)
        {
            var dbPath = Preferences.Get("Sqlite_Caminho", Preferences.Get("SqliteCaminho", CaminhoSqlitePadrao));
            if (string.IsNullOrWhiteSpace(dbPath))
                dbPath = CaminhoSqlitePadrao;

            TentarCriarDiretorioDoBanco(dbPath, out _);

            var complemento = Preferences.Get("Sqlite_Complemento", Preferences.Get("Complemento", "Default Timeout=5;"));
            connectionString = $"Data Source={dbPath};{complemento}";
        }
        else
        {
            var prefix = databaseType == AppDatabaseType.SqlServer ? "SqlServer" : "MySql";
            var defaultUser = databaseType == AppDatabaseType.SqlServer ? "sa" : "root";
            var defaultComplemento = databaseType == AppDatabaseType.SqlServer
                ? "TrustServerCertificate=True;Encrypt=True;Connect Timeout=5;Connection Timeout=5;"
                : "Connection Timeout=5;Default Command Timeout=30;";

            var dbServer = Preferences.Get($"{prefix}_Servidor", Preferences.Get("Servidor", "localhost"));
            var dbDatabase = Preferences.Get($"{prefix}_Banco", Preferences.Get("Banco", "db_academia_do_ze"));
            var dbUser = Preferences.Get($"{prefix}_Usuario", Preferences.Get("Usuario", defaultUser));
            var dbPassword = Preferences.Get($"{prefix}_Senha", Preferences.Get("Senha", string.Empty));
            var dbComplemento = Preferences.Get($"{prefix}_Complemento", Preferences.Get("Complemento", defaultComplemento));

            connectionString = $"Server={dbServer};Database={dbDatabase};User Id={dbUser};Password={dbPassword};{dbComplemento}";
        }

        return (connectionString, databaseType);
    }

    public static bool TentarCriarDiretorioDoBanco(string caminhoArquivo, out string erro)
    {
        erro = string.Empty;
        try
        {
            var diretorio = Path.GetDirectoryName(Path.GetFullPath(caminhoArquivo));
            if (!string.IsNullOrWhiteSpace(diretorio))
            {
                Directory.CreateDirectory(diretorio);
            }
            return true;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ArgumentException or NotSupportedException)
        {
            erro = ex.Message;
            return false;
        }
    }
}
