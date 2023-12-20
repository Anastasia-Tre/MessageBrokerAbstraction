﻿using Newtonsoft.Json;
using System.Text;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Serialization.Core;
using MessageBroker.Core.MessageBroker;

namespace Serialization.Json;

public static class MessageBrokerBuilderExtensions
{
    public static MessageBrokerBuilder AddJsonSerializer(this MessageBrokerBuilder mbb, Encoding encoding = null, JsonSerializerSettings jsonSerializerSettings = null)
    {
        mbb.PostConfigurationActions.Add(services =>
        {
            services.TryAddSingleton(svp 
                => new JsonMessageSerializer(jsonSerializerSettings ?? svp.GetService<JsonSerializerSettings>(),
                    encoding, svp.GetRequiredService<ILogger<JsonMessageSerializer>>()));

            services.TryAddSingleton<IMessageSerializer>(svp => svp.GetRequiredService<JsonMessageSerializer>());
        });
        return mbb;
    }
}