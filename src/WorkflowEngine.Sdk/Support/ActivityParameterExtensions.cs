using System;
using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Crud.Helpers;
using Nexus.Link.WorkflowEngine.Sdk.Persistence.Abstract.Entities;

namespace Nexus.Link.WorkflowEngine.Sdk.Support
{
    public static class ActivityParameterExtensions
    {
        /// <summary>
        /// MethodParameterRecordCreate.From(ActivityParameterCreate)
        /// </summary>
        public static ActivityVersionParameterRecordCreate From(this ActivityVersionParameterRecordCreate target, ActivityParameterCreate source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            target.ActivityVersionId = MapperHelper.MapToType<Guid, string>(source.ActivityVersionId);
            target.Name = source.Name;
            return target;
        }

        /// <summary>
        /// MethodParameterRecord.From(ActivityParameter)
        /// </summary>
        public static ActivityVersionParameterRecord From(this ActivityVersionParameterRecord target, ActivityParameter source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));

            ((ActivityVersionParameterRecordCreate) target).From(source);
            target.Id = MapperHelper.MapToType<Guid, string>(source.Id);
            target.Etag = source.Etag;
            return target;
        }

        /// <summary>
        /// ActivityParameter.From(MethodParameterRecord)
        /// </summary>
        public static ActivityParameter From(this ActivityParameter target, ActivityVersionParameterRecord source)
        {
            InternalContract.RequireNotNull(target, nameof(target));
            InternalContract.RequireNotNull(source, nameof(source));
            InternalContract.RequireValidated(source, nameof(source));
            
            target.Id = MapperHelper.MapToType<string, Guid>(source.Id);
            target.ActivityVersionId = MapperHelper.MapToType<string, Guid>(source.ActivityVersionId);
            target.Name = source.Name;
            target.Etag = source.Etag;
            return target;
        }
        
    }
}