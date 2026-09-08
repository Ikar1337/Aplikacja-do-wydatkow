using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using System.Web;
using System.Windows;
using Microsoft.Win32;
using ProjektPWSW.Wpf.Dtos;
using ProjektPWSW.Wpf.Models;
using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.Utils;

namespace ProjektPWSW.Wpf.ViewModels;

public class ExpensesVm : ViewModelBase
{
    private readonly ApiClient _api;

    public ObservableCollection<Expense> Items { get; } = new();
    public ObservableCollection<Category> Categories { get; } = new();

    private Expense? _selected;
    public Expense? Selected
    {
        get => _selected;
        set
        {
            _selected = value;
            OnPropertyChanged();

            if (_selected != null)
            {
                Date = _selected.Date;
                Amount = _selected.Amount;
                Description = _selected.Description;

                SelectedCategory = Categories.FirstOrDefault(c => c.Id == _selected.CategoryId);
            }

            DeleteCmd.RaiseCanExecuteChanged();
            UpdateCmd.RaiseCanExecuteChanged();
        }
    }

    private DateTime _date = DateTime.Today;
    public DateTime Date { get => _date; set { _date = value; OnPropertyChanged(); } }

    private decimal _amount;
    public decimal Amount { get => _amount; set { _amount = value; OnPropertyChanged(); } }

    private string? _description;
    public string? Description { get => _description; set { _description = value; OnPropertyChanged(); } }

    private Category? _selectedCategory;
    public Category? SelectedCategory { get => _selectedCategory; set { _selectedCategory = value; OnPropertyChanged(); } }

    public DateTime? From { get; set; }
    public DateTime? To { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }

    private Category? _filterCategory;
    public Category? FilterCategory
    {
        get => _filterCategory;
        set { _filterCategory = value; OnPropertyChanged(); }
    }

    private string _sort = "date_desc";
    public string Sort
    {
        get => _sort;
        set { _sort = value; OnPropertyChanged(); }
    }

    public RelayCommand RefreshCmd { get; }
    public RelayCommand AddCmd { get; }
    public RelayCommand DeleteCmd { get; }
    public RelayCommand ApplyFiltersCmd { get; }
    public RelayCommand LoadCategoriesCmd { get; }
    public RelayCommand UpdateCmd { get; }

    // <-- NOWA KOMENDA DO WGRYWANIA PARAGONU -->
    public RelayCommand UploadReceiptCmd { get; }

    public ExpensesVm(ApiClient api)
    {
        _api = api;

        AddCmd = new RelayCommand(AddAsync);
        UpdateCmd = new RelayCommand(UpdateAsync, () => Selected != null);
        DeleteCmd = new RelayCommand(DeleteAsync, () => Selected != null);
        ApplyFiltersCmd = new RelayCommand(LoadFilteredAsync);
        LoadCategoriesCmd = new RelayCommand(LoadCategoriesAsync);

        // <-- PODPIĘCIE NOWEJ KOMENDY -->
        UploadReceiptCmd = new RelayCommand(UploadReceiptAsync);

        RefreshCmd = new RelayCommand(async () =>
        {
            await LoadCategoriesAsync();
            await LoadAsync();
        });
    }

    public async Task LoadCategoriesAsync()
    {
        Categories.Clear();

        Categories.Add(new Category { Id = 0, Name = "(Wszystkie)", Type = "" });

        var list = await _api.GetListAsync<Category>("api/categories");
        foreach (var c in list) Categories.Add(c);

        if (SelectedCategory == null && Categories.Count > 1)
            SelectedCategory = Categories[1];

        if (FilterCategory == null)
            FilterCategory = Categories[0];
    }

    public async Task LoadAsync()
    {
        Items.Clear();
        var list = await _api.GetListAsync<Expense>("api/expenses");
        foreach (var e in list) Items.Add(e);
    }

    private async Task LoadFilteredAsync()
    {
        var qs = HttpUtility.ParseQueryString(string.Empty);

        if (From.HasValue) qs["from"] = From.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (To.HasValue) qs["to"] = To.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        if (FilterCategory != null && FilterCategory.Id != 0)
            qs["categoryId"] = FilterCategory.Id.ToString();
        if (MinAmount.HasValue) qs["minAmount"] = MinAmount.Value.ToString(CultureInfo.InvariantCulture);
        if (MaxAmount.HasValue) qs["maxAmount"] = MaxAmount.Value.ToString(CultureInfo.InvariantCulture);
        if (!string.IsNullOrWhiteSpace(Sort))
            qs["sort"] = Sort;

        var url = $"api/expenses/filter?{qs}";
        Items.Clear();
        var list = await _api.GetListAsync<Expense>(url);
        foreach (var e in list) Items.Add(e);
    }

    private async Task AddAsync()
    {
        if (SelectedCategory == null)
        {
            MessageBox.Show("Najpierw dodaj kategorię.");
            return;
        }

        try
        {
            var dto = new CreateExpenseDto
            {
                Date = Date,
                Amount = Amount,
                Description = Description,
                CategoryId = SelectedCategory.Id
            };

            var created = await _api.PostAsync<Expense>("api/expenses", dto);
            if (created != null) Items.Insert(0, created);

            Amount = 0;
            Description = "";
            Date = DateTime.Today;
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
            await _api.DeleteAsync($"api/expenses/{Selected.Id}");
            Items.Remove(Selected);
            Selected = null;
            DeleteCmd.RaiseCanExecuteChanged();
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    private async Task UpdateAsync()
    {
        if (Selected == null || SelectedCategory == null) return;

        try
        {
            var dto = new UpdateExpenseDto
            {
                Date = Date,
                Amount = Amount,
                Description = Description,
                CategoryId = SelectedCategory.Id
            };

            await _api.PutAsync($"api/expenses/{Selected.Id}", dto);
            await LoadAsync();

            Selected = null;
        }
        catch (Exception ex)
        {
            MessageBox.Show(ex.Message, "Błąd");
        }
    }

    // <-- NOWA METODA DO OBSŁUGI WGRYWANIA PLIKU -->
    private async Task UploadReceiptAsync()
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Obrazy i PDF (*.jpg;*.jpeg;*.png;*.pdf)|*.jpg;*.jpeg;*.png;*.pdf",
            Title = "Wybierz plik z rachunkiem"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var parsedItems = await _api.PostFileAsync<List<ParsedExpenseDto>>(
                    "api/expenses/parse-receipt",
                    openFileDialog.FileName);

                if (parsedItems != null && parsedItems.Count > 0)
                {
                    // Bierzemy kategorie (ale bez pierwszej, czyli "(Wszystkie)")
                    var availableCategories = Categories.Where(c => c.Id != 0).ToList();

                    // Otwieramy nasze nowe okno weryfikacji!
                    var window = new ReceiptVerificationWindow(parsedItems, availableCategories);
                    window.Owner = Application.Current.MainWindow;

                    // Jeśli użytkownik w oknie kliknął "Zapisz wydatki" i wszystko przeszło pomyślnie:
                    if (window.ShowDialog() == true)
                    {
                        int savedCount = 0;

                        // Zapisujemy po kolei do bazy każdą pozycję z okna
                        foreach (var item in window.FinalItems)
                        {
                            var dto = new CreateExpenseDto
                            {
                                Date = DateTime.Today, // Ustawiamy dzisiejszą datę
                                Amount = item.Amount,
                                Description = item.Description,
                                CategoryId = item.CategoryId.Value
                            };

                            await _api.PostAsync<Expense>("api/expenses", dto);
                            savedCount++;
                        }

                        MessageBox.Show($"Zapisano {savedCount} nowych wydatków do bazy!", "Sukces", MessageBoxButton.OK, MessageBoxImage.Information);

                        // Odświeżamy listę na głównym ekranie, żeby od razu było widać nowe wydatki
                        await LoadAsync();
                    }
                }
                else
                {
                    MessageBox.Show("Nie udało się odczytać żadnych pozycji.", "Brak danych", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd:\n{ex.Message}", "Błąd", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}