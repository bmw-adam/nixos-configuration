using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace TpvVyber.Client.Classes.Client;

public class Attributes
{
    [JsonPropertyName("realm_client")]
    public string RealmClient { get; set; }

    [JsonPropertyName("oidc.ciba.grant.enabled")]
    public string OidcCibaGrantEnabled { get; set; }

    [JsonPropertyName("client.secret.creation.time")]
    public string ClientSecretCreationTime { get; set; }

    [JsonPropertyName("backchannel.logout.session.required")]
    public string BackchannelLogoutSessionRequired { get; set; }

    [JsonPropertyName("standard.token.exchange.enabled")]
    public string StandardTokenExchangeEnabled { get; set; }

    [JsonPropertyName("oauth2.device.authorization.grant.enabled")]
    public string Oauth2DeviceAuthorizationGrantEnabled { get; set; }

    [JsonPropertyName("backchannel.logout.revoke.offline.tokens")]
    public string BackchannelLogoutRevokeOfflineTokens { get; set; }
}
