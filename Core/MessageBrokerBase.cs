using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serialization;

namespace Core
{
    public abstract class MessageBrokerBase : IMessageBroker
    {
        private readonly ILogger _logger;
        public ILoggerFactory LoggerFactory { get; }

        public virtual MessageBrokerSettings Settings { get; }

        private IMessageSerializer _serializer;
        public virtual IMessageSerializer Serializer
        {
            get {
                _serializer ??= GetSerializer();
                return _serializer;
            }
        }

        protected virtual IMessageSerializer GetSerializer() =>
            (IMessageSerializer)Settings.ServiceProvider.GetService(
                Settings.SerializerType);
            //?? throw new ConfigurationMessageBusException($"The bus {Name} could not resolve the required message serializer type {Settings.SerializerType.Name} from {nameof(Settings.ServiceProvider)}");


        protected MessageBrokerBase(MessageBrokerSettings settings)
        {
            if (settings is null) throw new ArgumentNullException(nameof(settings));
            //if (settings.ServiceProvider is null) throw new ConfigurationMessageBusException($"The bus {Name} has no {nameof(settings.ServiceProvider)} configured");

            Settings = settings;

            LoggerFactory = settings.ServiceProvider.GetService<ILoggerFactory>() ?? NullLoggerFactory.Instance;
            _logger = LoggerFactory.CreateLogger<MessageBrokerBase>();
        }

        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public Task Publish<TMessage>(TMessage message)
        {
            throw new NotImplementedException();
        }
    }
}
