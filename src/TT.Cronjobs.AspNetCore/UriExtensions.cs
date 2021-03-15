using System;

namespace TT.Cronjobs.AspNetCore
{
    public static class UriExtensions
    {
        public static Uri WithTrailingSlash(this Uri uri)
        {
            return new Uri(uri.ToString().TrimEnd('/') + '/');
        }
    }
}