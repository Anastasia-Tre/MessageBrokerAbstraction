using MessageBroker.Core.MessageBroker;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Provider.Mqtt
{
    public static class MessageBrokerBuilderExtensions
    {
        public static MessageBrokerBuilder WithProviderMqtt(this MessageBrokerBuilder mbb, Action<MqttMessageBrokerSettings> configure)
        {
            if (mbb == null) throw new ArgumentNullException(nameof(mbb));
            if (configure == null) throw new ArgumentNullException(nameof(configure));

            var providerSettings = new MqttMessageBrokerSettings();
            configure(providerSettings);

            return mbb.WithProvider(settings => new MqttMessageBroker(settings, providerSettings));
        }
    }
}
