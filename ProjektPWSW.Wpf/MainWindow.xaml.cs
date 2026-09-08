using ProjektPWSW.Wpf.Services;
using ProjektPWSW.Wpf.ViewModels;
using System.Windows;

namespace ProjektPWSW.Wpf;

public partial class MainWindow : Window
{
    private readonly ApiClient _api = new("http://localhost:5000/");

    public CategoriesVm Categories { get; }
    public ExpensesVm Expenses { get; }
    public SummaryVm Summary { get; }
    public GoalsVm Goals { get; }
    public ReportsVm Reports { get; }

    public MainWindow()
    {
        InitializeComponent();

        Categories = new CategoriesVm(_api);
        Expenses = new ExpensesVm(_api);
        Summary = new SummaryVm(_api);
        Goals = new GoalsVm(_api);
        Reports = new ReportsVm(_api);

        DataContext = this;

        Loaded += async (_, __) =>
        {
            await Categories.LoadAsync();
            await Expenses.LoadCategoriesAsync();
            await Expenses.LoadAsync();
            await Goals.LoadAsync();
        };
    }
}
