using System.Linq;
using System.Web;
using Our.Umbraco.IpFilter.Data.Repositories;
using Our.Umbraco.IpFilter.Extensions;
using Our.Umbraco.IpFilter.Services;
using Umbraco.Core;
using Umbraco.Web;
using Umbraco.Web.Routing;

namespace Our.Umbraco.IpFilter
{
    internal class Bootstrapper : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
        {
            global::Umbraco.Web.Trees.ContentTreeController.TreeNodesRendering += (sender, args) =>
            {
                if (sender.TreeAlias == "content")
                {
                    var ipFilterService = new IpFilterService();

                    foreach (var node in args.Nodes
                        .Where(x => int.Parse((string)x.Id) > 0
                            && ipFilterService.IsIpProtected(int.Parse((string)x.Id), checkUnpublished: true)))
                    {
                        node.CssClasses.Add("protected");

                        // If this node doesn't have an entry specifically then mark it grey
                        if (!ipFilterService.IsIpProtected(int.Parse((string)node.Id), false, true))
                        {
                            node.CssClasses.Add("alt");
                        }
                    }
                }
            };

            // Register the IpFilter menu item
            global::Umbraco.Web.Trees.ContentTreeController.MenuRendering += (sender, args) =>
            {
                if (sender.TreeAlias == "content")
                {
                    var nodeId = int.Parse(args.NodeId);
                    if (nodeId > 0)
                    {
                        var nodePath = "";

                        // See if node is in content cache
                        var node = UmbracoContext.Current.ContentCache.GetById(nodeId);
                        if (node != null)
                        {
                            nodePath = node.Path;
                        }

                        // Node not in content cache, so get it from the db
                        if (nodePath.IsNullOrWhiteSpace())
                        {
                            var contentNode = ApplicationContext.Current.Services.ContentService.GetById(nodeId);
                            if (contentNode != null)
                            {
                                nodePath = contentNode.Path;
                            }
                        }

                        // Check to see if we are in main content tree and not the trash
                        if (!nodePath.IsNullOrWhiteSpace() && nodePath.StartsWith("-1,"))
                        {
                            // Create the menu item
                            var i = new global::Umbraco.Web.Models.Trees.MenuItem("ipFilter", "IP Filter")
                            {
                                Icon = "lock",
                            };

                            // Set action to correct view
                            i.AdditionalData.Add("actionView", "/App_Plugins/IpFilter/Views/ipFilter.html");

                            // Insert the menu item
                            var paIdx = args.Menu.Items.FindIndex(x => x.Alias == "protect");

                            args.Menu.Items.Insert(paIdx + 1, i);

                        }
                    }
                }
            };

            // Enfore IP restriction
            global::Umbraco.Web.Routing.PublishedContentRequest.Prepared += (sender, args) =>
            {
                var req = sender as PublishedContentRequest;
                if (req != null)
                {
                    var errorPageNodeId = 0;
                    var ipAddress = HttpContext.Current.Request.GetClientIpAddress();

                    if (!req.PublishedContent.CanAccess(ipAddress, out errorPageNodeId))
                    {
                        if (errorPageNodeId > 0)
                        {
                            var node = UmbracoContext.Current.ContentCache.GetById(errorPageNodeId);
                            if (node != null && node.Id != req.PublishedContent.Id)
                            {
                                req.SetRedirect(node.Url);
                            }
                            else
                            {
                                req.SetResponseStatus(403, "403 Forbidden");
                                req.PublishedContent = null;
                            }
                        }
                        else
                        {
                            req.SetResponseStatus(403, "403 Forbidden");
                            req.PublishedContent = null;
                        }
                    }
                }
            };

            // Ensure database table is created
            new IpFilterRepository().EnsureDatabaseTable();
        }
    }
}