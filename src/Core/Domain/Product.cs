using Core.Dto;                                        // ProductDto для мапінгу

namespace Core.Domain;                                 // простір імен домену

public enum ProductStatus                              // стан товару (додаткове 3)
{                                                      // початок enum
    Draft,                                             // чернетка — тільки створено
    Active,                                            // активний — можна видавати
    Archived                                           // заархівовано — заборонено видавати
}                                                      // кінець enum

public sealed class Product                            // sealed — немає спадкування
{                                                      // початок класу
    private int _quantity;                             // приватне поле: залишок

    public string Id { get; }                          // Id — лише для читання
    public string Sku { get; }                         // SKU — лише для читання
    public string Name { get; }                        // назва — лише для читання
    public string Unit { get; }                        // одиниця — лише для читання
    public int Quantity => _quantity;                  // властивість-вираз: показ приватного поля
    public ProductStatus Status { get; private set; }  // статус: читати всім, змінювати лише методи

    private Product(string id, string sku, string name, string unit, int quantity)  // приватний конструктор
    {                                                  // початок конструктора
        Id = id;                                       // запис Id
        Sku = sku;                                     // запис Sku
        Name = name;                                   // запис Name
        Unit = unit;                                   // запис Unit
        _quantity = quantity;                          // запис залишку
        Status = ProductStatus.Draft;                  // новий товар — завжди Draft
    }                                                  // кінець конструктора

    public static Product Create(string id, string sku, string name, string unit, int quantity)  // фабрика
    {                                                  // початок Create
        if (string.IsNullOrWhiteSpace(id))             // перевірка Id
            throw new ArgumentException("Ідентифікатор обов'язковий", nameof(id));  // виняток

        if (string.IsNullOrWhiteSpace(sku))            // перевірка SKU
            throw new ArgumentException("SKU не може бути порожнім", nameof(sku));  // виняток

        if (string.IsNullOrWhiteSpace(name))           // перевірка Name
            throw new ArgumentException("Назва не може бути порожньою", nameof(name));  // виняток

        if (quantity < 0)                              // перевірка залишку
            throw new ArgumentOutOfRangeException(nameof(quantity), quantity,  // виняток
                "Початковий залишок не може бути від'ємним");  // повідомлення

        return new Product(                            // створення
            id.Trim(),                                 // обрізаємо Id
            sku.Trim().ToUpperInvariant(),             // обрізаємо + верхній регістр SKU
            name.Trim(),                               // обрізаємо Name
            unit.Trim(),                               // обрізаємо Unit
            quantity);                                 // кількість без змін
    }                                                  // кінець Create

    public void Activate()                             // додаткове 3: Draft → Active
    {                                                  // початок Activate
        Status = Status switch                         // switch expression для переходу
        {                                              // початок switch
            ProductStatus.Draft => ProductStatus.Active,                  // дозволено
            _ => throw new InvalidOperationException(                     // заборонено
                $"Неможливо активувати товар у стані {Status}")           // повідомлення
        };                                             // кінець switch
    }                                                  // кінець Activate

    public void Archive()                              // додаткове 3: Active → Archived
    {                                                  // початок Archive
        Status = Status switch                         // switch expression
        {                                              // початок switch
            ProductStatus.Active => ProductStatus.Archived,               // дозволено
            _ => throw new InvalidOperationException(                     // заборонено
                $"Неможливо архівувати товар у стані {Status}")           // повідомлення
        };                                             // кінець switch
    }                                                  // кінець Archive

    public void RegisterArrival(int amount)            // прихід товару
    {                                                  // початок RegisterArrival
        if (Status == ProductStatus.Archived)          // перевірка статусу
            throw new InvalidOperationException("Архівований товар не приймається");  // виняток

        if (amount <= 0)                               // перевірка кількості
            throw new ArgumentOutOfRangeException(nameof(amount), amount,  // виняток
                "Кількість приходу має бути більшою за нуль");  // повідомлення

        _quantity += amount;                           // збільшуємо залишок
    }                                                  // кінець RegisterArrival

    public void Issue(int amount)                      // видача товару
    {                                                  // початок Issue
        if (Status != ProductStatus.Active)            // перевірка статусу
            throw new InvalidOperationException(       // виняток
                $"Видавати можна лише Active, а зараз {Status}");  // повідомлення

        if (amount <= 0)                               // перевірка кількості
            throw new ArgumentOutOfRangeException(nameof(amount), amount,  // виняток
                "Кількість видачі має бути більшою за нуль");  // повідомлення

        if (amount > _quantity)                        // перевірка: не більше залишку
            throw new InvalidOperationException(       // виняток стану
                $"Не можна видати {amount}: залишок {Sku} = {_quantity}");  // повідомлення

        _quantity -= amount;                           // зменшуємо залишок
    }                                                  // кінець Issue

    public ProductDto ToDto() => new(Id, Sku, Name, Unit, Quantity);  // мапінг у DTO

    public static Product FromDto(ProductDto dto) =>   // мапінг із DTO
        Create(dto.Id, dto.Sku, dto.Name, dto.Unit, dto.Quantity);  // через Create

    public override string ToString() =>               // красивий вивід
        $"{Id} [{Sku}] {Name} — {Quantity} {Unit} ({Status})";  // зі статусом
}                                                      // кінець класу
