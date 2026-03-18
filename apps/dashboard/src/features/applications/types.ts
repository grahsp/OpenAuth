export type ApplicationType = "m2m" | "spa" | "web";

export type Application = {
    id: string;
    name: string;
}

export type CreateApplicationRequest = {
    type: ApplicationType;
    name: string;
    apiId: string | null;
    scopes: string[] | null;
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