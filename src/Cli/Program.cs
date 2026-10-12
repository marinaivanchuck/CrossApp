using System.Text;                                     // Encoding.UTF8
using Core.Domain;                                     // Product, Warehouse
using Core.Import;                                     // ProductImportService

Console.OutputEncoding = Encoding.UTF8;                // кирилиця в консолі

// ========== Сценарій 1: успіх ==========
Console.WriteLine("=== Сценарій 1: успіх ===");        // заголовок
Product product = Product.Create("P-001", "sku-001", "Цемент М400 25кг", "шт", 100);  // створення
Console.WriteLine(product);                            // початковий стан (100 шт, Draft)

product.Activate();                                    // Draft → Active (додаткове 3)
Console.WriteLine(product);                            // тепер Active

product.RegisterArrival(50);                           // +50 → 150
Console.WriteLine(product);                            // після приходу

product.Issue(30);                                     // -30 → 120
Console.WriteLine(product);                            // після видачі

// ========== Сценарій 2: порушення інваріантів ==========
Console.WriteLine();                                   // відступ
Console.WriteLine("=== Сценарій 2: порушення інваріантів ===");  // заголовок

TryDo("видача більша за залишок", () => product.Issue(1000));                    // видача 1000
TryDo("порожній SKU", () => Product.Create("P-002", " ", "Пісок", "т", 10));     // порожній SKU
TryDo("від'ємний залишок", () => Product.Create("P-003", "SKU-003", "Цегла", "шт", -5));  // -5
TryDo("повторна активація", () => product.Activate());                            // вже Active
TryDo("видача з архіву", () => { product.Archive(); product.Issue(10); });        // архів → видача

// ========== Сценарій 3: інваріант на ДВІ сутності (додаткове 2) ==========
Console.WriteLine();                                   // відступ
Console.WriteLine("=== Сценарій 3: інваріант на дві сутності ===");  // заголовок

Warehouse wh = Warehouse.Create("W-001", "Головний склад");  // створюємо склад
wh.AddProduct(product);                                // додаємо товар — OK
Console.WriteLine(wh);                                 // виводимо

TryDo("дублікат SKU на складі", () => wh.AddProduct(   // спроба додати дублікат
    Product.Create("P-100", "SKU-001", "Дубль", "шт", 5)));  // той самий SKU

// ========== Сценарій 4: імпорт → сутності (додаткове 1) ==========
Console.WriteLine();                                   // відступ
Console.WriteLine("=== Сценарій 4: імпорт у сутності ===");  // заголовок

var outcome = ProductImportService.Load("data/sample.csv");  // завантажуємо
Console.WriteLine($"Створено сутностей: {outcome.Products.Count}");  // кількість успішних
foreach (Product p in outcome.Products.Take(3))        // перші 3
    Console.WriteLine($"  {p}");                       // виводимо

Console.WriteLine($"Не пройшли інваріанти / парсинг: {outcome.Errors.Count}");  // кількість помилок
foreach (string e in outcome.Errors)                   // усі помилки
    Console.WriteLine($"  ! {e}");                     // виводимо

return 0;                                              // код виходу 0

static void TryDo(string title, Action action)         // локальна функція try/catch
{                                                      // початок TryDo
    try                                                // блок із винятком
    {                                                  // початок try
        action();                                      // виклик дії
        Console.WriteLine($"  {title}: виняток НЕ спрацював!");  // якщо винятку НЕ було
    }                                                  // кінець try
    catch (Exception ex)                               // ловимо будь-який виняток
    {                                                  // початок catch
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");  // тип + текст
    }                                                  // кінець catch
}                                                      // кінець TryDo
