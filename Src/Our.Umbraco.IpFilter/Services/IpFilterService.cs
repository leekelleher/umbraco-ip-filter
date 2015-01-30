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

        public bool IsIpProtected(int nodeId)
        {
            var node = UmbracoContext.Current.ContentCache.GetById(nodeId);
            return IsIpProtected(node);
        }

        public bool IsIpProtected(IPublishedContent node)
        {
            var pathIds = node.Path.Split(',').Select(int.Parse);

            return GetEnabledEntriesByNodeIds(pathIds).Any();
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

            // Apply blacklists
            if (entries.Any(x => x.Blacklist.Any()))
            {
                var ips = entries.SelectMany(x => x.Blacklist).Distinct();
                foreach (var ip in ips)
                {
                    var ipRegex = FormatIpAsRegex(ip);
                    if (Regex.IsMatch(ipAddress, ipRegex))
                    {
                        canAccess = false;
                        break;
                    }
                }
            }

            // Apply whitelists
            if (entries.Any(x => x.Whitelist.Any()))
            {
                // Whitelists override blacklists
                canAccess = false;

                var ips = entries.SelectMany(x => x.Whitelist).Distinct();
                foreach (var ip in ips)
                {
                    var ipRegex = FormatIpAsRegex(ip);
                    if (Regex.IsMatch(ipAddress, ipRegex))
                    {
                        canAccess = true;
                        break;
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
            return "^" + input.Replace(".", @"\.").Trim() + "$";
        }

        private CacheHelper Cache
        {
            get { return ApplicationContext.Current.ApplicationCache; }
        }
    }
}
