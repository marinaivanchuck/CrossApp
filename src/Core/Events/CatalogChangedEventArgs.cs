using System;                                          // EventArgs, DateTimeOffset

namespace Core.Events;                                 // простір імен для подій

public sealed class CatalogChangedEventArgs(           // успадковуємо від EventArgs
    string id, CatalogChangeKind kind, int delta, int quantityAfter) : EventArgs
{                                                      // початок класу
    public string Id { get; } = id;                    // ID товару
    public CatalogChangeKind Kind { get; } = kind;     // тип зміни (Added, Received...)
    public int Delta { get; } = delta;                 // на скільки змінилась кількість (+/-)
    public int QuantityAfter { get; } = quantityAfter; // залишок після зміни
    public DateTimeOffset At { get; } = DateTimeOffset.Now; // час події
}                                                      // кінець класу
