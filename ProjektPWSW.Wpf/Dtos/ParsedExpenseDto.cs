using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ProjektPWSW.Wpf.Dtos;

// Dodajemy interfejs INotifyPropertyChanged, żeby okienko WPF widziało zmiany 
// (np. gdy użytkownik zmieni kategorię z listy)
public class ParsedExpenseDto : INotifyPropertyChanged
{
    private string _description = string.Empty;
    private decimal _amount;
    private int? _categoryId;

    public string Description
    {
        get => _description;
        set { _description = value; OnPropertyChanged(); }
    }

    public decimal Amount
    {
        get => _amount;
        set { _amount = value; OnPropertyChanged(); }
    }

    public int? CategoryId
    {
        get => _categoryId;
        set { _categoryId = value; OnPropertyChanged(); }
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    protected void OnPropertyChanged([CallerMemberName] string? name = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
    }
}