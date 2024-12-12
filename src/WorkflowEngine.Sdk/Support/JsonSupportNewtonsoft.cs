using Newtonsoft.Json;
using Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

namespace Nexus.Link.WorkflowEngine.Sdk.Support;

/// <inheritdoc />
public class JsonSupportNewtonsoft : IJsonSupport
{
    /// <inheritdoc />
    public string Serialize<T>(T value)
    {
        return JsonConvert.SerializeObject(value);
    }

    /// <inheritdoc />
    public T Deserialize<T>(string json)
    {
        return JsonConvert.DeserializeObject<T>(json);
    }
}