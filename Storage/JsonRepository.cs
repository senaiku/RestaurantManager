using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RestaurantManager.Storage;

public class JsonRepository : IRepository
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,                                   // читаемый файл
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,  // русские буквы без \u-кодов
        Converters = { new JsonStringEnumConverter() }          // статусы текстом, а не числами
    };

    private readonly string _filePath;

    public JsonRepository(string filePath = "restaurant_data.json")
    {
        _filePath = filePath;
    }

    public AppData Load()
    {
        // Первый запуск: файла ещё нет, начинаем с пустых данных
        if (!File.Exists(_filePath))
            return new AppData();

        try
        {
            var json = File.ReadAllText(_filePath);
            return JsonSerializer.Deserialize<AppData>(json, Options) ?? new AppData();
        }
        catch (Exception ex) when (ex is JsonException or ArgumentException)
        {
            throw new InvalidDataException(
                $"Файл данных «{_filePath}» повреждён: {ex.Message}", ex);
        }
    }

    public void Save(AppData data)
    {
        var directory = Path.GetDirectoryName(Path.GetFullPath(_filePath));
        if (!string.IsNullOrEmpty(directory))
            Directory.CreateDirectory(directory);

        // Сначала пишем во временный файл, потом заменяем основной:
        // если программа упадёт посреди записи, старые данные не пострадают
        var json = JsonSerializer.Serialize(data, Options);
        var tempPath = _filePath + ".tmp";
        File.WriteAllText(tempPath, json);
        File.Move(tempPath, _filePath, overwrite: true);
    }
}