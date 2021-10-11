using System.Threading;
using System.Threading.Tasks;
using Nexus.Link.Libraries.Core.Error.Logic;

namespace Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary
{
    public interface IVerifyDatabasePatchLevel
    {
        /// <summary>
        /// 1. SDK version is greater than database level: PATCH
        /// 2. SDK version is less than database level: THROW EXCEPTION
        /// 3. SDK version is same as database level
        /// </summary>
        /// <exception cref="FulcrumAssertionFailedException">If database level is greater than SDK version</exception>
        Task VerifyDatabasePatchLevel(int sdkPatchLevel, CancellationToken cancellationToken = default);
    }
}
