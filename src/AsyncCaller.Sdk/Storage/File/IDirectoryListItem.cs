using System;
using System.Threading.Tasks;

namespace Xlent.Lever.AsyncCaller.Storage.File
{
    /// <summary>
    /// A generic interface for adding strings to a queue.
    /// </summary>
    public interface IDirectoryListItem
    {
        /// <summary>
        /// The name of the queue.
        /// </summary>
        string Name { get; }

        Uri Uri { get; }

        Task DeleteAsync();
    }
}
