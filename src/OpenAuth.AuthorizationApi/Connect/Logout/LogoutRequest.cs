using Microsoft.AspNetCore.Mvc;

namespace OpenAuth.AuthorizationApi.Connect.Logout;

public record LogoutRequest(
    [FromQuery(Name = "post_logout_redirect_uri")]
    string? RedirectUri,
    
    [FromQuery(Name = "state")]
    string? State
);