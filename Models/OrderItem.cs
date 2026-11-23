using System.Text.Json.Serialization;

namespace RestaurantManager.Models;

public class OrderItem
{
    private string _name = string.Empty;
    private decimal _unitPrice;
    private int _quantity;

    public int MenuItemId { get; set; }

    public string Name
    {
        get => _name;
        set => _name = string.IsNullOrWhiteSpace(value)
            ? throw new ArgumentException("Название блюда не может быть пустым.")
            : value.Trim();
    }

    public decimal UnitPrice
    {
        get => _unitPrice;
        set => _unitPrice = value <= 0
            ? throw new ArgumentException("Цена должна быть больше нуля.")
            : value;
    }

    public int Quantity
    {
        get => _quantity;
        set => _quantity = value <= 0
            ? throw new ArgumentException("Количество должно быть больше нуля.")
            : value;
    }

    // Стоимость позиции (в JSON не сохраняется, считается при каждом обращении)
    [JsonIgnore]
    public decimal Total => UnitPrice * Quantity;

    // Нужен для загрузки из JSON
    public OrderItem() { }

    public OrderItem(MenuItem menuItem, int quantity)
    {
        MenuItemId = menuItem.Id;
        Name = menuItem.Name;
        UnitPrice = menuItem.Price;
        Quantity = quantity;
    }

    public override string ToString() => $"{Name} x{Quantity} = {Total:F2} руб.";
}