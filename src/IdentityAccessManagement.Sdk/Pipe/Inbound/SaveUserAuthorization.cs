using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Context;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Misc;

namespace IdentityAccessManagement.Sdk.Pipe.Inbound
{
    public class SaveUserAuthorization
    {
        private readonly RequestDelegate _next;
        private readonly IContextValueProvider _provider;
        private static string ContextKey = "UserAuthorization";

        public SaveUserAuthorization(RequestDelegate next)
        {
            _next = next;
            _provider = new AsyncLocalContextValueProvider();
        }

        public async Task InvokeAsync(HttpContext context)
        {
            SaveUserAuthorizationToExecutionContext(context);
            await _next(context);
        }

        private void SaveUserAuthorizationToExecutionContext(HttpContext context)
        {
            var userAuthorization = ExtractUserAuthorizationFromHeader(context);
            if (userAuthorization == null) return;
            _provider.SetValue(ContextKey, userAuthorization);
        }

        private static string ExtractUserAuthorizationFromHeader(HttpContext context)
        {
            var request = context.Request;

            FulcrumAssert.IsNotNull(request, CodeLocation.AsString());
            if (request == null) return null;
            var authorizationHeaderExists =
                request.Headers.TryGetValue("Authorization", out var authorizationValues);

            if (!authorizationHeaderExists) return null;

            var authorizationArray = authorizationValues.ToArray();
            if (authorizationArray.Length > 1)
            {
                // ReSharper disable once UnusedVariable
                var message =
                    $"There was more than one authorization value in the header: {string.Join(", ", authorizationArray)}. The first one was picked as User Authorization";
                Log.LogWarning(message);
            }

            return authorizationArray[0];
        }
    }

    public static class SaveUserAuthorizationExtension
    {
        public static IApplicationBuilder UseNexusSaveCorrelationId(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<SaveUserAuthorization>();
        }
    }
}