using System.Globalization;
using Core.Dto;

namespace Core.Import;

public static class MixedCsvImporter
{
    private const char Separator = ';';

    public static (IReadOnlyList<ProductDto> Products,
                   IReadOnlyList<WarehouseDto> Warehouses,
                   IReadOnlyList<string> Errors)
        Load(string path)
    {
        var products = new List<ProductDto>();
        var warehouses = new List<WarehouseDto>();
        var errors = new List<string>();

        string[] lines = File.ReadAllLines(path);

        for (int i = 0; i < lines.Length; i++)
        {
            int number = i + 1;
            string line = lines[i];

            if (string.IsNullOrWhiteSpace(line) || line.StartsWith('#'))
                continue;
            if (number == 1 && line.StartsWith("type", StringComparison.OrdinalIgnoreCase))
                continue;

            string[] parts = line.Split(Separator, StringSplitOptions.TrimEntries);

            switch (parts)
            {
                case ["P", _, "", _, _, _]:
                    errors.Add($"рядок {number}: SKU порожній");
                    break;

                case ["P", _, _, _, _, var qty]
                    when !int.TryParse(qty, NumberStyles.Integer, CultureInfo.InvariantCulture, out int q) || q < 0:
                    errors.Add($"рядок {number}: кількість '{qty}' не число");
                    break;

                case ["P", var id, var sku, var name, var unit, var qty]:
                    products.Add(new ProductDto(id, sku, name, unit,
                        int.Parse(qty, CultureInfo.InvariantCulture)));
                    break;

                case ["W", var id, var name, var addr]:
                    warehouses.Add(new WarehouseDto(id, name, addr));
                    break;

                case ["W", ..]:
                    errors.Add($"рядок {number}: очікую 4 колонки для складу");
                    break;

                                default:
                    errors.Add($"рядок {number}: невідомий тип '{parts[0]}'");
                    break;
            }
        }

        return (products, warehouses, errors);
    }
}
