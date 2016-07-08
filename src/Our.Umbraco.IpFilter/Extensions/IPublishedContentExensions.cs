using Our.Umbraco.IpFilter.Services;
using Umbraco.Core.Models;

namespace Our.Umbraco.IpFilter.Extensions
{
    public static class IPublishedContentExensions
    {
        public static bool IsIpProtected(this IPublishedContent content)
        {
            return new IpFilterService().IsIpProtected(content);
        }

        public static bool CanAccess(this IPublishedContent content, string ipAddress)
        {
            return new IpFilterService().CanAccess(content, ipAddress);
        }

        public static bool CanAccess(this IPublishedContent content, string ipAddress, out int errorPageNodeId)
        {
            return new IpFilterService().CanAccess(content, ipAddress, out errorPageNodeId);
        }
    }
}