using System.Web;

namespace Our.Umbraco.IpFilter.Extensions
{
    public static class HttpRequestExtensions
    {
        public static string GetClientIpAddress(this HttpRequest req)
        {
            var ipAddress = req.ServerVariables["HTTP_X_FORWARDED_FOR"];

            if (!string.IsNullOrEmpty(ipAddress))
            {
                var addresses = ipAddress.Split(',');
                if (addresses.Length != 0)
                {
                    return addresses[0];
                }
            }

            return req.ServerVariables["REMOTE_ADDR"];
        }
    }
}
