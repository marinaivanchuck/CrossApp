# CrossApp

Наскрізний проєкт з крос-платформного програмування.

**Предметна область**: Склад.  
**Сутності**:
- Product (товар)
- StockBatch (партія)
- Warehouse (склад)
- Movement (переміщення)

**Призначення**: облік залишків товарів по партіях.

## Запуск

dotnet build
dotnet run --project src/Cli

## Середовище

- .NET SDK 9.0.317
- macOS Ventura 13.5 (Darwin 22.6.0) на Apple M1 (ARM64)
- RID: osx-arm64

## Розмір self-contained publish

| RID       | Розмір |
|-----------|--------|
| osx-arm64 | 81 MB  |
| linux-x64 | 76 MB  |

Публікація виконана командою:
dotnet publish src/Cli -c Release -r RID --self-contained true