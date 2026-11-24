using System.Text.Json;

namespace RestaurantManager.Storage;

// Хранилище в памяти: данные живут только пока работает программа.
// Полезно для тестов и демонстрации: файл на диске не создаётся и не меняется.
public class InMemoryRepository : IRepository
{
    private AppData _data = new();

    public AppData Load() => Copy(_data);

    public void Save(AppData data) => _data = Copy(data);

    // Копия через сериализацию, чтобы вызывающий код не менял сохранённые объекты напрямую
    private static AppData Copy(AppData data) =>
        JsonSerializer.Deserialize<AppData>(JsonSerializer.Serialize(data)) ?? new AppData();
}