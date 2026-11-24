namespace RestaurantManager.Models;

// Базовый класс для всех объектов, которые хранятся в системе и имеют идентификатор
public abstract class Entity
{
    public int Id { get; set; }
}

// Генерация следующего свободного Id для любого списка сущностей
public static class IdGenerator
{
    public static int Next<T>(IEnumerable<T> items) where T : Entity =>
        items.Any() ? items.Max(i => i.Id) + 1 : 1;
}