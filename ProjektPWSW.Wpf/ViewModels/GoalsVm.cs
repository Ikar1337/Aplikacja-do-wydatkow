using System.Collections.ObjectModel;
using System.Windows;
using ProjektPWSW.Wpf.Dtos;
using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.Utils;

namespace ProjektPWSW.Wpf.ViewModels;

public class GoalsVm : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<SavingGoalDto> Items { get; } = new();

    private SavingGoalDto? _selected;
    public SavingGoalDto? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged();
            FillForm();

            UpdateCmd.RaiseCanExecuteChanged();
            DeleteCmd.RaiseCanExecuteChanged();
            AddAmountCmd.RaiseCanExecuteChanged();
        }
    }

    public string Name { get; set; } = "";
    public decimal TargetAmount { get; set; }

    private DateTime? _targetDate = DateTime.Today.AddMonths(6);
    public DateTime? TargetDate
    {
        get => _targetDate;
        set { _targetDate = value; OnPropertyChanged(); }
    }

    public decimal CurrentAmount { get; set; }

    public decimal AddAmountValue { get; set; }

    public RelayCommand RefreshCmd { get; }
    public RelayCommand AddCmd { get; }
    public RelayCommand UpdateCmd { get; }
    public RelayCommand DeleteCmd { get; }
    public RelayCommand AddAmountCmd { get; }

    public GoalsVm(ApiClient api)
    {
        _api = api;
        RefreshCmd = new RelayCommand(LoadAsync);
        AddCmd = new RelayCommand(AddAsync);
        UpdateCmd = new RelayCommand(UpdateAsync, () => Selected != null);
        DeleteCmd = new RelayCommand(DeleteAsync, () => Selected != null);
        AddAmountCmd = new RelayCommand(AddAmountAsync, () => Selected != null);
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _api.GetListAsync<SavingGoalDto>("api/savinggoals");
        foreach (var g in list) Items.Add(g);
    }

    private async Task AddAsync()
    {
        try
        {
            var dto = new CreateSavingGoalDto
            {
                Name = Name,
                TargetAmount = TargetAmount,
                TargetDate = TargetDate,
                CurrentAmount = CurrentAmount
            };

            var created = await _api.PostAsync<SavingGoalDto>("api/savinggoals", dto);
            if (created != null) Items.Add(created);
            ClearForm();
            UpdateCmd.RaiseCanExecuteChanged();
            DeleteCmd.RaiseCanExecuteChanged();
            AddAmountCmd.RaiseCanExecuteChanged();

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
            var dto = new UpdateSavingGoalDto
            {
                Name = Name,
                TargetAmount = TargetAmount,
                TargetDate = TargetDate,
                CurrentAmount = CurrentAmount
            };

            await _api.PutAsync($"api/savinggoals/{Selected.Id}", dto);
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
            await _api.DeleteAsync($"api/savinggoals/{Selected.Id}");
            Items.Remove(Selected);
            ClearForm();
            UpdateCmd.RaiseCanExecuteChanged();
            DeleteCmd.RaiseCanExecuteChanged();
            AddAmountCmd.RaiseCanExecuteChanged();

        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    private async Task AddAmountAsync()
    {
        if (Selected == null) return;
        try
        {
            var updated = await _api.PostAsync<SavingGoalDto>($"api/savinggoals/{Selected.Id}/add?amount={AddAmountValue}", new { });
            if (updated != null)
            {
                await LoadAsync();
                AddAmountValue = 0;
                OnPropertyChanged(nameof(AddAmountValue));
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    private void FillForm()
    {
        if (Selected == null) return;
        Name = Selected.Name;
        TargetAmount = Selected.TargetAmount;
        TargetDate = Selected.TargetDate;
        CurrentAmount = Selected.CurrentAmount;

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(TargetAmount));
        OnPropertyChanged(nameof(TargetDate));
        OnPropertyChanged(nameof(CurrentAmount));
    }

    private void ClearForm()
    {
        Selected = null;
        Name = "";
        TargetAmount = 0;
        TargetDate = DateTime.Today.AddMonths(6);
        CurrentAmount = 0;

        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(TargetAmount));
        OnPropertyChanged(nameof(TargetDate));
        OnPropertyChanged(nameof(CurrentAmount));
    }
}
