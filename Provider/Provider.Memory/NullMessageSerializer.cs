using Serialization.Core;

namespace Provider.Memory;

internal class NullMessageSerializer : IMessageSerializer
{
    public object Deserialize(Type t, byte[] payload)
    {
        return null;
    }

    public byte[] Serialize(Type t, object message)
    {
        return null;
    }
}
