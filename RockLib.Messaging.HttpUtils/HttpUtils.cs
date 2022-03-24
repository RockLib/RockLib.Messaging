using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace RockLib.Messaging
{
    internal static class HttpUtils
    {
        public static void AddHeader(HttpHeaders headers, string headerName, string headerValue)
        {
            if (headerValue == null)
                return;

            if (SupportsMultipleValues(headerName))
                headers.Add(headerName, SplitByComma(headerValue));
            else
                headers.Add(headerName, headerValue);
        }

        public static bool IsContentHeader(string headerName)
        {
            switch (headerName)
            {
                case "Allow":
                case "Content-Disposition":
                case "Content-Encoding":
                case "Content-Language":
                case "Content-Length":
                case "Content-Location":
                case "Content-MD5":
                case "Content-Range":
                case "Content-Type":
                case "Expires":
                case "Last-Modified":
                    return true;

                default:
                    return false;
            }
        }

        private static bool SupportsMultipleValues(string headerName)
        {
            switch (headerName)
            {
                case "Allow":
                case "Content-Encoding":
                case "Content-Language":
                case "Accept":
                case "Accept-Charset":
                case "Accept-Encoding":
                case "Accept-Language":
                case "Cache-Control":
                case "Connection":
                case "Expect":
                case "If-Match":
                case "If-None-Match":
                case "Pragma":
                case "TE":
                case "Trailer":
                case "Transfer-Encoding":
                case "Upgrade":
                case "Via":
                    return true;

                default:
                    return false;
            }
        }

        private static IEnumerable<string> SplitByComma(string headerValue)
        {
            string value;
#if NET48
            if (!headerValue.Contains(','))
#else
            if (!headerValue.Contains(',', StringComparison.InvariantCultureIgnoreCase))
#endif
            {
                value = headerValue.Trim();
                if (value.Length > 0)
                    yield return value;
                yield break;
            }

            var sb = new StringBuilder();

            foreach (var header in headerValue)
            {
                switch (header)
                {
                    case ',':
                        value = sb.ToString().Trim();
                        if (value.Length > 0)
                            yield return value;
                        sb.Clear();
                        continue;
                    default:
                        sb.Append(header);
                        continue;
                }
            }

            if (sb.Length > 0)
            {
                value = sb.ToString().Trim();
                if (value.Length > 0)
                    yield return value;
            }
        }
    }
}
