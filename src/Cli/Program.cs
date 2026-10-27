using System.Text;                                     // Encoding.UTF8
using Core.Abstractions;                               // ICatalogStore
using Core.Storage;                                    // StoreFactory
using Core.Services;                                   // CatalogService
using Core.Domain;                                     // Product

Console.OutputEncoding = Encoding.UTF8;                // UTF-8

// ДОДАТКОВЕ 3: фабрика створює сховище (з декоратором) і повертає інтерфейс
ICatalogStore store = StoreFactory.Create(args);       // єдина точка вибору реалізації

var service = new CatalogService(store);               // впроваджуємо залежність через конструктор

Console.WriteLine($"Сховище: {store.GetType().Name}"); // виводимо тип сховища

// === Сценарій 1: успіх ===
Console.WriteLine("\n=== Сценарій 1: успіх ===");      // заголовок
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);  // створюємо товар
Console.WriteLine($"Створено: {created}");             // друкуємо (Draft)

service.Activate(created.Id);                          // <--- ДОДАНО! Draft → Active
Console.WriteLine($"Після активації: {service.Find(created.Id)}");

service.Receive(created.Id, 25);                       // прихід +25 → 75
Console.WriteLine($"Після приходу: {service.Find(created.Id)}");  // друкуємо

service.Issue(created.Id, 10);                         // видача -10 → 65
Console.WriteLine($"Після видачі: {service.Find(created.Id)}");   // друкуємо

// === Сценарій 2: помилки ===
Console.WriteLine("\n=== Сценарій 2: помилки ===");    // заголовок

TryDo("дубль Id", () => store.Add(created));           // дублікат Id
TryDo("Receive з неіснуючим Id", () => service.Receive("no-such", 10));  // немає Id
TryDo("видача більша за залишок", () => service.Issue(created.Id, 9999)); // більше залишку

// === Сценарій 3: список + пошук (ДОДАТКОВЕ 2) ===
Console.WriteLine("\n=== Сценарій 3: пошук ===");      // заголовок
Console.WriteLine($"Всього товарів: {service.All().Count}");  // рахуємо

var cheapItems = service.Search(p => p.Quantity < 50); // шукаємо з малою кількістю
Console.WriteLine($"Товарів з залишком < 50: {cheapItems.Count}");  // виводимо
foreach (var p in cheapItems.Take(3))                  // показуємо перші 3
    Console.WriteLine($"  {p}");                       // друкуємо

return 0;                                              // успіх

static void TryDo(string title, Action action)         // локальна функція try/catch
{                                                      // початок TryDo
    try                                                // блок спроби
    {                                                  // початок try
        action();                                      // виконуємо
        Console.WriteLine($"  {title}: виняток НЕ спрацював!");  // якщо не було винятку
    }                                                  // кінець try
    catch (Exception ex)                               // ловимо
    {                                                  // початок catch
        Console.WriteLine($"  {title}: {ex.GetType().Name} — {ex.Message}");  // виводимо
    }                                                  // кінець catch
}                                                      // кінець TryDo
