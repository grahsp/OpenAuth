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
