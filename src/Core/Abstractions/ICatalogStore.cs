using Core.Domain;                                     // Product

namespace Core.Abstractions;                           // простір імен для контрактів

public interface ICatalogStore                         // контракт сховища (без реалізації)
{                                                      // початок інтерфейсу
    IReadOnlyList<Product> List();                     // отримати всі записи (лише для читання)
    Product? GetById(string id);                       // знайти за id (null якщо немає)
    void Add(Product item);                            // додати новий запис
    void Update(Product item);                         // оновити існуючий запис
    bool Remove(string id);                            // видалити за id (true якщо видалено)

    // ДОДАТКОВЕ 2: пошук за довільною умовою
    IReadOnlyList<Product> Search(Func<Product, bool> predicate);  // повертає товари, що задовольняють умову
}                                                      // кінець інтерфейсу
