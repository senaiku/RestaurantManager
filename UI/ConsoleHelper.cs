using System.Globalization;

namespace RestaurantManager.UI;

// Вспомогательные методы безопасного ввода с консоли
public static class ConsoleHelper
{
    public static string ReadLine(string prompt)
    {
        Console.Write(prompt);
        var line = Console.ReadLine();

        // null приходит, если ввод закрыт (например, Ctrl+Z / Ctrl+D)
        if (line == null)
        {
            Console.WriteLine();
            Console.WriteLine("Ввод завершён. Выход из программы.");
            Environment.Exit(0);
        }

        return line.Trim();
    }

    // Непустая строка
    public static string ReadString(string prompt)
    {
        while (true)
        {
            var s = ReadLine(prompt);
            if (s.Length > 0)
                return s;
            Console.WriteLine("Значение не может быть пустым.");
        }
    }

    // Целое число в диапазоне [min; max]
    public static int ReadInt(string prompt, int min = int.MinValue, int max = int.MaxValue)
    {
        while (true)
        {
            var s = ReadLine(prompt);
            if (int.TryParse(s, NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
                && value >= min && value <= max)
                return value;

            var hint = min != int.MinValue && max != int.MaxValue ? $" от {min} до {max}"
                     : min != int.MinValue ? $" не меньше {min}"
                     : "";
            Console.WriteLine($"Некорректный ввод. Введите целое число{hint}.");
        }
    }

    // Число больше нуля (принимает и 99,50, и 99.50)
    public static decimal ReadDecimal(string prompt)
    {
        while (true)
        {
            var s = ReadLine(prompt);
            if (TryParseDecimal(s, out var value) && value > 0)
                return value;
            Console.WriteLine("Некорректный ввод. Введите число больше нуля (например, 450 или 99,50).");
        }
    }

    // То же, но пустой ввод (Enter) допустим и возвращает null
    public static decimal? ReadOptionalDecimal(string prompt)
    {
        while (true)
        {
            var s = ReadLine(prompt);
            if (s.Length == 0)
                return null;
            if (TryParseDecimal(s, out var value) && value > 0)
                return value;
            Console.WriteLine("Некорректный ввод. Введите число больше нуля или нажмите Enter.");
        }
    }

    // Дата в формате дд.мм.гггг; пустой ввод возвращает null
    public static DateTime? ReadOptionalDate(string prompt)
    {
        var formats = new[] { "dd.MM.yyyy", "d.M.yyyy" };

        while (true)
        {
            var s = ReadLine(prompt);
            if (s.Length == 0)
                return null;
            if (DateTime.TryParseExact(s, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out var date))
                return date;
            Console.WriteLine("Некорректная дата. Формат: дд.мм.гггг (например, 25.11.2026).");
        }
    }

    // Вопрос «да/нет»
    public static bool Confirm(string question)
    {
        while (true)
        {
            var s = ReadLine($"{question} (д/н): ").ToLowerInvariant();
            if (s is "д" or "да" or "y" or "yes")
                return true;
            if (s is "н" or "нет" or "n" or "no")
                return false;
            Console.WriteLine("Введите «д» или «н».");
        }
    }

    private static bool TryParseDecimal(string s, out decimal value) =>
        decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.CurrentCulture, out value) ||
        decimal.TryParse(s, NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out value);
}