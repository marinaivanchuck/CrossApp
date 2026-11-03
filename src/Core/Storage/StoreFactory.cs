using Core.Abstractions;

namespace Core.Storage;

public static class StoreFactory
{
    public static ICatalogStore Create(string[] args)
    {
        bool useFile = args.Contains("--file");

        return useFile
            ? new FileCatalogStore(Path.Combine(Directory.GetCurrentDirectory(), "data", "catalog.json"))
            : new InMemoryCatalogStore(SampleData.Products());
    }
}