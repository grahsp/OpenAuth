namespace OpenAuth.Api.Dtos;

public record TokenErrorResponse(string Error, string ErrorDescription, string ErrorUri);