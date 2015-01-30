using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Our.Umbraco.IpFilter.Data.Repositories;
using Our.Umbraco.IpFilter.Models;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Our.Umbraco.IpFilter.Services
{
    public class IpFilterService
    {
        private IpFilterRepository _ipFilterRepo;

        public IpFilterService()
        {
            _ipFilterRepo = new IpFilterRepository();
        }

        public bool IsIpProtected(int nodeId, bool recursive = true)
        {
            var node = UmbracoContext.Current.ContentCache.GetById(nodeId);
            return IsIpProtected(node, recursive);
        }

        public bool IsIpProtected(IPublishedContent node, bool recursive = true)
        {
            var pathIds = node.Path.Split(',').Select(int.Parse);
            var entries = GetEnabledEntriesByNodeIds(pathIds);

            return recursive 
                ? entries.Any()
                : entries.SingleOrDefault(x => x.NodeId == node.Id) != null;
        }

        public bool CanAccess(int nodeId, string ipAddress)
        {
            var errorPageNodeId = 0;
            return CanAccess(nodeId, ipAddress, out errorPageNodeId);
        }

        public bool CanAccess(IPublishedContent node, string ipAddress)
        {
            var errorPageNodeId = 0;
            return CanAccess(node, ipAddress, out errorPageNodeId);
        }

        public bool CanAccess(int nodeId, string ipAddress, out int errorPageNodeId)
        {
            var node = UmbracoContext.Current.ContentCache.GetById(nodeId);
            return CanAccess(node, ipAddress, out errorPageNodeId);
        }

        public bool CanAccess(IPublishedContent node, string ipAddress, out int errorPageNodeId)
        {
            var pathIds = node.Path.Split(',').Select(int.Parse).ToList();
            var entries = GetEnabledEntriesByNodeIds(pathIds)
                .OrderBy(x => pathIds.FindIndex(y => y == x.NodeId))
                .ToList();

            var canAccess = true;
            errorPageNodeId = 0;

            var lastList = "";
            var listChangeIndex = -1;

            for (var i = 0; i < entries.Count; i++)
            {
                var entry = entries[i];

                if (entry.Blacklist.Any())
                {
                    // If no IP filter has been hit yet, and given we are the
                    // blacklist, assume everyone has access unless blacklisted
                    if (lastList == "")
                    {
                        canAccess = true;
                        listChangeIndex = i;
                        lastList = "b";
                    }

                    // Lookup IP's between now and when we last changed list
                    var ips = entries.Skip(listChangeIndex)
                        .Take((i + 1) - listChangeIndex)
                        .SelectMany(x => x.Blacklist)
                        .Distinct();

                    // Check our IP against the list
                    foreach (var ip in ips)
                    {
                        var ipRegex = FormatIpAsRegex(ip);
                        if (Regex.IsMatch(ipAddress, ipRegex))
                        {
                            // Check to see if we have switched list type and if so
                            // track the list we are on now and when we changed
                            if (lastList != "b")
                            {
                                listChangeIndex = i;
                                lastList = "b";
                            }

                            canAccess = false;
                            break;
                        }
                    }
                }

                if (entry.Whitelist.Any())
                {
                    // If no IP filter has been hit yet, and given we are the
                    // whitelist, assume everyone is blocked unless whitelisted
                    if (lastList == "")
                    {
                        canAccess = false;
                        listChangeIndex = i;
                        lastList = "w";
                    }

                    // Lookup IP's between now and when we last changed list
                    var ips = entries.Skip(listChangeIndex)
                        .Take((i + 1) - listChangeIndex)
                        .SelectMany(x => x.Whitelist)
                        .Distinct();

                    // Check our IP against the list
                    foreach (var ip in ips)
                    {
                        var ipRegex = FormatIpAsRegex(ip);
                        if (Regex.IsMatch(ipAddress, ipRegex))
                        {
                            // Check to see if we have switched list type and if so
                            // track the list we are on now and when we changed
                            if (lastList != "w")
                            {
                                listChangeIndex = i;
                                lastList = "w";
                            }

                            canAccess = true;
                            break;
                        }
                    }

                }
            }

            // Find cloasest error page reference
            if (entries.Any(x => x.ErrorPageNodeId > 0))
            {
                errorPageNodeId = entries.Last(x => x.ErrorPageNodeId > 0).ErrorPageNodeId;
            }

            return canAccess;
        }

        public IpFilterEntry GetEntryByNodeId(int nodeId)
        {
            return _ipFilterRepo.GetByNodeId(nodeId);
        }

        public void SaveEntry(IpFilterEntry entry)
        {
            _ipFilterRepo.Save(entry);

            ClearCache();
        }

        public IEnumerable<IpFilterEntry> GetAllEnabledEntries()
        {
            return (IEnumerable<IpFilterEntry>)Cache.RuntimeCache.GetCacheItem("IpFilterService_GetAllEnabledEntries", () =>
            {
                return _ipFilterRepo.All().Where(x => x.Enabled);
            });
        }

        public IEnumerable<IpFilterEntry> GetEnabledEntriesByNodeIds(IEnumerable<int> ids)
        {
            return GetAllEnabledEntries().Where(x => ids.Contains(x.NodeId) && x.Enabled);
        }

        public void ClearCache()
        {
            Cache.RuntimeCache.ClearCacheItem("IpFilterService_GetAllEnabledEntries");
        }

        private static string FormatIpAsRegex(string input)
        {
            return "^" + input.Trim().TrimStart('^').TrimEnd('$') + "$";
        }

        private CacheHelper Cache
        {
            get { return ApplicationContext.Current.ApplicationCache; }
        }
    }
}
