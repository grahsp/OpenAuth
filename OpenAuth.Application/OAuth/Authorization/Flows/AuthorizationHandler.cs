using OpenAuth.Application.Clients.Interfaces;

namespace OpenAuth.Application.OAuth.Authorization.Flows;

public class AuthorizationHandler : IAuthorizationHandler
{
    private readonly IClientQueryService _clientQueryService;
    private readonly TimeProvider _time;
    
    public AuthorizationHandler(IClientQueryService clientQueryService, TimeProvider time)
    {
        _clientQueryService = clientQueryService;
        _time = time;
    }
    
    public async Task<AuthorizationResponse> AuthorizeAsync(AuthorizationRequest request)
    {
        throw new NotImplementedException();
    }
}