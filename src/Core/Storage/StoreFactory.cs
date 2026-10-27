using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");

        ICatalogStore baseStore = useFile
            ? new FileCatalogStore(Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog.json"))
            : new InMemoryCatalogStore(SampleData.Products());

        return new CachingCatalogStore(baseStore);
    }
}
