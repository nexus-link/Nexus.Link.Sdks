using System.Threading.Tasks;

namespace Nexus.Link.AsyncCaller.Sdk.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IFile : IDirectoryListItem
    {
        Task UploadTextAsync(string content);
    }
}
