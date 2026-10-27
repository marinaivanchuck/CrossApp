using System.Text.Json;                                // серіалізація JSON
using Core.Abstractions;                               // ICatalogStore
using Core.Domain;                                     // Product
using Core.Dto;                                        // ProductDto

namespace Core.Storage;                                // простір імен сховищ

public sealed class FileCatalogStore(string path) : ICatalogStore
{                                                      // клас реалізує контракт
    private static readonly JsonSerializerOptions Options = new()  // налаштування JSON
    {                                                  // початок ініціалізатора
        WriteIndented = true                           // з переносами рядків (читабельний файл)
    };                                                 // кінець ініціалізатора

    private readonly Dictionary<string, Product> _cache = new(StringComparer.OrdinalIgnoreCase);  // кеш
    private readonly string _path = Path.GetFullPath(path);  // абсолютний шлях до файлу
    private bool _loaded;                              // чи вже завантажили файл

    private void EnsureLoaded()                        // ліниве завантаження
    {                                                  // початок EnsureLoaded
        if (_loaded) return;                           // вже завантажено — вихід

        if (File.Exists(_path))                        // файл існує?
        {                                              // початок if
            string json = File.ReadAllText(_path);     // читаємо вміст
            var dtos = JsonSerializer.Deserialize<List<ProductDto>>(json) ?? [];  // JSON → DTO

            foreach (var dto in dtos)                  // проходимо по DTO
            {
                var p = Product.FromDto(dto);          // DTO → сутність (з перевірками)
                _cache[p.Id] = p;                      // кладемо в кеш
            }
        }                                              // кінець if

        _loaded = true;                                // запам'ятовуємо, що завантажили
    }                                                  // кінець EnsureLoaded

    private void Flush()                               // збереження на диск
    {                                                  // початок Flush
        Directory.CreateDirectory(Path.GetDirectoryName(_path)!);  // створити каталог якщо немає
        var dtos = _cache.Values.Select(p => p.ToDto()).ToList();   // сутності → DTO
        File.WriteAllText(_path, JsonSerializer.Serialize(dtos, Options));  // записати
    }                                                  // кінець Flush

    public IReadOnlyList<Product> List()               // отримати список
    {                                                  // початок List
        EnsureLoaded();                                // завантажити якщо треба
        return _cache.Values.ToList();                 // повернути копію
    }                                                  // кінець List

    public Product? GetById(string id)                 // пошук за id
    {                                                  // початок GetById
        EnsureLoaded();                                // завантажити
        return _cache.GetValueOrDefault(id);           // повернути або null
    }                                                  // кінець GetById

    public void Add(Product item)                      // додати
    {                                                  // початок Add
        ArgumentNullException.ThrowIfNull(item);       // перевірка null
        EnsureLoaded();                                // завантажити
        if (_cache.ContainsKey(item.Id))               // перевірка дублікату
            throw new InvalidOperationException($"Запис з id={item.Id} уже існує.");  // виняток

        _cache.Add(item.Id, item);                     // додати в кеш
        Flush();                                       // записати на диск
    }                                                  // кінець Add

    public void Update(Product item)                   // оновити
    {                                                  // початок Update
        EnsureLoaded();                                // завантажити
        _cache[item.Id] = item;                        // оновити в кеші
        Flush();                                       // записати на диск
    }                                                  // кінець Update

    public bool Remove(string id)                      // видалити
    {                                                  // початок Remove
        EnsureLoaded();                                // завантажити
        if (!_cache.Remove(id)) return false;          // якщо не було — false
        Flush();                                       // записати на диск
        return true;                                   // успіх
    }                                                  // кінець Remove

    // ДОДАТКОВЕ 2: пошук через LINQ
    public IReadOnlyList<Product> Search(Func<Product, bool> predicate)  // пошук за умовою
    {                                                  // початок Search
        EnsureLoaded();                                // завантажити
        return _cache.Values.Where(predicate).ToList();  // фільтруємо кеш
    }                                                  // кінець Search
}
