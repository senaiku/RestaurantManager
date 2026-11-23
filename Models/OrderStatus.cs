namespace RestaurantManager.Models;

public enum OrderStatus
{
    New,        // Новый
    InProgress, // Готовится
    Ready,      // Готов
    Paid,       // Оплачен
    Cancelled   // Отменён
}