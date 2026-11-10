using System;                                          // Console, Stopwatch
using System.Collections.Generic;                      // List
using System.Diagnostics;                              // Stopwatch
using System.IO;                                       // Path
using System.Linq;                                     // LINQ для демо
using System.Text;                                     // Encoding
using Core.Abstractions;                               // ICatalogStore
using Core.Domain;                                     // Product
using Core.Events;                                     // Події
using Core.Reports;                                    // ДОДАТКОВЕ 1, 2, 3: ReportService
using Cli.Notifications;                               // Sinks
using Core.Services;                                   // CatalogService
using Core.Storage;                                    // StoreFactory

Console.OutputEncoding = Encoding.UTF8;                // UTF-8

// 1. Ініціалізація сховища та сервісу
ICatalogStore store = StoreFactory.Create(args);       
var fileSink = new FileSink(Path.Combine(Directory.GetCurrentDirectory(), "logs", "sink.log")); 
bool useSink = args.Contains("--sink");
var service = new CatalogService(store, useSink ? fileSink : null);

// 2. Визначаємо підкоманду (пропускаємо прапорці типу --file)
var command = args.FirstOrDefault(a => !a.StartsWith("--")) ?? "report-top";

// 3. Отримуємо дані для звітів
var items = service.All();

// 4. Демонстрація звітів через switch (як вимагає методичка)
switch (command)
{
    case "report-top":
        Print(ReportService.TopByQuantity(items, 5));
        break;
    case "report-low":
        Print(ReportService.LowStock(items, 10));
        break;
    case "report-groups":
        foreach (var g in ReportService.ByUnit(items))
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;
    case "report-sum":
        var t = ReportService.Totals(items);
        Console.WriteLine($" позицій {t.Items}, всього {t.Total}, середнє {t.Average:F2}, макс {t.Max}");
        break;
        
    // ДОДАТКОВІ ЗАВДАННЯ
    case "report-join":
        // Створюємо фейкові переміщення для демонстрації Join
        var movements = new List<MovementDto> 
        {
            new("P-001", 50),
            new("P-004", -10),
            new("P-011", 8)
        };
        foreach (var line in ReportService.ProductsWithMovements(items, movements))
            Console.WriteLine($"  {line}");
        break;
        
    case "report-query-syntax":
        foreach (var g in ReportService.ByUnitQuerySyntax(items))
            Console.WriteLine($"  {g.Key,-6} позицій: {g.Count,3} сумарно: {g.TotalQuantity,6}");
        break;

    case "demo-deferred":
        Console.WriteLine("=== Демо відкладеного виконання ===");
        var source = items.ToList(); // беремо знімок
        var query = source.Where(p => p.Quantity < 10); // будуємо запит (нічого не виконується)
        
        Console.WriteLine($"Дефіцитних до змін: {query.Count()}"); // перше виконання
        
        // Додаємо новий товар у джерело
        source.Add(Product.Create("tmp00001", "SKU-999", "Скоба", "шт", 3)); 
        
        Console.WriteLine($"Дефіцитних після змін: {query.Count()}"); // друге виконання (бачить зміни!)
        
        var snapshot = query.ToList(); // фіксуємо результат
        source.Clear(); // очищуємо джерело
        
        Console.WriteLine($"У знімку лишилось: {snapshot.Count}"); // не змінюється (бо ToList)
        break;

    case "demo-stopwatch":
        Console.WriteLine("=== Додаткове 4: Порівняння швидкості ===");
        var largeData = Enumerable.Range(1, 100_000)
            .Select(i => Product.Create($"ID{i}", $"SKU-{i}", $"Товар {i}", "шт", i % 100))
            .ToList();

        var sw = Stopwatch.StartNew();
        var result1 = largeData.Where(p => p.Quantity < 10).ToList(); // з ToList (швидко)
        sw.Stop();
        Console.WriteLine($"З ToList (матеріалізація): {sw.ElapsedMilliseconds} мс, знайдено {result1.Count}");

        sw.Restart();
        var query2 = largeData.Where(p => p.Quantity < 10); // без ToList
        int count = 0;
        foreach (var p in query2) count++; // ітерація вручну
        sw.Stop();
        Console.WriteLine($"Без ToList (foreach): {sw.ElapsedMilliseconds} мс, знайдено {count}");
        break;

    default:
        Console.WriteLine("Доступні команди:");
        Console.WriteLine("  report-top | report-low | report-groups | report-sum");
        Console.WriteLine("  report-join | report-query-syntax | demo-deferred | demo-stopwatch");
        break;
}

return 0;

// Локальна функція для виводу рядків звіту
static void Print(IReadOnlyList<StockLine> lines)
{                                                      
    foreach (var l in lines)
        Console.WriteLine($" {l.Sku,-10} {l.Name,-22} {l.Quantity,6} {l.Unit}");
}
