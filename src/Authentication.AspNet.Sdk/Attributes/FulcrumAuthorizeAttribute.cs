using System;
using System.Collections.Generic;
using System.Linq;
using Nexus.Link.Libraries.Core.Platform.Authentication;
#if NETCOREAPP
using Microsoft.AspNetCore.Authorization;
#else
using System.Web.Http;
using System.Web.Http.Controllers;
#endif

namespace Nexus.Link.Authentication.AspNet.Sdk.Attributes
{
    public class FulcrumAuthorizeAttribute : AuthorizeAttribute
    {
        public FulcrumAuthorizeAttribute(params AuthenticationRoleEnum[] roles)
        {
            Roles = RestrictTo(roles);
        }

#if NETCOREAPP
#else
        protected override bool IsAuthorized(HttpActionContext actionContext)
        {
            var authorized = base.IsAuthorized(actionContext);
            return authorized;
        }
#endif

        private static string RestrictTo(params AuthenticationRoleEnum[] roles)
        {
            // SysAdmin always has access
            var result = new List<string> { From(AuthenticationRoleEnum.SysAdminUser) };
            if (roles != null)
            {
                result.AddRange(roles.Select(From));
            }
            return string.Join(",", result);
        }

        public static string From(AuthenticationRoleEnum role)
        {
            return Enum.GetName(typeof(AuthenticationRoleEnum), role);
        }

    }
}
