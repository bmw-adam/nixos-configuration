using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TpvVyber.Client.Classes.Client;

public class Attributes
{
    [JsonPropertyName("realm_client")]
    public string RealmClient { get; set; } = string.Empty;

    [JsonPropertyName("oidc.ciba.grant.enabled")]
    public string OidcCibaGrantEnabled { get; set; } = string.Empty;

    [JsonPropertyName("client.secret.creation.time")]
    public string ClientSecretCreationTime { get; set; } = string.Empty;

    [JsonPropertyName("backchannel.logout.session.required")]
    public string BackchannelLogoutSessionRequired { get; set; } = string.Empty;

    [JsonPropertyName("standard.token.exchange.enabled")]
    public string StandardTokenExchangeEnabled { get; set; } = string.Empty;

    [JsonPropertyName("oauth2.device.authorization.grant.enabled")]
    public string Oauth2DeviceAuthorizationGrantEnabled { get; set; } = string.Empty;

    [JsonPropertyName("backchannel.logout.revoke.offline.tokens")]
    public string BackchannelLogoutRevokeOfflineTokens { get; set; } = string.Empty;
}
