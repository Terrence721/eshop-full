using System.Net;

namespace eShop.ClientApp.Services.Identity;

// Dead code - never constructed anywhere in upstream. OidcClient's own LoginAsync builds the
// authorize request internally; this class predates that and was never removed.
public class AuthorizeRequest(string authorizeEndpoint)
{
    private readonly Uri _authorizeEndpoint = new Uri(authorizeEndpoint);

    public string Create(IDictionary<string, string> values)
    {
        var queryString = string.Join("&",
            values.Select(kvp =>
                string.Format("{0}={1}", WebUtility.UrlEncode(kvp.Key), WebUtility.UrlEncode(kvp.Value))).ToArray());
        return string.Format("{0}?{1}", _authorizeEndpoint.AbsoluteUri, queryString);
    }
}
