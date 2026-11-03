namespace Core.Events;                                 // простір імен для подій

public enum CatalogChangeKind                          // перелік можливих змін
{                                                      // початок enum
    Added,                                             // товар додано
    Received,                                          // здійснено прихід
    Issued,                                            // здійснено видачу
    Removed                                            // товар видалено
}                                                      // кінець enum
