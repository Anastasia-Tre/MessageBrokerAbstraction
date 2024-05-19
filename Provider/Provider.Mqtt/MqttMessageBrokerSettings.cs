using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Extensions.ManagedClient;

namespace Provider.Mqtt
{
    public class MqttMessageBrokerSettings
    {
        public MqttFactory MqttFactory { get; set; } = new();

        public MqttClientOptionsBuilder ClientBuilder { get; set; } = new();

        public ManagedMqttClientOptionsBuilder ManagedClientBuilder { get; set; } = new();
    }
}
