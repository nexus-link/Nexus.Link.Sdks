using System;
using Newtonsoft.Json;

namespace Nexus.Link.WorkflowEngine.Sdk.Internal.Logic;

internal class AbstractConverter<TAbstract, TReal>
    : JsonConverter where TReal : TAbstract
{
    public override bool CanConvert(Type objectType)
        => objectType == typeof(TAbstract);

    public override object ReadJson(JsonReader reader, Type type, Object value, JsonSerializer jser)
        => jser.Deserialize<TReal>(reader);

    public override void WriteJson(JsonWriter writer, Object value, JsonSerializer jser)
        => jser.Serialize(writer, value);
}