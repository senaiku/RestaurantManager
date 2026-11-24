using System.Text;
using RestaurantManager.Services;
using RestaurantManager.Storage;
using RestaurantManager.UI;

Console.OutputEncoding = Encoding.UTF8;
Console.InputEncoding = Encoding.UTF8;

// По умолчанию данные хранятся в файле restaurant_data.json.
// --memory: хранилище в памяти, данные не сохраняются.
// --demo: хранилище в памяти, заполненное готовыми демонстрационными данными.
IRepository repository;
if (args.Contains("--demo"))
{
    var demoRepository = new InMemoryRepository();
    demoRepository.Save(DemoData.Create());
    repository = demoRepository;
    Console.WriteLine("Демо-режим: данные тестовые и после выхода не сохраняются.");
}
else if (args.Contains("--memory"))
{
    repository = new InMemoryRepository();
}
else
{
    repository = new JsonRepository();
}

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