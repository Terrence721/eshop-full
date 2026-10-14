using System.Text.Json.Serialization;

namespace eShop.ClientApp.Models.Token;

public class UserToken
{
    [JsonPropertyName("id_token")] public string IdToken { get; set; } = null!;

    [JsonPropertyName("access_token")] public string AccessToken { get; set; } = null!;

    [JsonPropertyName("refresh_token")]
    public string RefreshToken { get; set; } = null!;

    [JsonPropertyName("expires_at")]
    public DateTimeOffset ExpiresAt { get; set; }
}
