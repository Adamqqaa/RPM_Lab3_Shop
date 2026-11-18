using Microsoft.EntityFrameworkCore;

namespace Lab3_Shop_Baimurzin.Models;

// Отдельный partial-файл, чтобы повторный scaffold не затирал строку подключения
public partial class ShopContext
{
    private const string ConnectionString =
        @"Server=.\SQLEXPRESS;Database=Lab3_Shop_Баймурзин;Trusted_Connection=True;TrustServerCertificate=True";

    public ShopContext()
    {
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (!optionsBuilder.IsConfigured)
            optionsBuilder.UseSqlServer(ConnectionString);
    }
}
