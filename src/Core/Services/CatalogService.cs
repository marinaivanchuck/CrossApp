using System;                                          // Action, EventHandler, Func
using System.Collections.Generic;                      // IReadOnlyList
using System.Linq;                                     // Where, ToList
using Core.Abstractions;                               // ICatalogStore, INotificationSink
using Core.Domain;                                     // Product
using Core.Events;                                     // Події

namespace Core.Services;                               // простір імен сервісів

public sealed class CatalogService(ICatalogStore store, INotificationSink? sink = null)
{                                                      // початок класу
    private readonly ICatalogStore _store = store      // зберігаємо базове сховище
        ?? throw new ArgumentNullException(nameof(store));
    
    private readonly INotificationSink? _sink = sink;  // додаткове 1: необов'язковий сповіщувач

    public event EventHandler<CatalogChangedEventArgs>? Changed; 

    private void OnChanged(Product p, CatalogChangeKind kind, int delta) 
    {                                                  
        var args = new CatalogChangedEventArgs(p.Id, kind, delta, p.Quantity); 
        Changed?.Invoke(this, args);                   
        
        _sink?.Notify($"{kind,-8} {p.Id} {delta:+0;-0} → залишок {p.Quantity}");
    }                                                  

    public Product Add(string sku, string name, string unit, int quantity) 
    {                                                  
        string id = Guid.NewGuid().ToString("N")[..8]; 
        var product = Product.Create(id, sku, name, unit, quantity); 
        _store.Add(product);                           
        OnChanged(product, CatalogChangeKind.Added, quantity); 
        return product;                                
    }                                                  

    public void Activate(string id)                    
    {                                                  
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.Activate();                            
        _store.Update(product);                        
    }                                                  

    public void Receive(string id, int quantity)       
    {                                                  
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.RegisterArrival(quantity);             
        _store.Update(product);                        
        OnChanged(product, CatalogChangeKind.Received, quantity); 
    }                                                  

    public void Issue(string id, int quantity)         
    {                                                  
        var product = _store.GetById(id) ?? throw new InvalidOperationException($"Немає запису з id={id}.");
        product.Issue(quantity);                       
        _store.Update(product);                        
        OnChanged(product, CatalogChangeKind.Issued, -quantity); 
    }                                                  

    public IReadOnlyList<Product> All() => _store.List(); 
    public Product? Find(string id) => _store.GetById(id); 
    
    public IReadOnlyList<Product> FindAll(Func<Product, bool> predicate) 
    {                                                  
        return _store.List().Where(predicate).ToList(); 
    }                                                  
}                                                      