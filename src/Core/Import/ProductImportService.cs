using System;                                          // Action
using System.Collections.Generic;                      // List
using Core.Domain;                                     // Product
using Core.Dto;                                        // ProductDto, ImportResult

namespace Core.Import;                                 // простір імен імпорту

public static class ProductImportService                // сервіс імпорту
{                                                       // початок класу
    public sealed record ProductImportOutcome(          // результат
        IReadOnlyList<Product> Products,                // успішні
        IReadOnlyList<string> Errors);                  // помилки

    // ДОДАТКОВЕ 2: Action<string> для обробки помилок ззовні
    public static ProductImportOutcome Load(string path, Action<string>? onError = null)
    {                                                   // початок Load
        ImportResult<ProductDto> dtoResult = ProductCsvImporter.Load(path); // парсимо

        var products = new List<Product>();             // список сутностей
        var errors = new List<string>(dtoResult.Errors); // переносимо помилки парсингу

        // Викликаємо Action для кожної помилки парсингу, якщо він переданий
        if (onError != null)                            // якщо є обробник
            foreach (var e in dtoResult.Errors) onError(e); // викликаємо делегат

        for (int i = 0; i < dtoResult.Items.Count; i++) // цикл по DTO
        {                                               // початок циклу
            ProductDto dto = dtoResult.Items[i];        // беремо DTO
            try                                         // спроба створити
            {                                           // початок try
                products.Add(Product.FromDto(dto));     // FromDto перевіряє інваріанти
            }                                           // кінець try
            catch (Exception ex)                        // якщо інваріант порушено
            {                                           // початок catch
                string msg = $"DTO {dto.Id}: {ex.Message}"; // формуємо текст
                errors.Add(msg);                        // додаємо в загальний список
                onError?.Invoke(msg);                   // ВИКЛИКАЄМО Action (якщо є)
            }                                           // кінець catch
        }                                               // кінець циклу

        return new ProductImportOutcome(products, errors); // повертаємо результат
    }                                                   // кінець Load
}                                                       // кінець класу
