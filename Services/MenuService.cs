using RestaurantManager.Models;

namespace RestaurantManager.Services;

public class MenuService
{
    private readonly List<Category> _categories;
    private readonly List<MenuItem> _items;

    public MenuService() : this(new List<Category>(), new List<MenuItem>()) { }

    // Этот конструктор пригодится для загрузки данных из файла
    public MenuService(IEnumerable<Category> categories, IEnumerable<MenuItem> items)
    {
        _categories = categories.ToList();
        _items = items.ToList();
    }

    public IReadOnlyList<Category> Categories => _categories;
    public IReadOnlyList<MenuItem> Items => _items;

    // ---------- Категории ----------

    public Category AddCategory(string name)
    {
        var category = new Category(NextCategoryId(), name); // название проверяется в модели

        if (_categories.Any(c => c.Name.Equals(category.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Категория «{category.Name}» уже существует.");

        _categories.Add(category);
        return category;
    }

    public void RenameCategory(int id, string newName)
    {
        var category = FindCategory(id);
        var checkName = new Category(id, newName); // проверка корректности названия

        if (_categories.Any(c => c.Id != id &&
                                 c.Name.Equals(checkName.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"Категория «{checkName.Name}» уже существует.");

        category.Name = checkName.Name;
    }

    public void DeleteCategory(int id)
    {
        var category = FindCategory(id);

        if (_items.Any(i => i.CategoryId == id))
            throw new InvalidOperationException(
                "Нельзя удалить категорию, в которой есть блюда. Сначала удалите или перенесите блюда.");

        _categories.Remove(category);
    }

    public Category? GetCategoryById(int id) => _categories.FirstOrDefault(c => c.Id == id);

    // ---------- Блюда ----------

    public MenuItem AddItem(string name, decimal price, int categoryId, string description = "")
    {
        FindCategory(categoryId); // проверяем, что категория существует

        var item = new MenuItem(NextItemId(), name, price, categoryId, description);
        _items.Add(item);
        return item;
    }

    public void UpdateItem(int id, string name, decimal price, int categoryId, string description = "")
    {
        var item = FindItem(id);
        FindCategory(categoryId);

        // Сначала создаём временный объект: если данные некорректны, исключение
        // возникнет до изменения настоящего блюда
        var checkedItem = new MenuItem(id, name, price, categoryId, description);

        item.Name = checkedItem.Name;
        item.Price = checkedItem.Price;
        item.CategoryId = checkedItem.CategoryId;
        item.Description = checkedItem.Description;
    }

    public void DeleteItem(int id)
    {
        var item = FindItem(id);
        _items.Remove(item);
    }

    public MenuItem? GetItemById(int id) => _items.FirstOrDefault(i => i.Id == id);

        // ---------- Поиск, фильтрация, сортировка ----------

    public IEnumerable<MenuItem> SearchItems(
        string? text = null,
        int? categoryId = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        MenuSortOrder sort = MenuSortOrder.ByName)
    {
        if (minPrice.HasValue && maxPrice.HasValue && minPrice > maxPrice)
            throw new ArgumentException("Минимальная цена не может быть больше максимальной.");

        IEnumerable<MenuItem> result = _items;

        if (!string.IsNullOrWhiteSpace(text))
        {
            var query = text.Trim();
            result = result.Where(i =>
                i.Name.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                i.Description.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        if (categoryId.HasValue)
            result = result.Where(i => i.CategoryId == categoryId.Value);

        if (minPrice.HasValue)
            result = result.Where(i => i.Price >= minPrice.Value);

        if (maxPrice.HasValue)
            result = result.Where(i => i.Price <= maxPrice.Value);

        return sort switch
        {
            MenuSortOrder.ByPriceAscending => result.OrderBy(i => i.Price).ThenBy(i => i.Name),
            MenuSortOrder.ByPriceDescending => result.OrderByDescending(i => i.Price).ThenBy(i => i.Name),
            _ => result.OrderBy(i => i.Name, StringComparer.CurrentCultureIgnoreCase)
        };
    }

    // ---------- Вспомогательные методы ----------

    private int NextCategoryId() => IdGenerator.Next(_categories);

    private int NextItemId() => IdGenerator.Next(_items);

    private Category FindCategory(int id) =>
        GetCategoryById(id) ?? throw new KeyNotFoundException($"Категория с Id {id} не найдена.");

    private MenuItem FindItem(int id) =>
        GetItemById(id) ?? throw new KeyNotFoundException($"Блюдо с Id {id} не найдено.");
}