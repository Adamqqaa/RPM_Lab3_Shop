using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Data;
using Lab3_Shop_Baimurzin.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab3_Shop_Baimurzin;

public partial class ProductListWindow : Window
{
    private readonly ObservableCollection<Product> _products;
    private readonly CollectionViewSource _source;
    public ICollectionView Products { get; set; }

    public ProductListWindow()
    {
        InitializeComponent();
        DataContext = this;

        _products = [];
        _source = new() { Source = _products };
        Products = _source.View;
        ReloadProducts();
    }

    private void ReloadProducts()
    {
        try
        {
            using var db = new ShopContext();
            var items = db.Products
                .AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Name)
                .Include(p => p.Manufacturer)
                .Include(p => p.Supplier)
                .OrderBy(p => p.Id)
                .ToList();

            _products.Clear();
            foreach (var item in items)
                _products.Add(item);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось загрузить товары из базы данных:\n{ex.Message}",
                "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        UpdateCounter();
    }

    private void UpdateCounter()
    {
        CounterText.Text = $"Показано товаров: {_products.Count} из {_products.Count}";
    }
}
