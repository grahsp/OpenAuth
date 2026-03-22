export type ApplicationType = "m2m" | "spa" | "web";

export type Application = {
    id: string;
    name: string;
    applicationType: ApplicationType;
    redirectUris: string[];
    tokenLifetimeInSeconds: number;
    allowedGrantTypes: string[];
    availableGrantTypes: string[];
}

export type CreateApplicationRequest = {
    type: ApplicationType;
    name: string;
    apiId?: string;
    scopes: string[] | null;
}

export type CreateApplicationResponse = {
    id: string;
    secret?: string;
}

export type ClientApiAccess = {
    id: string;
    scopes: string[];
}

export type UpdateApplicationConfigurationRequest = {
    name: string;
    applicationType: string;
    redirectUris: string[];
    tokenLifetimeInSeconds: number;
    allowedGrantTypes: string[];
}

export type Api = {
    id: string;
    name: string;
    audience: string;
    permissions: Permission[];
}

export type Permission = {
    scope: string;
    description?: string;
}