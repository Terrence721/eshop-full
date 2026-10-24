namespace eShop.ClientApp.Helpers
{
    public static class UriHelper
    {
        private static readonly char[] _trims = { '/' };

        public static string CombineUri(params string[] uriParts)
        {
            if (uriParts is null || uriParts.Length == 0)
            {
                return string.Empty;
            }

            var parts = uriParts
                .Select(part => (part ?? string.Empty).Trim(_trims))
                .Where(part => part.Length > 0)
                .ToArray();

            if (parts.Length == 0)
            {
                return string.Empty;
            }

            var uri = parts[0];
            for (var i = 1; i < parts.Length; i++)
            {
                uri = $"{uri}/{parts[i]}";
            }

            return uri;
        }
    }
}
