using Nexus.Link.Libraries.Core.Storage.Model;
using Nexus.Link.Libraries.SqlServer.Model;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary
{
    /// <summary>
    /// The interfaces we expect from a database table item.
    /// </summary>
    public interface ICompleteTableItem : ITableItem, ITimeStamped, IRecordVersion
    {
    }
}