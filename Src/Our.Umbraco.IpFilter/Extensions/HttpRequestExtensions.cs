using System.Linq;
using System.Web;
using Umbraco.Core;

namespace Our.Umbraco.IpFilter.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIpAddress(this HttpRequest request)
        {
            // check the querystring for a forced IP address (for testing purposes)
            var ipAddress = HttpContext.Current.IsDebuggingEnabled
                ? request.QueryString["ip"]
                : null;

            // Radware Cloud CDN
            if (string.IsNullOrWhiteSpace(ipAddress) && request.ServerVariables.AllKeys.InvariantContains("HTTP_X_RDWR_IP"))
                ipAddress = request.ServerVariables["HTTP_X_RDWR_IP"];

            // Akamai CDN
            if (string.IsNullOrWhiteSpace(ipAddress) && request.ServerVariables.AllKeys.InvariantContains("HTTP_TRUE_CLIENT_IP"))
                ipAddress = request.ServerVariables["HTTP_TRUE_CLIENT_IP"];

            // Level3 CDN
            if (string.IsNullOrWhiteSpace(ipAddress) && request.ServerVariables.AllKeys.InvariantContains("HTTP_X_TRUE_CLIENT_IP"))
                ipAddress = request.ServerVariables["HTTP_X_TRUE_CLIENT_IP"];

            // check the "HTTP_X_FORWARDED_FOR" value, splits the list by comma, then take the first value
            if (string.IsNullOrWhiteSpace(ipAddress))
            {
                var forwarded = request.ServerVariables["HTTP_X_FORWARDED_FOR"];
                if (!string.IsNullOrEmpty(forwarded))
                    ipAddress = forwarded.ToDelimitedList().FirstOrDefault();
            }

            // gets the fallback value (from "REMOTE_ADDR")
            if (string.IsNullOrWhiteSpace(ipAddress))
                ipAddress = request.ServerVariables["REMOTE_ADDR"];

            // checks if the IP address is IPv6 localhost
            if (ipAddress != null && ipAddress.Contains("::1"))
                ipAddress = ipAddress.Replace("::1", "127.0.0.1");

            // checks if the IP address contains a port number, then splits it by colon, taking the first value (IP address)
            if (ipAddress != null && ipAddress.Contains(":"))
                ipAddress = ipAddress.ToDelimitedList(":").FirstOrDefault();

            return ipAddress;
        }
    }
}
