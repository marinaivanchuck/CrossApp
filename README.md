# CrossApp

Наскрізний проєкт з крос-платформного програмування.

**Предметна область**: Склад.  
**Сутності**:
- Product (товар)
- StockBatch (партія)
- Warehouse (склад)
- Movement (переміщення)

**Призначення**: облік залишків товарів по партіях.

## Доменна модель

Сутності:
- **Product** (`src/Core/Domain/Product.cs`) — товар із залишком
- **Warehouse** (`src/Core/Domain/Warehouse.cs`) — склад із колекцією товарів

### Інваріанти Product

| Правило | Тип винятку | Метод |
|---|---|---|
| Ідентифікатор не порожній | ArgumentException | Create |
| SKU не порожній | ArgumentException | Create |
| Назва не порожня | ArgumentException | Create |
| Початковий залишок ≥ 0 | ArgumentOutOfRangeException | Create |
| Кількість приходу > 0 | ArgumentOutOfRangeException | RegisterArrival |
| Кількість видачі > 0 | ArgumentOutOfRangeException | Issue |
| Не можна видати більше залишку | InvalidOperationException | Issue |
| Видача лише зі стану Active | InvalidOperationException | Issue |
| Прихід заборонено зі стану Archived | InvalidOperationException | RegisterArrival |
| Перехід Draft → Active | InvalidOperationException | Activate |
| Перехід Active → Archived | InvalidOperationException | Archive |

### Інваріант Warehouse (охоплює дві сутності)

| Правило | Тип винятку | Метод |
|---|---|---|
| SKU унікальний у межах одного складу | InvalidOperationException | AddProduct |

### Принципи

- Публічного конструктора немає — лише `Create(...)`.
- Id, Sku, Name, Unit — незмінні після створення.
- Залишок — приватне поле, змінюється методами.
- Статус — `{ get; private set; }` + перевірка переходів через `switch`.
- `ToDto()` / `FromDto()` — мапінг із форматом тижня 3.
- Колекція `_products` — приватна; назовні — `IReadOnlyList`.

## Запуск

dotnet build
dotnet run --project src/Cli

## Середовище

- .NET SDK 10.0.401
- macOS Ventura 13.5 (Darwin 22.6.0) на Apple M1 (ARM64)
- RID: osx-arm64

## Розмір self-contained publish

| RID       | Розмір |
|-----------|--------|
| osx-arm64 | 83 MB  |
| linux-x64 | 79 MB  |

Публікація виконана командою:
dotnet publish src/Cli -c Release -r RID --self-contained true