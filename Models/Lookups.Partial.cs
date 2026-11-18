namespace Lab3_Shop_Baimurzin.Models;

// Нужны выпадающим спискам: по ToString работает поиск по первым буквам
// и экранные дикторы читают название, а не имя класса
public partial class Category
{
    public override string ToString() => Name?.Trim() ?? "";
}

public partial class Manufacturer
{
    public override string ToString() => Name?.Trim() ?? "";
}

public partial class Supplier
{
    public override string ToString() => Name?.Trim() ?? "";
}
