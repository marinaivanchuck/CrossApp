namespace Core.Reports;                                // простір імен звітів

public sealed record GroupSummary(                     // DTO для групи
    string Key,                                        // ключ групи (наприклад, одиниця)
    int Count,                                         // кількість позицій у групі
    int TotalQuantity);                                // сумарна кількість у групі
