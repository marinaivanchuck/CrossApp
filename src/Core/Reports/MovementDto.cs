namespace Core.Reports;                                // простір імен звітів

public sealed record MovementDto(                      // імітація переміщення товару
    string ProductId,                                  // ID товару
    int Delta);                                        // на скільки змінилась кількість
