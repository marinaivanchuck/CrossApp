using Core.Abstractions;                               // ICatalogStore
using Core.Domain;                                     // Product

namespace Core.Storage;                                // простір імен сховищ

public sealed class CachingCatalogStore(ICatalogStore inner) : ICatalogStore
{                                                      // клас-обгортка (декоратор)
    private readonly ICatalogStore _inner = inner;     // вкладене сховище (патерн Декоратор)

    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);  // локальний кеш

    public IReadOnlyList<Product> List() => _inner.List();  // просто делегуємо

    public Product? GetById(string id)                 // розумне кешування GetById
    {                                                  // початок GetById
        if (_cache.TryGetValue(id, out var cached))    // якщо є в кеші
            return cached;                             // повертаємо

        var product = _inner.GetById(id);              // інакше — з основного сховища
        if (product != null)                           // якщо знайшли
            _cache[id] = product;                      // кешуємо
        return product;                                // повертаємо
    }                                                  // кінець GetById

    public void Add(Product item)                      // додати
    {                                                  // початок Add
        _inner.Add(item);                              // у внутрішнє сховище
        _cache[item.Id] = item;                        // оновити кеш
    }                                                  // кінець Add

    public void Update(Product item)                   // оновити
    {                                                  // початок Update
        _inner.Update(item);                           // у внутрішнє сховище
        _cache[item.Id] = item;                        // оновити кеш
    }                                                  // кінець Update

    public bool Remove(string id)                      // видалити
    {                                                  // початок Remove
        bool removed = _inner.Remove(id);              // спробувати видалити
        if (removed) _cache.Remove(id);                // почистити кеш
        return removed;                                // повернути результат
    }                                                  // кінець Remove

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        _inner.Search(predicate);                      // делегуємо
}
