using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using Microsoft.Extensions.DependencyInjection;
using Dzaba.QueueSimulator.Lib;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Dzaba.QueueSimulator.WebApi.ActionFilters;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class, Inherited = true, AllowMultiple = false)]
public class HandleErrorsAttribute : ActionFilterAttribute, IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        var container = context.HttpContext.RequestServices;
        var logger = container.GetRequiredService<ILogger<HandleErrorsAttribute>>();

        if (context.Exception is ExitCodeException exitEx)
        {
            logger.LogWarning(exitEx, "Exit code response error.");

            var modelState = new ModelStateDictionary();
            foreach (var error in exitEx.Errors)
            {
                modelState.AddModelError(error.Key.ToString(), error.Value);
            }

            context.Result = new BadRequestObjectResult(modelState);
            return;
        }
    }
}
