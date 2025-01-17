using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.Activities;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;
using Nexus.Link.WorkflowEngine.Sdk.Internal.Support;
using System;

namespace Nexus.Link.WorkflowEngine.Sdk.Support;

/// <inheritdoc />
public class JsonSupportNewtonsoft : IJsonSupport
{
    private static JsonConverter[] jsonConverters = {
        new JobResultsConverter()
    };
    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value, jsonConverters);
    }

    /// <inheritdoc />
    public T Deserialize<T>(string json)
    {
        
        return JsonConvert.DeserializeObject<T>(json, jsonConverters);
    }
}

internal class JobResultsConverter : JsonConverter
{
    // We only want to handle the IJobResults interface
    public override bool CanConvert(Type objectType)
    {
        return (objectType == typeof(IJobResults));
    }

    // Deserialize JSON -> always instantiate JobResults
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        // Load the JSON object
        var jsonObject = JObject.Load(reader);

        // Since there's only one implementation, just create a new JobResults
        var target = new JobResults();

        // Populate that object with the JSON data
        serializer.Populate(jsonObject.CreateReader(), target);

        return target;
    }

    // Serialize an IJobResults -> we know it's always a JobResults
    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        // Cast to IJobResults (which we know is actually JobResults)
        var jobResults = (IJobResults)value;

        // Start writing the object
        writer.WriteStartObject();

        // Write all of the properties from the concrete type (JobResults)
        foreach (var prop in value.GetType().GetProperties())
        {
            if (prop.CanRead)
            {
                writer.WritePropertyName(prop.Name);
                serializer.Serialize(writer, prop.GetValue(value));
            }
        }

        // End object
        writer.WriteEndObject();
    }
}