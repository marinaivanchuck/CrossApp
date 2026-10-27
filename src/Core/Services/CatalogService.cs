using Core.Abstractions;                               // ICatalogStore
using Core.Domain;                                     // Product

namespace Core.Services;                               // простір імен сервісів

public sealed class CatalogService(ICatalogStore store)  // залежність — інтерфейс
{                                                      // початок класу
    private readonly ICatalogStore _store = store      // зберігаємо залежність
        ?? throw new ArgumentNullException(nameof(store));  // захист від null

    public Product Add(string sku, string name, string unit, int quantity)  // створення товару
    {                                                  // початок Add
        string id = Guid.NewGuid().ToString("N")[..8]; // генеруємо короткий Id
        var product = Product.Create(id, sku, name, unit, quantity);  // фабрика сутності
        _store.Add(product);                           // додаємо в сховище
        return product;                                // повертаємо створений об'єкт
    }                                                  // кінець Add

    public void Activate(string id)                    // активувати товар
    {                                                  // початок Activate
        var product = _store.GetById(id)               // шукаємо
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");  // якщо немає — виняток

        product.Activate();                            // Draft → Active (метод з лаби 4)
        _store.Update(product);                        // оновлюємо сховище
    }                                                  // кінець Activate

    public void Receive(string id, int quantity)       // прихід товару
    {                                                  // початок Receive
        var product = _store.GetById(id)               // шукаємо товар
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");  // якщо немає — виняток

        product.RegisterArrival(quantity);             // викликаємо доменний метод
        _store.Update(product);                        // зберігаємо оновлений стан
    }                                                  // кінець Receive

    public void Issue(string id, int quantity)         // видача товару
    {                                                  // початок Issue
        var product = _store.GetById(id)               // шукаємо
            ?? throw new InvalidOperationException($"Немає запису з id={id}.");  // виняток

        product.Issue(quantity);                       // доменний метод (інваріанти)
        _store.Update(product);                        // оновлюємо сховище
    }                                                  // кінець Issue

    public IReadOnlyList<Product> All() => _store.List();  // усі товари

    public Product? Find(string id) => _store.GetById(id);  // пошук за Id

    public IReadOnlyList<Product> Search(Func<Product, bool> predicate) =>
        _store.Search(predicate);                      // пошук за умовою
}
