using System.Threading.Tasks;

namespace LoanManagementSystem.Services
{
    public interface IRazorViewToStringRenderer
    {
        Task<string> RenderViewToStringAsync<TModel>(string viewPath, TModel model);
    }
}
