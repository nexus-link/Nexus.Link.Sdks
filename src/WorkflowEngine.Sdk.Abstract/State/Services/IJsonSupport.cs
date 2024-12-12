namespace Nexus.Link.WorkflowEngine.Sdk.Abstract.State.Services;

/// <summary>
/// Support for important json features such as Serialize and Deserialize.
/// </summary>
public interface IJsonSupport
{
    /// <summary>
    /// Serialize <paramref name="value"/> to a JSON string.
    /// </summary>
    public string Serialize<T>(T value);

    /// <summary>
    /// Deserialize the string <paramref name="json"/> to an object of type <typeparamref name="T"/>.
    /// </summary>
    public T Deserialize<T>(string json);
}