using System;                                          
using System.IO;                                       
using System.Text;                                     
using Core.Abstractions;                               
using Core.Events;                                     
using Core.Import;                                     
using Cli.Notifications;                               
using Core.Services;                                   
using Core.Storage;                                    

Console.OutputEncoding = Encoding.UTF8;                

ICatalogStore store = StoreFactory.Create(args);       
var fileSink = new FileSink(Path.Combine(Directory.GetCurrentDirectory(), "logs", "sink.log")); 

bool useSink = args.Contains("--sink");
var service = new CatalogService(store, useSink ? fileSink : null);

var logPath = Path.Combine(Directory.GetCurrentDirectory(), "logs", "app.log");
Directory.CreateDirectory(Path.GetDirectoryName(logPath)!);

void OnCatalogChanged(object? sender, CatalogChangedEventArgs e) 
{                                                      
    var line = $"{e.At:HH:mm:ss} {e.Kind,-8} {e.Id} {e.Delta,4:+0;-0} → залишок {e.QuantityAfter}";
    Console.WriteLine($"[EVENT] {line}");              
    
    // ЗАХИСТ ВІД ПОМИЛКИ 6
    try 
    {
        File.AppendAllText(logPath, line + Environment.NewLine); 
    }
    catch (Exception ex) 
    {
        Console.WriteLine($"[LOG ERROR] Не вдалося записати у файл: {ex.Message}");
    }
}                                                      

service.Changed += OnCatalogChanged;                   

int changes = 0;                                       
service.Changed += (_, _) => changes++;                

Console.WriteLine("=== Сценарій 1: Успішні операції ===");
var created = service.Add("SKU-101", "Кабель UTP cat6", "м", 50);
service.Activate(created.Id); 
service.Receive(created.Id, 25); 
service.Issue(created.Id, 10); 
Console.WriteLine($"Подій отримано (очікується 3): {changes}");

Console.WriteLine("\n=== Сценарій 2: Відписка ===");
service.Changed -= OnCatalogChanged;                   
service.Receive(created.Id, 5);                        
Console.WriteLine($"Подій отримано (очікується 4, бо лямбда все ще працює): {changes}");

Console.WriteLine("\n=== Сценарій 3: Помилка (події бути не має) ===");
try { service.Receive("no-such-id", 10); } 
catch (Exception ex) { Console.WriteLine($"Виняток: {ex.Message}"); }
Console.WriteLine($"Подій отримано (не змінилось): {changes}");

Console.WriteLine("\n=== Сценарій 4: Func та Action ===");
var lowStock = service.FindAll(p => p.Quantity < 50);  
Console.WriteLine($"Товарів < 50: {lowStock.Count}");

Console.WriteLine("Імпорт з обробкою помилок через Action:");
var importOutcome = ProductImportService.Load("data/sample.csv", 
    msg => Console.WriteLine($"  [ACTION] {msg}"));    
Console.WriteLine($"Імпортовано: {importOutcome.Products.Count}, помилок: {importOutcome.Errors.Count}");

return 0;