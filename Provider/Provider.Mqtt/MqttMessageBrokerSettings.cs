using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Provider.Mqtt;

public class MqttMessageBrokerSettings
{
    public MqttFactory MqttFactory { get; set; } = new();

    public MqttClientOptionsBuilder ClientBuilder { get; set; } = new();

    public ManagedMqttClientOptionsBuilder ManagedClientBuilder { get; set; } =
        new();
}
