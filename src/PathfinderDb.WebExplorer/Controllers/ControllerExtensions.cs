using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;

namespace DbBrowser.Controllers
{
    internal static class ControllerExtensions
    {
        public static PathfinderDb.Schema.DataSet DataSet(this Controller controller)
        {
            var env = (IWebHostEnvironment)controller.HttpContext.RequestServices.GetService(typeof(IWebHostEnvironment));
            return Models.MemoryDataSet.LoadDataSet(env.ContentRootPath);
        }
    }
}