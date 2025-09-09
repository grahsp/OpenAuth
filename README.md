# Self-Hosted Authentication Server

A minimal self-hosted authentication server built with **.NET**.  
It provides client registration, secret management, token issuance, and JWKS publication — everything needed to secure your APIs and apps using the **OAuth2 Client Credentials** flow.


## ✨ Features

- **Client Management**
  - Register new clients
  - Fetch client details by ID or name
  - Delete clients

- **Client Secret Lifecycle**
  - Create secrets (with configurable expiration)
  - List all secrets for a client
  - Revoke secrets (mark inactive)
  - Delete secrets (remove permanently)

- **Token Issuance**
  - `/connect/token` endpoint issues signed JWT access tokens
  - Supports **Client Credentials** flow

- **JWKS (JSON Web Key Set)**
  - `/jwks` endpoint exposes public signing keys
  - Enables downstream APIs to validate issued tokens


## 🏗️ Why?

This project started as an exploration of building an **OAuth2-style authentication service from scratch** in .NET.  
The goals are:
- Learn how to model clients and secrets in a clean domain-driven way
- Implement secure secret rotation and revocation
- Understand JWT signing and validation
- Provide a **self-contained, self-hosted** alternative to full-fledged identity servers
