using System.Text.Json;
using Core.Dto;

namespace Core.Import;

public static class ProductJsonImporter
{
    private static readonly JsonSerializerOptions Options = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public static ImportResult<ProductDto> Load(string path)
    {
        var errors = new List<string>();

        try
        {
            string json = File.ReadAllText(path);
            var items = JsonSerializer.Deserialize<List<ProductDto>>(json, Options) ?? [];

            var validItems = new List<ProductDto>();
            for (int i = 0; i < items.Count; i++)
            {
                var p = items[i];
                if (string.IsNullOrWhiteSpace(p.Id) ||
                    string.IsNullOrWhiteSpace(p.Sku) ||
                    string.IsNullOrWhiteSpace(p.Name))
                {
                    errors.Add($"запис {i + 1}: Id/Sku/Name порожні");
                    continue;
                }
                if (p.Quantity < 0)
                {
                    errors.Add($"запис {i + 1}: кількість {p.Quantity} від'ємна");
                    continue;
                }
                validItems.Add(p);
            }

            return new ImportResult<ProductDto>(validItems, errors);
        }
        catch (JsonException ex)
        {
            errors.Add($"помилка JSON: {ex.Message}");
            return new ImportResult<ProductDto>([], errors);
        }
    }
}
