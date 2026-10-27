using Core.Abstractions;                               // ICatalogStore
using Core.Domain;                                     // Product

namespace Core.Storage;                                // простір імен для сховищ

public sealed class InMemoryCatalogStore(IEnumerable<Product>? seed = null) : ICatalogStore
{                                                      // клас реалізує контракт
    private readonly Dictionary<string, Product> _items =   // приватний словник (Id → Product)
        (seed ?? []).ToDictionary(p => p.Id, StringComparer.OrdinalIgnoreCase);  // наповнюємо або порожній

    public IReadOnlyList<Product> List() => _items.Values.ToList();  // всі значення словника

    public Product? GetById(string id) => _items.GetValueOrDefault(id);  // null якщо немає (без винятку)

    public void Add(Product item)                      // додавання
    {                                                  // початок Add
        ArgumentNullException.ThrowIfNull(item);       // якщо null — виняток
        if (_items.ContainsKey(item.Id))               // перевірка унікальності Id
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");  // виняток

        _items.Add(item.Id, item);                     // додаємо
    }                                                  // кінець Add

    public void Update(Product item) => _items[item.Id] = item;  // просто перезаписуємо за ключем

    public bool Remove(string id) => _items.Remove(id);          // видаляємо (true якщо був)

    // ДОДАТКОВЕ 2: пошук через LINQ
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        _items.Values.Where(predicate).ToList();       // фільтруємо за умовою
}
