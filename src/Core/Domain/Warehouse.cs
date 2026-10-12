namespace Core.Domain;                                 // простір імен домену

public sealed class Warehouse                          // сутність «Склад»
{                                                      // початок класу
    private readonly List<Product> _products = [];     // приватна колекція (назовні — лише read-only)

    public string Id { get; }                          // Id — лише для читання
    public string Name { get; }                        // назва — лише для читання
    public IReadOnlyList<Product> Products => _products.AsReadOnly();  // read-only поверхня

    private Warehouse(string id, string name)          // приватний конструктор
    {                                                  // початок конструктора
        Id = id;                                       // запис Id
        Name = name;                                   // запис Name
    }                                                  // кінець конструктора

    public static Warehouse Create(string id, string name)  // фабрика
    {                                                  // початок Create
        if (string.IsNullOrWhiteSpace(id))             // перевірка Id
            throw new ArgumentException("Id складу обов'язковий", nameof(id));  // виняток

        if (string.IsNullOrWhiteSpace(name))           // перевірка назви
            throw new ArgumentException("Назва складу обов'язкова", nameof(name));  // виняток

        return new Warehouse(id.Trim(), name.Trim());  // створення
    }                                                  // кінець Create

    public void AddProduct(Product product)            // додати товар на склад
    {                                                  // початок AddProduct
        if (product is null)                           // перевірка на null
            throw new ArgumentNullException(nameof(product));  // виняток

        // ІНВАРІАНТ, ЩО ОХОПЛЮЄ ДВІ СУТНОСТІ: на одному складі SKU унікальний
        if (_products.Any(p => p.Sku == product.Sku))  // перевірка унікальності SKU
            throw new InvalidOperationException(       // виняток
                $"Товар зі SKU {product.Sku} вже є на складі {Name}");  // повідомлення

        _products.Add(product);                        // додаємо товар
    }                                                  // кінець AddProduct

    public override string ToString() =>               // гарний вивід
        $"Склад {Id} [{Name}]: товарів — {_products.Count}";  // кількість товарів
}                                                      // кінець класу
