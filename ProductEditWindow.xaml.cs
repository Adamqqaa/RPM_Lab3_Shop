using System.Globalization;
using System.Windows;
using Lab3_Shop_Baimurzin.Models;
using Microsoft.EntityFrameworkCore;

namespace Lab3_Shop_Baimurzin;

/// <summary>Форма добавления (productId == null) и редактирования товара.</summary>
public partial class ProductEditWindow : Window
{
    private static readonly CultureInfo Ru = CultureInfo.GetCultureInfo("ru-RU");
    private readonly int? _productId;

    public ProductEditWindow(int? productId = null)
    {
        InitializeComponent();
        _productId = productId;
        LoadData();
    }

    private bool IsNew => _productId == null;

    private void LoadData()
    {
        using var db = new ShopContext();

        CategoryBox.ItemsSource = db.Categories.AsNoTracking().OrderBy(c => c.Name).ToList();
        ManufacturerBox.ItemsSource = db.Manufacturers.AsNoTracking().OrderBy(m => m.Name).ToList();
        SupplierBox.ItemsSource = db.Suppliers.AsNoTracking().OrderBy(s => s.Name).ToList();

        if (IsNew)
        {
            Title = "Добавление товара";
            IdLabel.Visibility = Visibility.Collapsed;
            IdBox.Visibility = Visibility.Collapsed;
            DeleteButton.Visibility = Visibility.Collapsed;
            DiscountBox.Text = "0";
            return;
        }

        var product = db.Products.AsNoTracking()
            .Include(p => p.Name)
            .FirstOrDefault(p => p.Id == _productId);
        if (product == null)
        {
            MessageBox.Show("Товар не найден, возможно, он уже удалён.", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            Loaded += (_, _) => Close();
            return;
        }

        Title = "Редактирование товара";
        IdBox.Text = product.Id.ToString();
        ArticleBox.Text = product.Article;
        NameBox.Text = product.Name?.Name?.Trim();
        CategoryBox.SelectedValue = product.CategoryId;
        DescriptionBox.Text = product.Description;
        ManufacturerBox.SelectedValue = product.ManufacturerId;
        SupplierBox.SelectedValue = product.SupplierId;
        PriceBox.Text = (product.Price ?? 0).ToString("0.##", Ru);
        CountBox.Text = (product.Count ?? 0).ToString(Ru);
        DiscountBox.Text = (product.Discount ?? 0).ToString(Ru);
    }

    // Принимает и запятую, и точку в качестве разделителя
    private static bool TryParseNumber(string text, out decimal value) =>
        decimal.TryParse(text.Trim().Replace('.', ','), NumberStyles.Number, Ru, out value);

    private void SaveButton_Click(object sender, RoutedEventArgs e)
    {
        var errors = new List<string>();

        string article = ArticleBox.Text.Trim();
        string name = NameBox.Text.Trim();

        if (article.Length == 0) errors.Add("Укажите артикул.");
        if (name.Length == 0) errors.Add("Укажите наименование товара.");
        if (CategoryBox.SelectedValue == null) errors.Add("Выберите категорию.");
        if (ManufacturerBox.SelectedValue == null) errors.Add("Выберите производителя.");
        if (SupplierBox.SelectedValue == null) errors.Add("Выберите поставщика.");

        if (!TryParseNumber(PriceBox.Text, out decimal price))
            errors.Add("Цена должна быть числом.");
        else if (price < 0)
            errors.Add("Цена не может быть отрицательной.");
        else if (decimal.Round(price, 2) != price)
            errors.Add("Цена может содержать не больше двух знаков после запятой.");

        if (!int.TryParse(CountBox.Text.Trim(), out int count))
            errors.Add("Количество должно быть целым числом.");
        else if (count < 0)
            errors.Add("Количество не может быть отрицательным.");

        if (!TryParseNumber(DiscountBox.Text, out decimal discount))
            errors.Add("Скидка должна быть числом.");
        else if (discount < 0 || discount > 100)
            errors.Add("Скидка должна быть от 0 до 100%.");

        if (errors.Count > 0)
        {
            MessageBox.Show(string.Join("\n", errors), "Проверьте данные",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            using var db = new ShopContext();

            if (db.Products.Any(p => p.Article == article && p.Id != (_productId ?? 0)))
            {
                MessageBox.Show("Товар с таким артикулом уже существует.", "Проверьте данные",
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            Product product;
            if (IsNew)
            {
                // ID вычисляется как максимальный в БД + 1
                int newId = (db.Products.Max(p => (int?)p.Id) ?? 0) + 1;
                product = new Product { Id = newId, Unit = "шт.", ImagePath = "" };
                db.Products.Add(product);
            }
            else
            {
                product = db.Products.First(p => p.Id == _productId);
            }

            product.Article = article;
            product.Name = FindOrCreateProductName(db, name);
            product.CategoryId = (int)CategoryBox.SelectedValue!;
            product.Description = DescriptionBox.Text.Trim();
            product.ManufacturerId = (int)ManufacturerBox.SelectedValue!;
            product.SupplierId = (int)SupplierBox.SelectedValue!;
            product.Price = (double)price;
            product.Count = count;
            product.Discount = (double)discount;

            db.SaveChanges();
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось сохранить товар:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    // Наименования хранятся в отдельной таблице ProductNames
    private static ProductName FindOrCreateProductName(ShopContext db, string name)
    {
        var existing = db.ProductNames.AsEnumerable()
            .FirstOrDefault(n => string.Equals(n.Name?.Trim(), name, StringComparison.CurrentCultureIgnoreCase));
        if (existing != null)
            return existing;

        var created = new ProductName
        {
            Id = (db.ProductNames.Max(n => (int?)n.Id) ?? 0) + 1,
            Name = name,
        };
        db.ProductNames.Add(created);
        return created;
    }

    private void DeleteButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            using var db = new ShopContext();

            if (db.OrderArticles.Any(o => o.ProductId == _productId))
            {
                MessageBox.Show("Этот товар присутствует в заказе, удалить его нельзя.",
                    "Удаление невозможно", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (MessageBox.Show("Удалить товар? Это действие нельзя отменить.", "Подтверждение",
                    MessageBoxButton.YesNo, MessageBoxImage.Question) != MessageBoxResult.Yes)
                return;

            var product = db.Products.FirstOrDefault(p => p.Id == _productId);
            if (product != null)
            {
                db.Products.Remove(product);
                db.SaveChanges();
            }
            DialogResult = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Не удалось удалить товар:\n{ex.Message}", "Ошибка",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
