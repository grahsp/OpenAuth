namespace OpenAuth.Api.Connect.Authorize;

public static class AuthorizeValidator
{
    public static bool Validate(this AuthorizeRequest dto, out string? error, out string? description)
    {
        error = null;
        description = null;

        if (string.IsNullOrWhiteSpace(dto.ClientId))
        {
            error = "invalid_request";
            description = "Client id is required.";
            return false;
        }

        if (string.IsNullOrWhiteSpace(dto.RedirectUri))
        {
            error = "invalid_request";
            description = "Redirect uri is required.";
            return false;
        }

        if (!Uri.TryCreate(dto.RedirectUri, UriKind.Absolute, out _))
        {
            error = "invalid_request";
            description = "Redirect uri is invalid.";
            return false;
        }

        return true;
    }
}