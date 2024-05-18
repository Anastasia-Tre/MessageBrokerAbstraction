using Serialization.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Memory
{
    internal class NullMessageSerializer : IMessageSerializer
    {
        public object Deserialize(Type t, byte[] payload) => null;
        public byte[] Serialize(Type t, object message) => null;
    }
}
