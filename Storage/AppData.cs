using RestaurantManager.Models;

namespace RestaurantManager.Storage;

// Все данные приложения, которые сохраняются в файл
public class AppData
{
    public List<Category> Categories { get; set; } = new();
    public List<MenuItem> MenuItems { get; set; } = new();
    public List<Order> Orders { get; set; } = new();
}