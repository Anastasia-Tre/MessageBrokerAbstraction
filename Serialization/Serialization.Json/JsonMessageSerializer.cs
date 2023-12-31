﻿using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Newtonsoft.Json;
using Serialization.Core;

namespace Serialization.Json;

public class JsonMessageSerializer : IMessageSerializer
{
    private readonly Encoding _encoding;
    private readonly ILogger _logger;
    private readonly JsonSerializerSettings? _serializerSettings;

    public JsonMessageSerializer(JsonSerializerSettings? serializerSettings,
        Encoding? encoding, ILogger logger)
    {
        _serializerSettings = serializerSettings;
        _encoding = encoding ?? Encoding.UTF8;
        _logger = logger;
    }

    public JsonMessageSerializer()
        : this(null, null, NullLogger<JsonMessageSerializer>.Instance)
    {
    }

    public byte[] Serialize(Type? t, object message)
    {
        var jsonPayload =
            JsonConvert.SerializeObject(message, t, _serializerSettings);
        _logger.LogDebug(
            "Type {MessageType} serialized from {Message} to JSON {MessageJson}",
            t, message, jsonPayload);

        var payload = _encoding.GetBytes(jsonPayload);
        return payload;
    }

    public object Deserialize(Type t, byte[] payload)
    {
        var jsonPayload = string.Empty;
        try
        {
            jsonPayload = _encoding.GetString(payload);
            var message =
                JsonConvert.DeserializeObject(jsonPayload, t,
                    _serializerSettings);
            _logger.LogDebug(
                "Type {MessageType} deserialized from JSON {MessageJson} to {Message}",
                t, jsonPayload, message);
            return message;
        }
        catch (Exception e)
        {
            _logger.LogError(e,
                "Type {MessageType} could not been deserialized, payload: {MessagePayload}, JSON: {MessageJson}",
                t,
                _logger.IsEnabled(LogLevel.Debug)
                    ? Convert.ToBase64String(payload)
                    : "(...)", jsonPayload);
            throw;
        }
    }
}
