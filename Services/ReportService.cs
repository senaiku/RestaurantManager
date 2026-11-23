using System.Text;
using RestaurantManager.Models;

namespace RestaurantManager.Services;

public class ReportService
{
    private readonly OrderService _orderService;
    private readonly MenuService _menuService;

    public ReportService(OrderService orderService, MenuService menuService)
    {
        _orderService = orderService;
        _menuService = menuService;
    }

    // from и to включительно; если не указаны, берём всё время
    public SalesReport GenerateSalesReport(DateTime? from = null, DateTime? to = null)
    {
        if (from.HasValue && to.HasValue && from.Value.Date > to.Value.Date)
            throw new ArgumentException("Начало периода не может быть позже его конца.");

        var ordersInPeriod = _orderService.Orders
            .Where(o => (!from.HasValue || o.CreatedAt >= from.Value.Date) &&
                        (!to.HasValue || o.CreatedAt < to.Value.Date.AddDays(1)))
            .ToList();

        var paid = ordersInPeriod.Where(o => o.Status == OrderStatus.Paid).ToList();
        var cancelled = ordersInPeriod.Count(o => o.Status == OrderStatus.Cancelled);

        var allPositions = paid.SelectMany(o => o.Items).ToList();

        // Продажи по блюдам: от самых популярных к менее популярным
        var dishes = allPositions
            .GroupBy(i => i.MenuItemId)
            .Select(g => new DishSales(g.First().Name, g.Sum(i => i.Quantity), g.Sum(i => i.Total)))
            .OrderByDescending(d => d.Quantity)
            .ThenBy(d => d.Name)
            .ToList();

        // Продажи по категориям: от самой прибыльной
        var categories = allPositions
            .GroupBy(i => GetCategoryName(i.MenuItemId))
            .Select(g => new CategorySales(g.Key, g.Sum(i => i.Quantity), g.Sum(i => i.Total)))
            .OrderByDescending(c => c.Revenue)
            .ThenBy(c => c.CategoryName)
            .ToList();

        return new SalesReport
        {
            From = from,
            To = to,
            OrdersCount = paid.Count,
            CancelledCount = cancelled,
            DishesCount = allPositions.Sum(i => i.Quantity),
            TotalRevenue = paid.Sum(o => o.CalculateTotal()),
            Dishes = dishes,
            Categories = categories
        };
    }

    // Текст отчёта для вывода в консоль
    public string Format(SalesReport report)
    {
        var sb = new StringBuilder();

        sb.AppendLine("=== Отчёт о продажах ===");
        sb.AppendLine($"Период: {FormatPeriod(report)}");
        sb.AppendLine($"Оплаченных заказов: {report.OrdersCount}");
        sb.AppendLine($"Отменённых заказов: {report.CancelledCount}");
        sb.AppendLine($"Продано блюд: {report.DishesCount}");
        sb.AppendLine($"Выручка: {report.TotalRevenue:F2} руб.");
        sb.AppendLine($"Средний чек: {report.AverageCheck:F2} руб.");

        if (report.OrdersCount == 0)
        {
            sb.AppendLine("Продаж за этот период нет.");
            return sb.ToString();
        }

        sb.AppendLine();
        sb.AppendLine("Продажи по блюдам:");
        foreach (var d in report.Dishes)
            sb.AppendLine($"  {d.Name} — {d.Quantity} шт., {d.Revenue:F2} руб.");

        sb.AppendLine();
        sb.AppendLine("Продажи по категориям:");
        foreach (var c in report.Categories)
            sb.AppendLine($"  {c.CategoryName} — {c.Quantity} шт., {c.Revenue:F2} руб.");

        return sb.ToString();
    }

    // ---------- Вспомогательные методы ----------

    // Если блюдо потом удалили из меню, показываем «Без категории»
    private string GetCategoryName(int menuItemId)
    {
        var item = _menuService.GetItemById(menuItemId);
        var category = item == null ? null : _menuService.GetCategoryById(item.CategoryId);
        return category?.Name ?? "Без категории";
    }

    private static string FormatPeriod(SalesReport report)
    {
        if (!report.From.HasValue && !report.To.HasValue)
            return "за всё время";

        var from = report.From.HasValue ? report.From.Value.ToString("dd.MM.yyyy") : "начало";
        var to = report.To.HasValue ? report.To.Value.ToString("dd.MM.yyyy") : "сегодня";
        return $"{from} — {to}";
    }
}