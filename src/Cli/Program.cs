using System.Text;
using Core.Dto;
using Core.Import;

Console.OutputEncoding = Encoding.UTF8;

string path = args.Length > 0 ? args[0] : Path.Combine("data", "sample.csv");

if (!File.Exists(path))
{
    Console.WriteLine($"Файл не знайдено: {Path.GetFullPath(path)}");
    return 1;
}
if (args.Contains("--mixed"))
{
    var (prods, warehouses, errs) = MixedCsvImporter.Load(path);
    Console.WriteLine($"Товарів: {prods.Count}, складів: {warehouses.Count}, помилок: {errs.Count}");
    foreach (var p in prods.Take(3))
        Console.WriteLine($"  P {p.Id} {p.Name} — {p.Quantity} {p.Unit}");
    foreach (var w in warehouses)
        Console.WriteLine($"  W {w.Id} {w.Name} ({w.Address})");
    foreach (var e in errs)
        Console.WriteLine($"  ! {e}");
    return 0;
}
string ext = Path.GetExtension(path).ToLowerInvariant();

ImportResult<ProductDto> result = ext switch
{
    ".json" => ProductJsonImporter.Load(path),
    _       => ProductCsvImporter.Load(path)
};

Console.WriteLine($"Завантажено записів: {result.Items.Count}");
foreach (ProductDto p in result.Items.Take(5))
    Console.WriteLine($" {p.Id,-6} {p.Sku,-10} {p.Name,-30} {p.Quantity,6} {p.Unit}");

if (result.Errors.Count > 0)
{
    Console.WriteLine($"Пропущено рядків: {result.Errors.Count}");
    foreach (string e in result.Errors)
        Console.WriteLine($" ! {e}");
}

int total = result.Items.Count + result.Errors.Count;
double errorPct = total > 0 ? 100.0 * result.Errors.Count / total : 0;
Console.WriteLine($"Статистика: всього {total}, прийнято {result.Items.Count}, " +
                  $"пропущено {result.Errors.Count} ({errorPct:F1}%)");

return 0;
