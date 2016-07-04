using Our.Umbraco.IpFilter.Models;
using Our.Umbraco.IpFilter.Services;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;

namespace Our.Umbraco.IpFilter.Web.Controllers
{
    [PluginController("IpFilter")]
    public class IpFilterApiController : UmbracoApiController
    {
        private readonly IpFilterService _ipFilterService;

        public IpFilterApiController()
        {
            _ipFilterService = new IpFilterService();
        }

        [System.Web.Http.HttpGet]
        [global::Umbraco.Web.Mvc.UmbracoAuthorize]
        public IpFilterEntry GetEntryByNodeId(int nodeId)
        {
            return _ipFilterService.GetEntryByNodeId(nodeId);
        }

        [System.Web.Http.HttpPost]
        [global::Umbraco.Web.Mvc.UmbracoAuthorize]
        public void SaveEntry(IpFilterEntry entry)
        {
            _ipFilterService.SaveEntry(entry);
        }
    }
}