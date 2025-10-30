using OpenAuth.Application.OAuth.Authorization.Dtos;
using OpenAuth.Application.OAuth.Authorization.Interfaces;
using OpenAuth.Application.Shared.Models;

namespace OpenAuth.Application.OAuth.Authorization.Factories;

public interface IAuthorizationRequestFactory
{
    Result<AuthorizationRequest> CreateFrom(IAuthorizeQuery query);
}