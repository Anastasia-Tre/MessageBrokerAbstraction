using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SecretStore
{
    public interface ISecretStore
    {
        string GetSecret(string name);
    }
}
