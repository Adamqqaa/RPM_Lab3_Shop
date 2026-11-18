using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Lab3_Shop_Baimurzin.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab3_Shop_Baimurzin;

public partial class ProductListWindow : Window
{
    private readonly ObservableCollection<Product> _products;
    private readonly CollectionViewSource _source;
    public ICollectionView Products { get; set; }

    private string[] _searchWords = [];

    public ProductListWindow()
    {
        InitializeComponent();
        DataContext = this;

        _products = [];
        _source = new() { Source = _products };
        Products = _source.View;
        Products.Filter = FilterProduct;
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

    // Каждое слово запроса должно встретиться хотя бы в одном текстовом поле товара,
    // поэтому можно искать сразу по нескольким атрибутам: "Knauf штукатурка"
    private bool FilterProduct(object obj)
    {
        if (obj is not Product product)
            return false;
        if (_searchWords.Length == 0)
            return true;

        var texts = product.SearchableTexts.ToList();
        return _searchWords.All(word =>
            texts.Any(text => text.Contains(word, StringComparison.CurrentCultureIgnoreCase)));
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        _searchWords = SearchBox.Text.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        Products.Refresh();
        UpdateCounter();
    }

    private void UpdateCounter()
    {
        int shown = Products.Cast<object>().Count();
        CounterText.Text = $"Показано товаров: {shown} из {_products.Count}";
    }
}
