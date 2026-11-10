using System;                                          // Func, Action, Tuple
using System.Collections.Generic;                      // IEnumerable, IReadOnlyList
using System.Linq;                                     // LINQ-методи
using Core.Domain;                                     // Product

namespace Core.Reports;                                // простір імен звітів

public static class ReportService                      // сервіс звітів
{                                                      // початок класу
    // 1) Фільтр + сортування + проєкція (LowStock)
    public static IReadOnlyList<StockLine> LowStock(IEnumerable<Product> source, int threshold)
    {                                                  // початок LowStock
        return source.Where(p => p.Quantity < threshold) // фільтруємо (відкладено)
            .OrderBy(p => p.Quantity)                  // сортуємо за кількістю
            .ThenBy(p => p.Sku, StringComparer.Ordinal) // стабільне сортування (другий ключ)
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)) // проєкція в DTO
            .ToList();                                 // матеріалізація (виконання)
    }                                                  // кінець LowStock

    // 2) Топ-N зі стабільним порядком
    public static IReadOnlyList<StockLine> TopByQuantity(IEnumerable<Product> source, int n)
    {                                                  // початок TopByQuantity
        return source.OrderByDescending(p => p.Quantity) // найбільші спочатку
            .ThenBy(p => p.Sku, StringComparer.Ordinal) // стабільність (другий ключ)
            .Take(n)                                   // беремо перші N
            .Select(p => new StockLine(p.Sku, p.Name, p.Quantity, p.Unit)) // у DTO
            .ToList();                                 // матеріалізація
    }                                                  // кінець TopByQuantity

    // 3) Групування з підрахунком і сумою
    public static IReadOnlyList<GroupSummary> ByUnit(IEnumerable<Product> source)
    {                                                  // початок ByUnit
        return source.GroupBy(p => p.Unit, StringComparer.OrdinalIgnoreCase) // групуємо
            .Select(g => new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity))) // проєкція
            .OrderByDescending(s => s.TotalQuantity)   // сортуємо групи за сумою
            .ToList();                                 // матеріалізація
    }                                                  // кінець ByUnit

    // 4) Агрегати з обробкою порожньої послідовності
    public static (int Items, int Total, double Average, int Max) Totals(IEnumerable<Product> source)
    {                                                  // початок Totals
        var list = source.ToList();                    // матеріалізуємо, щоб не читати двічі
        if (list.Count == 0)                           // якщо порожньо
            return (0, 0, 0d, 0);                      // безпечні нулі

        return (list.Count,                            // кількість
                list.Sum(p => p.Quantity),             // сума
                list.Average(p => p.Quantity),         // середнє (double)
                list.Max(p => p.Quantity));            // максимум
    }                                                  // кінець Totals

    // ДОДАТКОВЕ 1: Join двох колекцій
    public static IReadOnlyList<string> ProductsWithMovements(
        IEnumerable<Product> products, IEnumerable<MovementDto> movements)
    {                                                  // початок Join-методу
        // Метод синтаксису Join: з'єднуємо товари та переміщення за ID
        var query = products.Join(
            movements,                                 // внутрішня послідовність
            p => p.Id,                                 // ключ зовнішньої (Product)
            m => m.ProductId,                          // ключ внутрішньої (Movement)
            (p, m) => $"{p.Sku} {p.Name} → зміна: {m.Delta:+0;-0}"); // результат

        return query.ToList();                         // матеріалізація
    }                                                  // кінець Join-методу

    // ДОДАТКОВЕ 2: Власний метод-розширення TopBy
    public static IEnumerable<T> TopBy<T>(
        this IEnumerable<T> source, Func<T, int> selector, int n)
    {                                                  // початок TopBy
        return source.OrderByDescending(selector).Take(n); // сортуємо і беремо N
    }                                                  // кінець TopBy

    // ДОДАТКОВЕ 3: Синтаксис запитів (Query Syntax)
    public static IReadOnlyList<GroupSummary> ByUnitQuerySyntax(IEnumerable<Product> source)
    {                                                  // початок Query Syntax
        // Це той самий GroupBy, але записаний як SQL-подібний запит
        var query = from product in source             // беремо джерело
                    group product by product.Unit into g // групуємо за Unit
                    orderby g.Sum(p => p.Quantity) descending // сортуємо групи за сумою
                    select new GroupSummary(g.Key, g.Count(), g.Sum(p => p.Quantity)); // проєкція

        return query.ToList();                         // матеріалізація
    }                                                  // кінець Query Syntax
}                                                      // кінець класу
