using System;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Temporary;

namespace Nexus.Link.WorkflowEngine.Sdk.Temporary
{
    [Obsolete("You are discouraged to use the SQL implementation of Nexus Workflows. Instead use the Rest Clients implementation.")]
    public static class DatabasePatchSettings
    {
        /// <summary>
        /// Note! Keep this in sync with patch scripts in WorkflowEngine.Sdk.Persistence.Sql.
        ///
        /// Until we use the Workflow Engine SaaS,
        /// we use the <see cref="IVerifyDatabasePatchLevel"/> mechanism to handle database patching.
        /// </summary>
        public const int DatabasePatchVersion = 2;

        public static IVerifyDatabasePatchLevel DatabasePatchLevelVerifier { get; set; }

    }
}
