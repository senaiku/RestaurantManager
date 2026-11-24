using RestaurantManager.Models;

namespace RestaurantManager.Services;

public class OrderService
{
    private readonly MenuService _menuService;
    private readonly List<Order> _orders;

    public OrderService(MenuService menuService) : this(menuService, new List<Order>()) { }

    // Этот конструктор пригодится для загрузки данных из файла
    public OrderService(MenuService menuService, IEnumerable<Order> orders)
    {
        _menuService = menuService;
        _orders = orders.ToList();
    }

    public IReadOnlyList<Order> Orders => _orders;

    // Создать пустой заказ
    public Order CreateOrder()
    {
        var order = new Order(NextOrderId());
        _orders.Add(order);
        return order;
    }

    // Создать заказ сразу с несколькими позициями: (Id блюда, количество).
    // Если хотя бы одна позиция некорректна, заказ не создаётся вообще.
    public Order CreateOrder(IEnumerable<(int MenuItemId, int Quantity)> positions)
    {
        var order = new Order(NextOrderId());

        foreach (var (menuItemId, quantity) in positions)
        {
            var menuItem = FindMenuItem(menuItemId);
            order.AddItem(menuItem, quantity);
        }

        if (order.Items.Count == 0)
            throw new InvalidOperationException("Нельзя создать заказ без позиций.");

        _orders.Add(order);
        return order;
    }

    public void AddItemToOrder(int orderId, int menuItemId, int quantity)
    {
        var order = FindOrder(orderId);
        var menuItem = FindMenuItem(menuItemId);
        order.AddItem(menuItem, quantity);
    }

    public void RemoveItemFromOrder(int orderId, int menuItemId)
    {
        var order = FindOrder(orderId);
        order.RemoveItem(menuItemId);
    }

    public void ChangeStatus(int orderId, OrderStatus newStatus)
    {
        var order = FindOrder(orderId);
        order.ChangeStatus(newStatus);
    }

    public decimal CalculateOrderTotal(int orderId) => FindOrder(orderId).CalculateTotal();

    public Order? GetOrderById(int id) => _orders.FirstOrDefault(o => o.Id == id);

    // Заказы с нужным статусом
    public IEnumerable<Order> GetOrdersByStatus(OrderStatus status) =>
        _orders.Where(o => o.Status == status);

    // Заказы, которые ещё не завершены (не оплачены и не отменены)
    public IEnumerable<Order> GetActiveOrders() =>
        _orders.Where(o => o.Status is OrderStatus.New or OrderStatus.InProgress or OrderStatus.Ready);

            // ---------- Поиск, фильтрация, сортировка ----------

    public IEnumerable<Order> SearchOrders(
        OrderStatus? status = null,
        DateTime? from = null,
        DateTime? to = null,
        OrderSortOrder sort = OrderSortOrder.ByDateNewestFirst)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new ArgumentException("Начало периода не может быть позже его конца.");

        IEnumerable<Order> result = _orders;

        if (status.HasValue)
            result = result.Where(o => o.Status == status.Value);

        if (from.HasValue)
            result = result.Where(o => o.CreatedAt >= from.Value.Date);

        if (to.HasValue)
            result = result.Where(o => o.CreatedAt < to.Value.Date.AddDays(1));

        return sort switch
        {
            OrderSortOrder.ByDateOldestFirst => result.OrderBy(o => o.CreatedAt),
            OrderSortOrder.ByTotalDescending => result.OrderByDescending(o => o.CalculateTotal()),
            _ => result.OrderByDescending(o => o.CreatedAt)
        };
    }

    // ---------- Вспомогательные методы ----------

    private int NextOrderId() => IdGenerator.Next(_orders);

    private Order FindOrder(int id) =>
        GetOrderById(id) ?? throw new KeyNotFoundException($"Заказ с Id {id} не найден.");

    private MenuItem FindMenuItem(int id) =>
        _menuService.GetItemById(id) ?? throw new KeyNotFoundException($"Блюдо с Id {id} не найдено в меню.");
}