
# OpenAuth Dashboard

The OpenAuth Dashboard is a React application used to manage the configuration of the OpenAuth authorization server.

It provides a user interface for creating and managing applications (clients), API resources, and permissions.

The dashboard communicates with the authorization server's management APIs to configure how tokens are issued and what access clients are allowed.

# Responsibilities

The dashboard provides functionality for managing authorization configuration, including:

- Creating and managing applications (OAuth clients)
- Registering API resources
- Defining permissions (scopes)
- Configuring which clients can access which APIs
- Assigning allowed scopes per client

The dashboard interacts with the authorization server through its management endpoints.

# Running the Dashboard Locally

## Requirements

- Node.js 18+
- npm

The dashboard expects the OpenAuth authorization server to be running.

## 1. Install dependencies

```bash
cd ./dashboard
npm install
```

## 2. Configure environment variables

The application uses a Vite development proxy.  
The host and port of the authorization server must be configured through environment variables.

- `VITE_SERVER_HOST` — Host of the OpenAuth authorization server  
- `VITE_SERVER_PORT` — Port of the OpenAuth authorization server

## 3. Run the dashboard

```bash
npm run dev
```

The dashboard will start on: `http://localhost:5173`
