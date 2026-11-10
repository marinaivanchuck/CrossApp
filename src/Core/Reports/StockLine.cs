namespace Core.Reports;                                // простір імен звітів

public sealed record StockLine(                        // DTO для звіту (не сутність!)
    string Sku,                                        // артикул
    string Name,                                       // назва
    int Quantity,                                      // кількість
    string Unit);                                      // одиниця виміру
