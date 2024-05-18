using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using MessageBroker.Core.MessageBroker;
using MessageBroker.Core.Subscriber;

namespace Provider.Memory
{
    public class MemoryMessageBrokerBuilder : MessageBrokerBuilder
    {
        internal MemoryMessageBrokerBuilder(MessageBrokerBuilder other) : base(other)
        {
        }

        private static string DefaultMessageTypeToTopicConverter(Type type) => type.Name;

        public MemoryMessageBrokerBuilder AutoDeclareFrom(IEnumerable<Assembly> assemblies, Func<Type, bool> subscriberTypeFilter = null, Func<Type, string> messageTypeToTopicConverter = null)
        {
            messageTypeToTopicConverter ??= DefaultMessageTypeToTopicConverter;

            var prospectTypes = assemblies
                .SelectMany(x => x.GetTypes())
                .Where(t => t.IsClass && !t.IsAbstract)
                .Where(x => x.GetInterfaces().First().IsGenericType && x.GetInterfaces().First().GetGenericTypeDefinition() == typeof(ISubscriber<>))
                .Where(x => subscriberTypeFilter == null || subscriberTypeFilter(x))
                .Where(x => !x.IsGenericType || x.IsConstructedGenericType);

            var foundSubscribers = prospectTypes; //.Where(x => x.GetInterfaces().First().GetGenericTypeDefinition() == typeof(ISubscriber<>)).ToList();
            
            var knownMessageTypes = new HashSet<Type>();

            // register all subscribers, handlers
            foreach (var subscriber in foundSubscribers)
            {
                knownMessageTypes.Add(subscriber.GetInterfaces().First().GetGenericArguments().First());
            }

            var ancestorsByType = new Dictionary<Type, ISet<Type>>();
            foreach (var messageType in knownMessageTypes)
            {
                ancestorsByType[messageType] = GetAncestorTypes(messageType);
            }

            var rootMessageTypes = new List<Type>();
            // register all the publishers
            foreach (var messageType in knownMessageTypes)
            {
                var messageTypeAncestors = ancestorsByType[messageType];

                // register root message types only
                var isRoot = messageTypeAncestors.All(ancestor => !knownMessageTypes.Contains(ancestor));
                if (isRoot)
                {
                    rootMessageTypes.Add(messageType);

                    var topicName = messageTypeToTopicConverter(messageType);
                    SetPublisher(messageType, x => x.DefaultTopic(topicName));
                }
            }

            // register all subscribers
            foreach (var rootMessageType in rootMessageTypes)
            {
                var subscribers = foundSubscribers.Where(x => x.GetInterfaces().First().GetGenericArguments().First() == rootMessageType 
                    || ancestorsByType[x.GetInterfaces().First().GetGenericArguments().First()].Contains(rootMessageType)).ToList();
                if (subscribers.Count > 0)
                {
                    var topicName = messageTypeToTopicConverter(rootMessageType);

                    // register subscriber
                    SetSubscriber(rootMessageType, x =>
                    {
                        x.Topic(topicName);
                        foreach (var subscriber in subscribers)
                        {
                            if (subscriber.GetInterfaces().First().GetGenericArguments().First() == rootMessageType && !x.SubscriberSettings.Invokers.Contains(x.SubscriberSettings))
                            {
                                x.WithSubscriber(subscriber);
                            }
                        }
                    });
                }
            }

            return this;
        }

        private static ISet<Type> GetAncestorTypes(Type messageType)
        {
            var ancestors = new HashSet<Type>();
            for (var mt = messageType; mt.BaseType != typeof(object) && mt.BaseType != null; mt = mt.BaseType)
            {
                ancestors.Add(mt.BaseType);
            }
            return ancestors;
        }
    }
}
