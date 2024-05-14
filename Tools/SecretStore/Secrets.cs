namespace SecretStore;

public static class Secrets
{
    private static ISecretStore? _store;
    public static SecretService? Service { get; private set; }

    public static void Load(string path)
    {
        _store = new FileSecretStore(path);
        Service = new SecretService(_store);
    }
}
