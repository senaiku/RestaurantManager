namespace RestaurantManager.Models;

public class Category : Entity
{
    private string _name = string.Empty;

    public string Name
    {
        get => _name;
        set
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Название категории не может быть пустым.");
            _name = value.Trim();
        }
    }

    // Нужен для загрузки из JSON
    public Category() { }

    public Category(int id, string name)
    {
        Id = id;
        Name = name;
    }

    public override string ToString() => $"{Id}. {Name}";
}