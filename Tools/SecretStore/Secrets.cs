namespace SecretStore;

public static class Secrets
{
    private static ISecretStore? _store;
    public static SecretService? Service { get; private set; }

    public static void Load(string topic)
    {
        _store = new FileSecretStore(topic);
        Service = new SecretService(_store);
    }
}
