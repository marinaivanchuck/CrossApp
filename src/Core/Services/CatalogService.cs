using System;                                          // Потрібно для EventHandler, Func, ArgumentNullException, InvalidOperationException, Guid
using System.Collections.Generic;                      // Потрібно для IReadOnlyList
using System.Linq;                                     // Потрібно для методів LINQ (Where, ToList)
using Core.Abstractions;                               // ICatalogStore, INotificationSink (сховище та інтерфейс сповіщувача)
using Core.Domain;                                     // Product (наша доменна сутність)
using Core.Events;                                     // CatalogChangedEventArgs, CatalogChangeKind (наші події)

namespace Core.Services;                               // Простір імен для сервісів

// Використовуємо "первинний конструктор" (primary constructor)
// Він приймає сховище (обов'язково) і сповіщувач (опційно, для додаткового завдання)
public sealed class CatalogService(ICatalogStore store, INotificationSink? sink = null)
{                                                      
    // Зберігаємо сховище у приватне поле. Оператор ?? throw захищає від передачі null
    private readonly ICatalogStore _store = store      
        ?? throw new ArgumentNullException(nameof(store));
    
    // ДОДАТКОВЕ 1: Зберігаємо DI-сповіщувач (може бути null, тому стоїть знак ?)
    private readonly INotificationSink? _sink = sink;  

    // ОГОЛОШЕННЯ ПОДІЇ. event обмежує доступ ззовні (можна тільки += та -=).
    // EventHandler<CatalogChangedEventArgs> - стандартний делегат .NET.
    // Знак ? означає, що підписників може не бути (Changed == null).
    public event EventHandler<CatalogChangedEventArgs>? Changed; 

    // Приватний метод для підняття (виклику) події
    private void OnChanged(Product p, CatalogChangeKind kind, int delta) 
    {                                                  
        // Формуємо об'єкт із даними про зміну (ID, тип зміни, дельта, новий залишок)
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity); 
        
        // Викликаємо подію. Оператор ?. захищає від NullReferenceException, якщо ніхто не підписався.
        // this передається як sender. 
        Changed?.Invoke(this, args);                   
        
        // ДОДАТКОВЕ 1: Якщо в конструктор передали sink, він також отримує сповіщення (через DI)
        _sink?.Notify($"{kind,-8} {p.Id} {delta:+0;-0} → залишок {p.Quantity}");
    }                                                  

    public Product Add(string sku, string name, string unit, int quantity) 
    {                                                  
        // Генеруємо випадковий ID. ToString("N") дає 32 символи без дефісів. [..8] бере перші 8 символів.
        string id = Guid.NewGuid().ToString("N")[..8]; 
        
        // Створюємо сутність через фабрику (там перевіряються всі інваріанти)
        var product = Product.Create(id, sku, name, unit, quantity); 
        
        _store.Add(product);                           // Зберігаємо у сховище
        
        // ПІДНІМАЄМО ПОДІЮ лише ПІСЛЯ успішного збереження
        OnChanged(product, CatalogChangeKind.Added, quantity); 
        return product;                                
    }                                                  

    public void Activate(string id)                    
    {                                                  
        // Шукаємо товар. Якщо не знайшли - кидаємо InvalidOperationException
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.Activate();                            // Змінюємо стан сутності (Draft -> Active)
        _store.Update(product);                        // Оновлюємо у сховищі
        // Тут НЕМАЄ OnChanged, бо активація не змінює кількість товару (це не подія складу)
    }                                                  

    public void Receive(string id, int quantity)       
    {                                                  
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        
        // Доменний метод сутності. Якщо quantity <= 0, він кине виняток.
        product.RegisterArrival(quantity);             
        _store.Update(product);                        // Оновлюємо сховище
        
        // Піднімаємо подію (тип Received, дельта додатна)
        OnChanged(product, CatalogChangeKind.Received, quantity); 
    }                                                  

    public void Issue(string id, int quantity)         
    {                                                  
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        
        // Доменний метод. Якщо не вистачає залишку, кине виняток.
        product.Issue(quantity);                       
        _store.Update(product);                        // Оновлюємо сховище
        
        // Піднімаємо подію (тип Issued, дельта ВІД'ЄМНА, тому ставимо мінус)
        OnChanged(product, CatalogChangeKind.Issued, -quantity); 
    }                                                  

    // Повертає всі товари зі сховища
    public IReadOnlyList<Product> All() => _store.List(); 
    
    // Шукає один товар за ID
    public Product? Find(string id) => _store.GetById(id); 
    
    // ДОДАТКОВЕ 2: Використання делегата Func для пошуку за довільною умовою
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) 
    {                                                  
        // LINQ Where приймає наш делегат predicate і фільтрує колекцію
        return _store.List().Where(predicate).ToList(); 
    }                                                  
}