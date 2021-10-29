﻿using Nexus.Link.Capabilities.WorkflowMgmt.Abstract.Entities.State;

namespace Nexus.Link.WorkflowEngine.Sdk.Interfaces
{
    public class ActivityExceptionAlert
    {
        public string WorkflowInstanceId { get; set; }
        public string ActivityInstanceId { get; set; }

        public ActivityExceptionCategoryEnum ExceptionCategory { get; set; }

        public string ExceptionTechnicalMessage { get; set; }

        public string ExceptionFriendlyMessage { get; set; }
    }
}