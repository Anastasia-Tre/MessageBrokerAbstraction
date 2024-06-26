﻿namespace SecretStore;

public class FileSecretStore : ISecretStore
{
    private readonly IDictionary<string, string> _secrets;

    public FileSecretStore(string topic)
    {
        if (!File.Exists(topic))
        {
            _secrets = new Dictionary<string, string>();
            return;
        }

        var lines = File.ReadAllLines(topic);
        _secrets = lines
            .Select(x => x.Split('=', 2, StringSplitOptions.RemoveEmptyEntries))
            .ToDictionary(x => x[0], x => x[1]);
    }

    public string? GetSecret(string name)
    {
        _secrets.TryGetValue(name, out var value);
        return value;
    }
}
