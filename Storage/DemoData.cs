using RestaurantManager.Models;
using RestaurantManager.Services;

namespace RestaurantManager.Storage;

// Готовый набор данных для демонстрации программы
public static class DemoData
{
    public static AppData Create()
    {
        var menu = new MenuService();

        var salads = menu.AddCategory("Салаты");
        var soups = menu.AddCategory("Супы");
        var mains = menu.AddCategory("Горячие блюда");
        var desserts = menu.AddCategory("Десерты");
        var drinks = menu.AddCategory("Напитки");

        var caesar = menu.AddItem("Цезарь", 450, salads.Id, "С куриной грудкой и пармезаном");
        var greek = menu.AddItem("Греческий салат", 380, salads.Id, "Овощи, фета, оливки");
        var borscht = menu.AddItem("Борщ", 300, soups.Id, "Со сметаной");
        var mushroomSoup = menu.AddItem("Грибной крем-суп", 330, soups.Id);
        var steak = menu.AddItem("Стейк из говядины", 1200, mains.Id, "С овощами гриль");
        var pasta = menu.AddItem("Паста карбонара", 520, mains.Id);
        var cheesecake = menu.AddItem("Чизкейк", 280, desserts.Id, "Нью-Йорк");
        var tiramisu = menu.AddItem("Тирамису", 310, desserts.Id);
        var juice = menu.AddItem("Морс клюквенный", 150, drinks.Id);
        var coffee = menu.AddItem("Капучино", 200, drinks.Id);
        var tea = menu.AddItem("Чай", 120, drinks.Id);

        var orders = new OrderService(menu);

        // Создаёт заказ, «сдвигает» его дату в прошлое и проводит по статусам до нужного
        void AddOrder(int daysAgo, OrderStatus target, params (MenuItem Item, int Qty)[] lines)
        {
            var order = orders.CreateOrder(lines.Select(l => (l.Item.Id, l.Qty)));
            order.CreatedAt = DateTime.Now.AddDays(-daysAgo).AddMinutes(-15 * order.Id);

            foreach (var step in PathTo(target))
                orders.ChangeStatus(order.Id, step);
        }

        // Оплаченные заказы за последние 6 дней
        AddOrder(6, OrderStatus.Paid, (caesar, 2), (juice, 2));
        AddOrder(5, OrderStatus.Paid, (steak, 1), (coffee, 1), (cheesecake, 1));
        AddOrder(4, OrderStatus.Paid, (borscht, 2), (pasta, 1), (tea, 2));
        AddOrder(3, OrderStatus.Paid, (greek, 1), (mushroomSoup, 1), (tiramisu, 2), (coffee, 2));
        AddOrder(2, OrderStatus.Paid, (steak, 2), (caesar, 1), (juice, 2));
        AddOrder(1, OrderStatus.Paid, (pasta, 2), (cheesecake, 2), (coffee, 2));
        AddOrder(1, OrderStatus.Paid, (borscht, 1), (steak, 1), (tea, 1));
        AddOrder(0, OrderStatus.Paid, (caesar, 1), (pasta, 1), (juice, 1));

        // Отменённый заказ
        AddOrder(2, OrderStatus.Cancelled, (tiramisu, 1));

        // Текущие заказы в разных статусах
        AddOrder(0, OrderStatus.Ready, (greek, 2), (tea, 2));
        AddOrder(0, OrderStatus.InProgress, (steak, 1), (coffee, 1));
        AddOrder(0, OrderStatus.New, (caesar, 1), (juice, 1));

        return new AppData
        {
            Categories = menu.Categories.ToList(),
            MenuItems = menu.Items.ToList(),
            Orders = orders.Orders.ToList()
        };
    }

    // Цепочка статусов, через которую нужно пройти, чтобы дойти до целевого
    private static OrderStatus[] PathTo(OrderStatus target) => target switch
    {
        OrderStatus.InProgress => new[] { OrderStatus.InProgress },
        OrderStatus.Ready => new[] { OrderStatus.InProgress, OrderStatus.Ready },
        OrderStatus.Paid => new[] { OrderStatus.InProgress, OrderStatus.Ready, OrderStatus.Paid },
        OrderStatus.Cancelled => new[] { OrderStatus.Cancelled },
        _ => Array.Empty<OrderStatus>()
    };
}