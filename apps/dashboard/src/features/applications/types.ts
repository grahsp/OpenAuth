export type ApplicationType = "m2m" | "spa" | "web";

export type Application = {
    id: string;
    name: string;
    applicationType: string;
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

export type ApiSummary = {
    id: string;
    name: string;
    audience: string;
}

export type ApiDetails = {
    id: string;
    name: string;
    audience: string;
    permissions: ApiPermission[];
}

export type ApiPermission = {
    scope: string;
    description: string | null;
}