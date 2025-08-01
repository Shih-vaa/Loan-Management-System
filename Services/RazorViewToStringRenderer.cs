using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Abstractions;

namespace LoanManagementSystem.Services
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
{
    private readonly IRazorViewEngine _viewEngine;
    private readonly ITempDataProvider _tempDataProvider;
    private readonly IServiceProvider _serviceProvider;
    private readonly IActionContextAccessor _actionContextAccessor;

    public RazorViewToStringRenderer(
        IRazorViewEngine viewEngine,
        ITempDataProvider tempDataProvider,
        IServiceProvider serviceProvider,
        IActionContextAccessor actionContextAccessor)
    {
        _viewEngine = viewEngine;
        _tempDataProvider = tempDataProvider;
        _serviceProvider = serviceProvider;
        _actionContextAccessor = actionContextAccessor;
    }

    public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
    {
        var httpContext = _actionContextAccessor.ActionContext?.HttpContext ?? new DefaultHttpContext { RequestServices = _serviceProvider };
        var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

        using var sw = new StringWriter();
        var viewResult = _viewEngine.FindView(actionContext, viewName, false);
        if (!viewResult.Success)
        {
            throw new InvalidOperationException($"Could not find view '{viewName}'");
        }

        var viewDictionary = new ViewDataDictionary<TModel>(
            new EmptyModelMetadataProvider(),
            new ModelStateDictionary())
        {
            Model = model
        };

        var tempData = new TempDataDictionary(actionContext.HttpContext, _tempDataProvider);
        var viewContext = new ViewContext(
            actionContext,
            viewResult.View,
            viewDictionary,
            tempData,
            sw,
            new HtmlHelperOptions()
        );

        await viewResult.View.RenderAsync(viewContext);
        return sw.ToString();
    }
}

}
