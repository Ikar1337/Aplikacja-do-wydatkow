using System.Diagnostics;
using System.Windows;
using ProjektPWSW.Wpf.Dtos;
using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.Utils;

namespace ProjektPWSW.Wpf.ViewModels;

public class ReportsVm : ViewModelBase
{
    private readonly ApiClient _api;

    public int Year { get; set; } = DateTime.Today.Year;
    public int Month { get; set; } = DateTime.Today.Month;

    private string _mode = "monthly";
    public string Mode
    {
        get => _mode;
        set
        {
            _mode = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(IsMonthly));
            OnPropertyChanged(nameof(IsYearly));
            OnPropertyChanged(nameof(IsRange));
        }
    }

    public bool IsMonthly => Mode == "monthly";
    public bool IsYearly => Mode == "yearly";
    public bool IsRange => Mode == "range";

    private DateTime? _from;
    public DateTime? From
    {
        get => _from;
        set { _from = value; OnPropertyChanged(); }
    }

    private DateTime? _to;
    public DateTime? To
    {
        get => _to;
        set { _to = value; OnPropertyChanged(); }
    }

    private string _lastPath = "";
    public string LastPath
    {
        get => _lastPath;
        set { _lastPath = value; OnPropertyChanged(); }
    }

    private string _reportContent = "";
    public string ReportContent
    {
        get => _reportContent;
        set { _reportContent = value; OnPropertyChanged(); }
    }

    public RelayCommand GenerateCmd { get; }

    public ReportsVm(ApiClient api)
    {
        _api = api;
        GenerateCmd = new RelayCommand(GenerateAsync);
    }

    private async Task GenerateAsync()
    {
        try
        {
            string url;

            if (Mode == "yearly")
            {
                url = $"api/reports/yearly?year={Year}";
            }
            else if (Mode == "range")
            {
                if (From == null || To == null)
                {
                    MessageBox.Show("Ustaw daty Od/Do.");
                    return;
                }

                url = $"api/reports/range?from={From:yyyy-MM-dd}&to={To:yyyy-MM-dd}";
            }
            else // monthly
            {
                url = $"api/reports/monthly?year={Year}&month={Month}";
            }

            // API zwraca: { path, content }
            var res = await _api.GetAsync<ReportResultDto>(url);

            LastPath = res.Path;
            ReportContent = res.Content;

            // Otwarcie pliku od razu po wygenerowaniu
            try
            {
                Process.Start(new ProcessStartInfo
                {
                    FileName = LastPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show("Raport wygenerowany, ale nie udało się go otworzyć.\n" + ex.Message);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }
}
