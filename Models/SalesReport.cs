namespace RestaurantManager.Models;

// Продажи по одному блюду
public record DishSales(string Name, int Quantity, decimal Revenue);

// Продажи по одной категории
public record CategorySales(string CategoryName, int Quantity, decimal Revenue);

public class SalesReport
{
    public DateTime? From { get; init; }
    public DateTime? To { get; init; }

    public int OrdersCount { get; init; }       // оплаченные заказы
    public int CancelledCount { get; init; }    // отменённые заказы
    public int DishesCount { get; init; }       // всего блюд продано
    public decimal TotalRevenue { get; init; }  // выручка

    public decimal AverageCheck => OrdersCount == 0 ? 0 : TotalRevenue / OrdersCount;

    public IReadOnlyList<DishSales> Dishes { get; init; } = new List<DishSales>();
    public IReadOnlyList<CategorySales> Categories { get; init; } = new List<CategorySales>();
}