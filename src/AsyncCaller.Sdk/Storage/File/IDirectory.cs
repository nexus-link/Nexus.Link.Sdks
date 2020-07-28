namespace Xlent.Lever.AsyncCaller.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IDirectory : IDirectoryListItem
    {
        IFile CreateFile(string name, string contentType);
    }
}
