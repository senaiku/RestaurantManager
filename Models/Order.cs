using System.Text.Json.Serialization;

namespace RestaurantManager.Models;

public class Order
{
    public int Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Статус можно менять только через ChangeStatus (так работает проверка переходов)
    [JsonInclude]
    public OrderStatus Status { get; private set; } = OrderStatus.New;

    public List<OrderItem> Items { get; set; } = new();

    // Нужен для загрузки из JSON
    public Order() { }

    public Order(int id)
    {
        Id = id;
    }

    // Добавить блюдо; если такое уже есть в заказе, увеличиваем количество
    public void AddItem(MenuItem menuItem, int quantity)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Изменять состав можно только у нового заказа.");

        var existing = Items.FirstOrDefault(i => i.MenuItemId == menuItem.Id);
        if (existing != null)
        {
            existing.Quantity += quantity; // проверка > 0 сработает в свойстве
            return;
        }

        Items.Add(new OrderItem(menuItem, quantity));
    }

    public void RemoveItem(int menuItemId)
    {
        if (Status != OrderStatus.New)
            throw new InvalidOperationException("Изменять состав можно только у нового заказа.");

        var item = Items.FirstOrDefault(i => i.MenuItemId == menuItemId)
                   ?? throw new KeyNotFoundException("Такого блюда нет в заказе.");
        Items.Remove(item);
    }

    // Общая стоимость заказа
    public decimal CalculateTotal() => Items.Sum(i => i.Total);

    // Общее количество блюд в заказе
    public int TotalQuantity() => Items.Sum(i => i.Quantity);

    public void ChangeStatus(OrderStatus newStatus)
    {
        if (!CanChangeStatusTo(newStatus))
            throw new InvalidOperationException(
                $"Нельзя перевести заказ из статуса «{Status}» в «{newStatus}».");

        if (newStatus == OrderStatus.InProgress && Items.Count == 0)
            throw new InvalidOperationException("Нельзя отправить на кухню пустой заказ.");

        Status = newStatus;
    }

    public bool CanChangeStatusTo(OrderStatus newStatus) => (Status, newStatus) switch
    {
        (OrderStatus.New, OrderStatus.InProgress) => true,
        (OrderStatus.InProgress, OrderStatus.Ready) => true,
        (OrderStatus.Ready, OrderStatus.Paid) => true,
        (OrderStatus.New or OrderStatus.InProgress or OrderStatus.Ready, OrderStatus.Cancelled) => true,
        _ => false
    };

    public override string ToString() =>
        $"Заказ №{Id} от {CreatedAt:dd.MM.yyyy HH:mm} [{Status}] — {CalculateTotal():F2} руб.";
}