using RestaurantManager.Models;
using RestaurantManager.Services;
using RestaurantManager.Storage;

namespace RestaurantManager.UI;

public class ConsoleApp
{
    private readonly MenuService _menu;
    private readonly OrderService _orders;
    private readonly ReportService _reports;
    private readonly IRepository _repository;

    public ConsoleApp(MenuService menu, OrderService orders, ReportService reports, IRepository repository)
    {
        _menu = menu;
        _orders = orders;
        _reports = reports;
        _repository = repository;
    }

    public void Run()
    {
        Console.WriteLine("=== Система управления рестораном ===");

        RunMenu("Главное меню", "Выход", new (string, Action)[]
        {
            ("Блюда меню", DishesMenu),
            ("Категории", CategoriesMenu),
            ("Заказы", OrdersMenu),
            ("Отчёт о продажах", ShowSalesReport)
        });

        Console.WriteLine("До свидания!");
    }

    // ---------- Общая логика меню ----------

    private void RunMenu(string title, string exitText, (string Text, Action Action)[] items)
    {
        while (true)
        {
            Console.WriteLine();
            Console.WriteLine($"--- {title} ---");
            for (var i = 0; i < items.Length; i++)
                Console.WriteLine($"{i + 1}. {items[i].Text}");
            Console.WriteLine($"0. {exitText}");

            var choice = ConsoleHelper.ReadInt("Выберите пункт: ", 0, items.Length);
            if (choice == 0)
                return;

            Execute(items[choice - 1].Action);
        }
    }

    // Выполняет действие; ошибки показывает пользователю, а не роняет программу.
    // После успешного действия данные сохраняются в файл.
    private void Execute(Action action)
    {
        try
        {
            action();
            Save();
        }
        catch (Exception ex) when (ex is ArgumentException or InvalidOperationException or KeyNotFoundException)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Непредвиденная ошибка: {ex.Message}");
        }
    }

    private void Save()
    {
        try
        {
            _repository.Save(new AppData
            {
                Categories = _menu.Categories.ToList(),
                MenuItems = _menu.Items.ToList(),
                Orders = _orders.Orders.ToList()
            });
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException)
        {
            Console.WriteLine($"Не удалось сохранить данные: {ex.Message}");
        }
    }

    // ======================= БЛЮДА =======================

    private void DishesMenu() =>
        RunMenu("Блюда меню", "Назад", new (string, Action)[]
        {
            ("Показать меню", ShowMenu),
            ("Добавить блюдо", AddDish),
            ("Изменить блюдо", EditDish),
            ("Удалить блюдо", DeleteDish)
        });

    private void ShowMenu()
    {
        if (_menu.Items.Count == 0)
        {
            Console.WriteLine("Меню пока пусто.");
            return;
        }

        foreach (var category in _menu.Categories)
        {
            var items = _menu.Items.Where(i => i.CategoryId == category.Id).ToList();
            if (items.Count == 0)
                continue;

            Console.WriteLine();
            Console.WriteLine($"[{category.Name}]");
            foreach (var item in items)
                PrintItem(item);
        }
    }

    private static void PrintItem(MenuItem item)
    {
        var description = item.Description.Length > 0 ? $" ({item.Description})" : "";
        Console.WriteLine($"  {item}{description}");
    }

    private void AddDish()
    {
        var category = PickCategory("Номер категории для блюда");
        if (category == null)
            return;

        var name = ConsoleHelper.ReadString("Название: ");
        var price = ConsoleHelper.ReadDecimal("Цена: ");
        var description = ConsoleHelper.ReadLine("Описание (Enter — пропустить): ");

        var item = _menu.AddItem(name, price, category.Id, description);
        Console.WriteLine($"Блюдо добавлено: {item}");
    }

    private void EditDish()
    {
        var item = PickItem("Номер блюда для изменения");
        if (item == null)
            return;

        Console.WriteLine($"Текущие данные: {item}, категория: {CategoryName(item.CategoryId)}");

        var name = ConsoleHelper.ReadLine($"Новое название (Enter — оставить «{item.Name}»): ");
        if (name.Length == 0)
            name = item.Name;

        var price = ConsoleHelper.ReadOptionalDecimal($"Новая цена (Enter — оставить {item.Price:F2}): ")
                    ?? item.Price;

        var description = ConsoleHelper.ReadLine("Новое описание (Enter — оставить, «-» — очистить): ");
        if (description.Length == 0)
            description = item.Description;
        else if (description == "-")
            description = "";

        var categoryId = item.CategoryId;
        if (ConsoleHelper.Confirm("Изменить категорию?"))
        {
            var category = PickCategory("Номер новой категории");
            if (category == null)
                return;
            categoryId = category.Id;
        }

        _menu.UpdateItem(item.Id, name, price, categoryId, description);
        Console.WriteLine("Блюдо обновлено.");
    }

    private void DeleteDish()
    {
        var item = PickItem("Номер блюда для удаления");
        if (item == null)
            return;

        if (!ConsoleHelper.Confirm($"Удалить блюдо «{item.Name}»?"))
        {
            Console.WriteLine("Отменено.");
            return;
        }

        _menu.DeleteItem(item.Id);
        Console.WriteLine("Блюдо удалено.");
    }

    // ===================== КАТЕГОРИИ =====================

    private void CategoriesMenu() =>
        RunMenu("Категории", "Назад", new (string, Action)[]
        {
            ("Показать категории", ShowCategories),
            ("Добавить категорию", AddCategory),
            ("Переименовать категорию", RenameCategory),
            ("Удалить категорию", DeleteCategory)
        });

    private void ShowCategories()
    {
        if (_menu.Categories.Count == 0)
        {
            Console.WriteLine("Категорий пока нет.");
            return;
        }

        foreach (var category in _menu.Categories)
        {
            var count = _menu.Items.Count(i => i.CategoryId == category.Id);
            Console.WriteLine($"  {category} — блюд: {count}");
        }
    }

    private void AddCategory()
    {
        var name = ConsoleHelper.ReadString("Название категории: ");
        var category = _menu.AddCategory(name);
        Console.WriteLine($"Категория добавлена: {category}");
    }

    private void RenameCategory()
    {
        var category = PickCategory("Номер категории для переименования");
        if (category == null)
            return;

        var newName = ConsoleHelper.ReadString($"Новое название для «{category.Name}»: ");
        _menu.RenameCategory(category.Id, newName);
        Console.WriteLine("Категория переименована.");
    }

    private void DeleteCategory()
    {
        var category = PickCategory("Номер категории для удаления");
        if (category == null)
            return;

        if (!ConsoleHelper.Confirm($"Удалить категорию «{category.Name}»?"))
        {
            Console.WriteLine("Отменено.");
            return;
        }

        _menu.DeleteCategory(category.Id);
        Console.WriteLine("Категория удалена.");
    }

    // ======================= ЗАКАЗЫ =======================

    private void OrdersMenu() =>
        RunMenu("Заказы", "Назад", new (string, Action)[]
        {
            ("Создать заказ", CreateOrder),
            ("Показать все заказы", ShowOrders),
            ("Показать заказ подробно", ShowOrderDetails),
            ("Добавить блюдо в заказ", AddItemToOrder),
            ("Убрать блюдо из заказа", RemoveItemFromOrder),
            ("Изменить статус заказа", ChangeOrderStatus)
        });

    private void CreateOrder()
    {
        if (_menu.Items.Count == 0)
        {
            Console.WriteLine("В меню нет блюд. Сначала добавьте блюда.");
            return;
        }

        ShowMenu();
        Console.WriteLine();
        Console.WriteLine("Добавляйте позиции в заказ. Для завершения введите 0 вместо номера блюда.");

        var positions = new List<(int MenuItemId, int Quantity)>();
        while (true)
        {
            var id = ConsoleHelper.ReadInt("Номер блюда (0 — завершить): ", 0);
            if (id == 0)
                break;

            var item = _menu.GetItemById(id);
            if (item == null)
            {
                Console.WriteLine("Блюда с таким номером нет.");
                continue;
            }

            var quantity = ConsoleHelper.ReadInt($"Количество «{item.Name}»: ", 1, 100);
            positions.Add((item.Id, quantity));
            Console.WriteLine($"Добавлено: {item.Name} x{quantity}");
        }

        if (positions.Count == 0)
        {
            Console.WriteLine("Заказ не создан: в нём нет позиций.");
            return;
        }

        var order = _orders.CreateOrder(positions);
        Console.WriteLine();
        Console.WriteLine("Заказ создан:");
        PrintOrderDetails(order);
    }

    private void ShowOrders()
    {
        if (_orders.Orders.Count == 0)
        {
            Console.WriteLine("Заказов пока нет.");
            return;
        }

        foreach (var order in _orders.Orders)
            Console.WriteLine($"  {FormatOrder(order)}");
    }

    private void ShowOrderDetails()
    {
        var order = PickOrder("Номер заказа");
        if (order == null)
            return;

        Console.WriteLine();
        PrintOrderDetails(order);
    }

    private void AddItemToOrder()
    {
        var order = PickOrder("Номер заказа");
        if (order == null)
            return;

        if (order.Status != OrderStatus.New)
        {
            Console.WriteLine("Изменять состав можно только у нового заказа.");
            return;
        }

        var item = PickItem("Номер блюда для добавления");
        if (item == null)
            return;

        var quantity = ConsoleHelper.ReadInt($"Количество «{item.Name}»: ", 1, 100);
        _orders.AddItemToOrder(order.Id, item.Id, quantity);

        Console.WriteLine();
        PrintOrderDetails(order);
    }

    private void RemoveItemFromOrder()
    {
        var order = PickOrder("Номер заказа");
        if (order == null)
            return;

        if (order.Status != OrderStatus.New)
        {
            Console.WriteLine("Изменять состав можно только у нового заказа.");
            return;
        }

        if (order.Items.Count == 0)
        {
            Console.WriteLine("В заказе нет позиций.");
            return;
        }

        Console.WriteLine();
        PrintOrderDetails(order);

        var id = ConsoleHelper.ReadInt("Номер блюда для удаления из заказа (0 — отмена): ", 0);
        if (id == 0)
        {
            Console.WriteLine("Отменено.");
            return;
        }

        _orders.RemoveItemFromOrder(order.Id, id);
        Console.WriteLine("Позиция удалена.");
    }

    private void ChangeOrderStatus()
    {
        var order = PickOrder("Номер заказа");
        if (order == null)
            return;

        // Показываем только те статусы, в которые из текущего можно перейти
        var allowed = Enum.GetValues<OrderStatus>().Where(order.CanChangeStatusTo).ToList();
        if (allowed.Count == 0)
        {
            Console.WriteLine($"Заказ в статусе «{StatusName(order.Status)}» изменить нельзя.");
            return;
        }

        Console.WriteLine($"Текущий статус: {StatusName(order.Status)}");
        for (var i = 0; i < allowed.Count; i++)
            Console.WriteLine($"{i + 1}. {StatusName(allowed[i])}");

        var choice = ConsoleHelper.ReadInt("Новый статус (0 — отмена): ", 0, allowed.Count);
        if (choice == 0)
        {
            Console.WriteLine("Отменено.");
            return;
        }

        _orders.ChangeStatus(order.Id, allowed[choice - 1]);
        Console.WriteLine($"Статус изменён: {StatusName(order.Status)}");
    }

    private static string FormatOrder(Order order) =>
        $"Заказ №{order.Id} от {order.CreatedAt:dd.MM.yyyy HH:mm} [{StatusName(order.Status)}] — {order.CalculateTotal():F2} руб.";

    private static void PrintOrderDetails(Order order)
    {
        Console.WriteLine(FormatOrder(order));

        if (order.Items.Count == 0)
        {
            Console.WriteLine("  (позиций нет)");
            return;
        }

        foreach (var item in order.Items)
            Console.WriteLine($"  {item.MenuItemId}. {item}");

        Console.WriteLine($"Итого: {order.CalculateTotal():F2} руб. (блюд: {order.TotalQuantity()})");
    }

    private static string StatusName(OrderStatus status) => status switch
    {
        OrderStatus.New => "Новый",
        OrderStatus.InProgress => "Готовится",
        OrderStatus.Ready => "Готов",
        OrderStatus.Paid => "Оплачен",
        OrderStatus.Cancelled => "Отменён",
        _ => status.ToString()
    };

    // ======================= ОТЧЁТ =======================

    private void ShowSalesReport()
    {
        Console.WriteLine("Укажите период отчёта (Enter — без ограничения).");
        var from = ConsoleHelper.ReadOptionalDate("Начало периода (дд.мм.гггг): ");
        var to = ConsoleHelper.ReadOptionalDate("Конец периода (дд.мм.гггг): ");

        var report = _reports.GenerateSalesReport(from, to);
        Console.WriteLine();
        Console.Write(_reports.Format(report));
    }

    // ================= ВЫБОР ИЗ СПИСКА =================

    private Category? PickCategory(string prompt)
    {
        if (_menu.Categories.Count == 0)
        {
            Console.WriteLine("Категорий пока нет. Сначала добавьте категорию.");
            return null;
        }

        ShowCategories();
        while (true)
        {
            var id = ConsoleHelper.ReadInt($"{prompt} (0 — отмена): ", 0);
            if (id == 0)
            {
                Console.WriteLine("Отменено.");
                return null;
            }

            var category = _menu.GetCategoryById(id);
            if (category != null)
                return category;
            Console.WriteLine("Категории с таким номером нет.");
        }
    }

    private MenuItem? PickItem(string prompt)
    {
        if (_menu.Items.Count == 0)
        {
            Console.WriteLine("Меню пока пусто.");
            return null;
        }

        ShowMenu();
        Console.WriteLine();
        while (true)
        {
            var id = ConsoleHelper.ReadInt($"{prompt} (0 — отмена): ", 0);
            if (id == 0)
            {
                Console.WriteLine("Отменено.");
                return null;
            }

            var item = _menu.GetItemById(id);
            if (item != null)
                return item;
            Console.WriteLine("Блюда с таким номером нет.");
        }
    }

    private Order? PickOrder(string prompt)
    {
        if (_orders.Orders.Count == 0)
        {
            Console.WriteLine("Заказов пока нет.");
            return null;
        }

        ShowOrders();
        while (true)
        {
            var id = ConsoleHelper.ReadInt($"{prompt} (0 — отмена): ", 0);
            if (id == 0)
            {
                Console.WriteLine("Отменено.");
                return null;
            }

            var order = _orders.GetOrderById(id);
            if (order != null)
                return order;
            Console.WriteLine("Заказа с таким номером нет.");
        }
    }

    private string CategoryName(int categoryId) =>
        _menu.GetCategoryById(categoryId)?.Name ?? "—";
}