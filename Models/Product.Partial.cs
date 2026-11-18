namespace Lab3_Shop_Baimurzin.Models;

// Вычисляемые свойства для отображения в карточке товара
public partial class Product
{
    public const double BigDiscountThreshold = 20;

    public string CategoryName => Category?.Name?.Trim() ?? "";
    public string ProductTitle => Name?.Name?.Trim() ?? "";
    public string ManufacturerName => Manufacturer?.Name?.Trim() ?? "";
    public string SupplierName => Supplier?.Name?.Trim() ?? "";

    public double DiscountValue => Discount ?? 0;
    public bool HasDiscount => DiscountValue > 0;
    public bool IsBigDiscount => DiscountValue > BigDiscountThreshold;

    public double FinalPrice => (Price ?? 0) * (1 - DiscountValue / 100);}
