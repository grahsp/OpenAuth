export type Application = {
    id: string;
    name: string;
}

export type CreateApplicationRequest = {
    type: ApplicationType;
    name: string;
    redirectUris: string[];
}

export type ApplicationType = "spa" | "m2m";