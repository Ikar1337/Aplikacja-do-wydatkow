using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Windows;

namespace ProjektPWSW.Wpf;

public partial class App : Application
{
    private Process? _apiProcess;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Szukamy pliku API w tym samym folderze, co plik WPF
        var apiPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ProjektPWSW.Api.exe");

        if (File.Exists(apiPath))
        {
            // Odpalamy API w tle (bez czarnego okienka konsoli)
            var startInfo = new ProcessStartInfo
            {
                FileName = apiPath,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _apiProcess = Process.Start(startInfo);
        }
        else
        {
            MessageBox.Show("Nie znaleziono pliku API (ProjektPWSW.Api.exe) w folderze z aplikacją!", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    protected override void OnExit(ExitEventArgs e)
    {
        // Kiedy znajomy zamyka okienko WPF, zabijamy też proces API w tle
        if (_apiProcess != null && !_apiProcess.HasExited)
        {
            _apiProcess.Kill();
        }

        base.OnExit(e);
    }
}