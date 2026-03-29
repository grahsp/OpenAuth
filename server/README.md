
# OpenAuth Authorization Server
  
The OpenAuth Authorization Server is an ASP.NET Core application responsible for handling OAuth2 / OpenID Connect flows and issuing access tokens.  
  
It provides endpoints for authorization, token issuance, and token validation, as well as management APIs used by the dashboard.

# Responsibilities  
  
The authorization server handles several key responsibilities:  
  
- OAuth2 / OpenID Connect flows  
- Token issuance  
- Token signing and validation  
- Management of clients, API resources, and scopes  
- Exposing discovery and JWKS endpoints  
  
Key endpoints include:
* `/connect/authorize`
* `/connect/token`
* `/jwks`
* `/.well-known/openid-configuration`

# Running the Server Locally  
  
## Requirements  
  
- .NET SDK 9+  
- SQL Server (or Docker)  

  
## 1. Configure environment variables  
  
The server uses environment variables for configuration.  
  
Example:  
  
```bash  
ConnectionStrings__DefaultConnection="Server=localhost;Database=OpenAuth;User Id=sa;Password=YourPassword;TrustServerCertificate=True"
Admin__Password=admin
Database__Initialize=true
Seed__Demo=true
```

## Database Initialization  
  
The authorization server requires a database schema to be created before it can run.  
  
This can be done in one of two ways.  
  
### Option 1 — Run migrations manually (recommended for development)  
  
Install the EF Core CLI tools if you don't already have them:  
  
```bash  
dotnet tool install --global dotnet-ef
```

Then apply the migrations:

```bash
cd apps/server  
dotnet ef database update
```

### Option 2 — Automatic initialization

You can enable automatic database initialization by setting the following environment variable:

```bash
Database__Initialize=true
```

When this flag is enabled, the server will automatically apply migrations on startup.

## 2. Run the Server

```bash
cd ./server  
dotnet run
```

The server will start on: `http://localhost:8080`
