using System.Collections.Generic;
using System.Windows;
using ProjektPWSW.Wpf.Dtos;
using ProjektPWSW.Wpf.Models;

namespace ProjektPWSW.Wpf;

public partial class ReceiptVerificationWindow : Window
{
    // Lista kategorii do ComboBoxów
    public List<Category> Categories { get; set; }

    // Lista, którą wyciągniemy po zamknięciu okna
    public List<ParsedExpenseDto> FinalItems { get; private set; }

    public ReceiptVerificationWindow(List<ParsedExpenseDto> parsedItems, List<Category> categories)
    {
        InitializeComponent();

        Categories = categories;
        FinalItems = parsedItems;

        // Podpinamy dane pod DataGrid
        ItemsGrid.ItemsSource = FinalItems;
    }

    private void Save_Click(object sender, RoutedEventArgs e)
    {
        // Sprawdzamy, czy wszystkie pozycje mają wybraną kategorię
        foreach (var item in FinalItems)
        {
            if (item.CategoryId == null || item.CategoryId == 0)
            {
                MessageBox.Show("Uzupełnij wszystkie kategorie przed zapisaniem!", "Uwaga", MessageBoxButton.OK, MessageBoxImage.Warning);
                return; // Zatrzymujemy okno, nie zamykamy
            }
        }

        DialogResult = true; // Zamykamy z sukcesem
        Close();
    }

    private void Cancel_Click(object sender, RoutedEventArgs e)
    {
        DialogResult = false; // Zamykamy bez zapisywania
        Close();
    }
}