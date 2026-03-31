import type {AddApiPermissionsRequest, Api, RemoveApiPermissionsRequest} from "./types.ts";
import {http} from "../../http.ts";

export async function getApis(): Promise<Api[]> {
    return await http<Api[]>("/api/apis");
}

export async function getApi(id: string): Promise<Api> {
    return await http<Api>(`/api/apis/${id}`);
}

export async function removeApiPermissions(request: RemoveApiPermissionsRequest): Promise<void> {
    const { apiId, scopes } = request;

    await http<void>(`/api/apis/${apiId}/permissions`, {
        method: "DELETE",
        body: JSON.stringify({ scopes })
    });
}

export async function addApiPermissions(request: AddApiPermissionsRequest): Promise<void> {
    const { apiId, permissions } = request;

    await http<void>(`/api/apis/${apiId}/permissions`, {
        method: "POST",
        body: JSON.stringify({ permissions })
    });
}
