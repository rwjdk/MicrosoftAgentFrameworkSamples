namespace BuildYourOwnVectorStore.Helpers;

public static class FolderHelper
{
    public static string GetStoreFolder(string name)
    {
        string storeFolder = Path.Combine(GetStoresFolder(), name);
        Directory.CreateDirectory(storeFolder);
        return storeFolder;
    }

    public static string GetStoresFolder()
    {
        string storesFolder = Path.Combine(Path.GetTempPath(), "VectorStoreCollections");
        Directory.CreateDirectory(storesFolder);
        return storesFolder;
    }
}