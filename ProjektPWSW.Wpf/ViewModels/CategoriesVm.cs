using System.Collections.ObjectModel;
using System.Windows;
using ProjektPWSW.Wpf.Models;
using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.Utils;

namespace ProjektPWSW.Wpf.ViewModels;

public class CategoriesVm : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<Category> Items { get; } = new();

    private Category? _selected;
    public Category? Selected
    {
        get => _selected;
        set { _selected = value; OnPropertyChanged(); FillFormFromSelected(); }
    }

    private string _name = "";
    public string Name
    {
        get => _name;
        set { _name = value; OnPropertyChanged(); }
    }

    private string _type = "Wydatek";
    public string Type
    {
        get => _type;
        set { _type = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshCmd { get; }
    public RelayCommand AddCmd { get; }
    public RelayCommand UpdateCmd { get; }
    public RelayCommand DeleteCmd { get; }

    public CategoriesVm(ApiClient api)
    {
        _api = api;

        RefreshCmd = new RelayCommand(LoadAsync);
        AddCmd = new RelayCommand(AddAsync);
        UpdateCmd = new RelayCommand(UpdateAsync, () => Selected != null);
        DeleteCmd = new RelayCommand(DeleteAsync, () => Selected != null);
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _api.GetListAsync<Category>("api/categories");
        foreach (var c in list) Items.Add(c);
    }

    private async Task AddAsync()
    {
        try
        {
            var created = await _api.PostAsync<Category>("api/categories", new { name = Name, type = Type });
            if (created != null) Items.Add(created);
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    private async Task UpdateAsync()
    {
        if (Selected == null) return;
        try
        {
            await _api.PutAsync("api/categories", new { id = Selected.Id, name = Name, type = Type });
            await LoadAsync();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    private async Task DeleteAsync()
    {
        if (Selected == null) return;
        try
        {
            await _api.DeleteAsync($"api/categories/{Selected.Id}");
            Items.Remove(Selected);
            ClearForm();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd (może są wydatki w tej kategorii)");
        }
    }

    private void FillFormFromSelected()
    {
        if (Selected == null) return;
        Name = Selected.Name;
        Type = Selected.Type;
    }

    private void ClearForm()
    {
        Name = "";
        Type = "Wydatek";
        Selected = null;
    }
}
