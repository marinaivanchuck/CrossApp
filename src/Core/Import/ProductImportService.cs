using Core.Domain;                                     // Product
using Core.Dto;                                        // ProductDto, ImportResult

namespace Core.Import;                                 // простір імен імпорту

public static class ProductImportService                // сервіс імпорту
{                                                       // початок класу
    // Результат: сутності + перелік рядків, які НЕ пройшли інваріанти
    public sealed record ProductImportOutcome(          // record-результат
        IReadOnlyList<Product> Products,                // успішно створені сутності
        IReadOnlyList<string> Errors);                  // помилки (з номерами)

    public static ProductImportOutcome Load(string path)  // завантаження
    {                                                   // початок Load
        ImportResult<ProductDto> dtoResult = ProductCsvImporter.Load(path);  // парсимо CSV → DTO

        var products = new List<Product>();             // сюди складаємо валідні сутності
        var errors = new List<string>(dtoResult.Errors); // переносимо помилки парсингу

        for (int i = 0; i < dtoResult.Items.Count; i++) // обробляємо кожен DTO
        {                                               // початок циклу
            ProductDto dto = dtoResult.Items[i];        // беремо DTO
            try                                         // try: спроба створити сутність
            {                                           // початок try
                products.Add(Product.FromDto(dto));     // FromDto проходить ті самі перевірки
            }                                           // кінець try
            catch (Exception ex)                        // catch: інваріант порушено
            {                                           // початок catch
                errors.Add($"DTO {dto.Id}: {ex.Message}");  // додаємо помилку
            }                                           // кінець catch
        }                                               // кінець циклу

        return new ProductImportOutcome(products, errors);  // повертаємо результат
    }                                                   // кінець Load
}                                                       // кінець класу
