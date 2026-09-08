using System.Collections.ObjectModel;
using System.Windows;
using ProjektPWSW.Wpf.Dtos;
using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.Utils;

namespace ProjektPWSW.Wpf.ViewModels;

public class SummaryVm : ViewModelBase
{
    private readonly ApiClient _api;

    private int _year = DateTime.Today.Year;
    public int Year
    {
        get => _year;
        set { _year = value; OnPropertyChanged(); }
    }

    private int _month = DateTime.Today.Month;
    public int Month
    {
        get => _month;
        set { _month = value; OnPropertyChanged(); }
    }

    private string _mode = "monthly";
    public string Mode
    {
        get => _mode;
        set { _mode = value; OnPropertyChanged(); }
    }
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

    private decimal _total;
    public decimal Total { get => _total; set { _total = value; OnPropertyChanged(); } }

    private int _count;
    public int Count { get => _count; set { _count = value; OnPropertyChanged(); } }

    public ObservableCollection<CategorySummaryDto> Categories { get; } = new();

    public RelayCommand LoadCmd { get; }

    public SummaryVm(ApiClient api)
    {
        _api = api;
        LoadCmd = new RelayCommand(LoadAsync);
    }

    private async Task LoadAsync()
    {
        try
        {
            string url;

            if (Mode == "yearly")
            {
                url = $"api/summary/yearly?year={Year}";
            }
            else if (Mode == "range")
            {
                if (From == null || To == null)
                {
                    MessageBox.Show("Ustaw daty Od/Do.");
                    return;
                }

                url = $"api/summary/range?from={From:yyyy-MM-dd}&to={To:yyyy-MM-dd}";
            }
            else // monthly
            {
                url = $"api/summary/monthly?year={Year}&month={Month}";
            }

            var dto = await _api.GetAsync<MonthlySummaryDto>(url);

            Total = dto.TotalAmount;
            Count = dto.TransactionCount;

            Categories.Clear();
            foreach (var c in dto.Categories) Categories.Add(c);
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

}
