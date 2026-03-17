export type Application = {
    id: string;
    name: string;
}

export type CreateApplicationRequest = {
    type: "m2m" | "spa";
    name: string;
    redirectUris: string[];
}