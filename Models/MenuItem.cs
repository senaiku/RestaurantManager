namespace RestaurantManager.Models;

public class MenuItem
{
    private string _name = string.Empty;
    private decimal _price;

    public int Id { get; set; }

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Название блюда не может быть пустым.");
            _name = value.Trim();
        }
    }

    public string Description { get; set; } = string.Empty;

    public decimal Price
    {
        get => _price;
        set
        {
            if (value <= 0)
                throw new ArgumentException("Цена должна быть больше нуля.");
            _price = value;
        }
    }

    // Связь с категорией через Id (так проще хранить в JSON)
    public int CategoryId { get; set; }

    // Нужен для загрузки из JSON
    public MenuItem() { }

    public MenuItem(int id, string name, decimal price, int categoryId, string description = "")
    {
        Id = id;
        Name = name;
        Price = price;
        CategoryId = categoryId;
        Description = description;
    }

    public override string ToString() => $"{Id}. {Name} — {Price:F2} руб.";
}