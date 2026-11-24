using System.Text;
using RestaurantManager.Services;
using RestaurantManager.Storage;
using RestaurantManager.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// По умолчанию данные хранятся в файле restaurant_data.json.
// Запуск с ключом --memory использует хранилище в памяти (данные не сохраняются).
IRepository repository = args.Contains("--memory")
    ? new InMemoryRepository()
    : new JsonRepository();

AppData data;
try
{
    data = repository.Load();
}
catch (InvalidDataException ex)
{
    // Не продолжаем работу, чтобы не перезаписать повреждённый файл пустыми данными
    Console.WriteLine(ex.Message);
    Console.WriteLine("Исправьте или удалите файл данных и запустите программу снова.");
    return;
}

var menuService = new MenuService(data.Categories, data.MenuItems);
var orderService = new OrderService(menuService, data.Orders);
var reportService = new ReportService(orderService, menuService);

new ConsoleApp(menuService, orderService, reportService, repository).Run();