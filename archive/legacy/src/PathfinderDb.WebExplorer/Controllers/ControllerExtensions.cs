using System.Web.Mvc;

namespace DbBrowser.Controllers
{
    internal static class ControllerExtensions
    {
        public static PathfinderDb.Schema.DataSet DataSet(this Controller controller)
        {
            return Models.MemoryDataSet.LoadDataSet(controller.Server);
        }
    }
}