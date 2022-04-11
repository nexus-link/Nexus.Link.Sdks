using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Nexus.Link.Libraries.Core.Application;
using Nexus.Link.Libraries.Core.Assert;
using Nexus.Link.Libraries.Core.Logging;
using Nexus.Link.Libraries.Core.Translation;
using Nexus.Link.Libraries.Web.AspNet.Logging;
#if NETCOREAPP

#else
using System.Web.Http.Filters;
using System.Web.Http.Controllers;
using Newtonsoft.Json.Linq;
using Nexus.Link.Libraries.Core.Json;
using System.Net.Http;
using System.Text;
using Nexus.Link.Libraries.Web.Logging;
#endif

namespace Nexus.Link.Misc.AspNet.Sdk.Inbound
{
#if NETCOREAPP
    /// <summary>
    /// Filter to translate values through a value translator
    /// </summary>
    public class ValueTranslatorFilter : IAsyncActionFilter, IAsyncResultFilter
#else
    public class ValueTranslatorFilter : ActionFilterAttribute
#endif
    {
        /// <summary>
        /// The service that does the actual translation.
        /// </summary>
        public ITranslatorService TranslatorService { get; }

        /// <summary>
        /// The concepts which we want to try to translate the UserId to
        /// </summary>
        public static List<string> UserIdConcepts = new();

        private readonly Func<string> _getClientNameMethod;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValueTranslatorFilter(ITranslatorService translatorService, Func<string> getClientNameMethod, IEnumerable<string> userIdConcepts = null)
        {
            InternalContract.RequireNotNull(translatorService, nameof(translatorService));
            InternalContract.RequireNotNull(getClientNameMethod, nameof(getClientNameMethod));
            TranslatorService = translatorService;
            _getClientNameMethod = getClientNameMethod;

            if (userIdConcepts != null)
            {
                UserIdConcepts.AddRange(userIdConcepts);
            }
        }

#if NETCOREAPP
        /// <summary>
        /// This is the logic for the filter
        /// </summary>
        /// <param name="context"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            var translator = GetTranslator(context);
            if (translator != null)
            {
                var controllerActionDescriptor = context.ActionDescriptor as ControllerActionDescriptor;
                var methodInfo = controllerActionDescriptor?.MethodInfo;

                if (methodInfo != null)
                {
                    try
                    {
                        DecorateUserId(translator);
                        DecorateArguments(methodInfo.GetParameters(), context.ActionArguments, translator);
                    }
                    catch (Exception exception)
                    {
                        LogDecorationFailure(context, exception);
                    }
                }
            }

            await next();
        }

        /// <inheritdoc />
        public async Task OnResultExecutionAsync(ResultExecutingContext context, ResultExecutionDelegate next)
        {
            var translator = GetTranslator(context);
            if (translator != null)
            {
                try
                {
                    await TranslateResponseAsync(context, translator, CancellationToken.None);
                }
                catch (Exception exception)
                {
                    await LogTranslationFailureAsync(context, exception, CancellationToken.None);
                    throw;
                }
            }

            await next();
        }
#else
        public override async Task OnActionExecutingAsync(HttpActionContext actionContext, CancellationToken cancellationToken)
        {
            var translator = GetTranslator(actionContext);
            if (translator != null)
            {
                var methodInfo = FindControllerMethod(actionContext);

                // TODO: Generalize to Decorate() method
                if (methodInfo != null)
                {
                    try
                    {
                        DecorateUserId(translator);
                        DecorateArguments(methodInfo.GetParameters(), actionContext.ActionArguments, translator);
                    }
                    catch (Exception exception)
                    {
                        await LogFailureAsync(actionContext.Request, actionContext.Response, exception, cancellationToken);
                    }
                }
            }
        }

        private static MethodInfo FindControllerMethod(HttpActionContext actionContext)
        {
            MethodInfo methodInfo = null;
            try
            {
                var methodInfos = actionContext.ControllerContext.Controller.GetType().GetMethods(BindingFlags.Public | BindingFlags.Instance);
                foreach (var info in methodInfos)
                {
                    var parameterInfos = info.GetParameters();
                    if (info.Name == actionContext.ActionDescriptor.ActionName &&
                        parameterInfos?.Length == actionContext.ActionArguments.Count)
                    {
                        var i = 0;
                        var allGood = true;
                        foreach (var parameterInfo in parameterInfos)
                        {
                            var argType = actionContext.ActionArguments.ElementAt(i).Value.GetType();
                            if (parameterInfo.ParameterType != argType)
                            {
                                allGood = false;
                                break;
                            }
                            i++;
                        }

                        if (allGood)
                        {
                            methodInfo = info;
                            break;
                        }
                    }
                }
            }
            catch
            {
                Log.LogWarning($"Unable to find Controller method {actionContext.ControllerContext.ControllerDescriptor.ControllerName}.{actionContext.ActionDescriptor.ActionName}");
            }
            return methodInfo;
        }

        public override async Task OnActionExecutedAsync(HttpActionExecutedContext actionExecutedContext, CancellationToken cancellationToken)
        {
            var translator = GetTranslator(actionExecutedContext);
            if (translator != null)
            {
                try
                {
                    await TranslateResponseAsync(actionExecutedContext, translator, cancellationToken);
                }
                catch (Exception exception)
                {
                    await LogFailureAsync(actionExecutedContext.Request, actionExecutedContext.Response, exception, cancellationToken);
                    throw;
                }
            }
        }
#endif

        private ITranslator GetTranslator(object context)
        {
            // TODO: Context not needed
            if (context == null || TranslatorService == null) return null;
            var clientName = _getClientNameMethod();
            return clientName == null ? null : new Translator(clientName, TranslatorService);
        }

        private static void DecorateArguments(IEnumerable<ParameterInfo> parameters,
            IDictionary<string, object> arguments, ITranslator translator)
        {
            foreach (var parameterInfo in parameters)
            {
                if (parameterInfo.ParameterType == typeof(string))
                {
                    DecorateValue(arguments, translator, parameterInfo);
                }
                else if (parameterInfo.ParameterType.IsClass)
                {
                    DecorateObject(arguments, translator, parameterInfo);
                }
            }
        }

        private static void DecorateValue(IDictionary<string, object> arguments, ITranslator translator,
            ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.Name;
            var attribute = parameterInfo.GetCustomAttribute<TranslationConceptAttribute>();
            var conceptName = attribute?.ConceptName;
            if (string.IsNullOrWhiteSpace(conceptName)) return;
            if (!arguments.ContainsKey(parameterName)) return;
            var currentValue = arguments[parameterName] as string;
            if (string.IsNullOrWhiteSpace(currentValue)) return;
            arguments[parameterName] = translator.Decorate(conceptName, currentValue);
        }

        private static void DecorateObject(IDictionary<string, object> arguments, ITranslator translator,
            ParameterInfo parameterInfo)
        {
            var parameterName = parameterInfo.Name;
            if (!arguments.ContainsKey(parameterName)) return;
            var currentValue = arguments[parameterName];
            if (currentValue == null) return;
            arguments[parameterName] = translator.Decorate(currentValue);
        }

        private static void DecorateUserId(ITranslator translator)
        {
            if (UserIdConcepts != null && UserIdConcepts.Any())
            {
                var userId = FulcrumApplication.Context.ValueProvider.GetValue<string>("UserId");
                if (!string.IsNullOrWhiteSpace(userId))
                {
                    var decoratedUserIds = new List<string>();
                    foreach (var concept in UserIdConcepts)
                    {
                        var decoratedUserId = translator.Decorate(concept, userId);
                        decoratedUserIds.Add(decoratedUserId);
                    }

                    FulcrumApplication.Context.ValueProvider.SetValue("DecoratedUserIds", decoratedUserIds);
                }

            }
        }

#if NETCOREAPP

        private static void LogDecorationFailure(ActionExecutingContext context, Exception exception)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                resultAsLog = JsonConvert.SerializeObject(context.Result, Formatting.Indented);
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = context.HttpContext.Request.ToLogString();
            }
            catch (Exception)
            {
                requestAsLog = context.HttpContext?.Request?.Path;
            }

            Log.LogError(
                $"Failed to decorate the arguments for the request {requestAsLog}. Result:\r{resultAsLog}",
                exception);
        }

        private static async Task TranslateResponseAsync(ResultExecutingContext context, ITranslator translator,
            CancellationToken cancellationToken)
        {
            if (context?.Result == null) return;
            if (!(context.Result is ObjectResult objectResult)) return;
            if (objectResult.Value == null) return;

            var itemBeforeTranslation = objectResult.Value;

            await translator.Add(itemBeforeTranslation).ExecuteAsync(cancellationToken);
            var itemAfterTranslation = translator.Translate(itemBeforeTranslation, itemBeforeTranslation.GetType());
            context.Result = new ObjectResult(itemAfterTranslation);
        }

        private static async Task LogTranslationFailureAsync(ResultExecutingContext context, Exception exception, CancellationToken cancellationToken)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                resultAsLog = JsonConvert.SerializeObject(context.Result, Formatting.Indented);
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = await context.HttpContext.Request.ToLogStringAsync(context.HttpContext.Response, cancellationToken: cancellationToken);
            }
            catch (Exception)
            {
                requestAsLog = context.HttpContext?.Request?.Path;
            }

            Log.LogError(
                $"Failed to translate the response for the request {requestAsLog}. Result:\r{resultAsLog}",
                exception);
        }
#else
        private static async Task LogFailureAsync(HttpRequestMessage request, HttpResponseMessage response, Exception exception, CancellationToken cancellationToken)
        {
            string requestAsLog;
            string resultAsLog;

            try
            {
                await response.Content.LoadIntoBufferAsync();
                resultAsLog = await response.Content.ReadAsStringAsync();
            }
            catch (Exception e)
            {
                resultAsLog = $"Failed to serialize: {e.Message}";
            }

            try
            {
                requestAsLog = await request.ToLogStringAsync(response, cancellationToken: cancellationToken);
            }
            catch (Exception)
            {
                requestAsLog = request.RequestUri?.ToString();
            }

            Log.LogError($"Failed to decorate the arguments for the request {requestAsLog}. Result:\r{resultAsLog}", exception);
        }

        private static async Task TranslateResponseAsync(HttpActionExecutedContext context, ITranslator translator, CancellationToken cancellationToken = default)
        {
            if (context.ActionContext?.Response?.Content == null) return;
            await context.ActionContext?.Response.Content.LoadIntoBufferAsync();
            var asString = await context.ActionContext?.Response?.Content?.ReadAsStringAsync();
            if (string.IsNullOrWhiteSpace(asString)) return;

            var objectResult = JsonHelper.SafeDeserializeObject<JObject>(asString);
            if (objectResult == null) return;
            var itemBeforeTranslation = objectResult;

            await translator.Add(itemBeforeTranslation).ExecuteAsync(cancellationToken);
            var itemAfterTranslation = translator.Translate(itemBeforeTranslation, itemBeforeTranslation.GetType());
            context.ActionContext.Response.Content = new StringContent(JsonConvert.SerializeObject(itemAfterTranslation), Encoding.UTF8, "application/json");
        }
#endif
    }
#if NETCOREAPP
    /// <summary>
    /// https://stackoverflow.com/questions/55990151/is-adding-addmvc-service-twice-in-configureservices-a-good-practice-in-asp-n
    /// </summary>
    public class ValueTranslatorFilterConfigureOptions : IConfigureOptions<MvcOptions>
    {
        private readonly ITranslatorService _translatorService;

        /// <summary>
        /// Constructor
        /// </summary>
        public ValueTranslatorFilterConfigureOptions(ITranslatorService translatorService)
        {
            _translatorService = translatorService;
        }
        /// <inheritdoc />
        public void Configure(MvcOptions options)
        {
            var valueTranslatorFilter = new ValueTranslatorFilter(
                _translatorService,
                GetClientName);
            options.Filters.Add(valueTranslatorFilter);
        }

        private static string GetClientName() => FulcrumApplication.Context?.ClientPrincipal?.Identity?.Name;
    }

    /// <summary>
    /// Convenience method for adding the <see cref="ValueTranslatorFilter"/>.
    /// </summary>
    public static class ValueTranslatorFilterExtensions
    {
        /// <summary>
        /// Add the <see cref="ValueTranslatorFilter"/> to MVC.
        /// </summary>
        public static IServiceCollection AddValueTranslatorFilter(this IServiceCollection services)
        {
            services.ConfigureOptions<ValueTranslatorFilterConfigureOptions>();
            return services;
        }
    }
#endif
}
