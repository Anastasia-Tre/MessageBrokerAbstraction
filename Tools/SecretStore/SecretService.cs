﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SecretStore
{
    public class SecretService
    {
        private readonly Regex _placeholder = new(@"\{\{(\w+)\}\}", RegexOptions.None, TimeSpan.FromSeconds(2));
        private readonly ISecretStore _secretStore;

        public SecretService(ISecretStore secretStore) => _secretStore = secretStore;

        public string PopulateSecrets(string value)
        {
            if (!string.IsNullOrEmpty(value))
            {
                var index = 0;
                Match match;
                do
                {
                    match = _placeholder.Match(value, index);
                    if (match.Success)
                    {
                        var secretName = match.Groups[1].Value;
                        var secretValue = _secretStore.GetSecret(secretName)
                                          ?? throw new ArgumentException($"The secret name '{secretName}' was not present in vault. Ensure that you have a local secrets.txt file in the src folder.");
                        value = value.Replace(match.Value, secretValue, StringComparison.InvariantCulture);
                        index = match.Index + 1;
                    }
                } while (match.Success);
            }
            return value;
        }
    }
}
