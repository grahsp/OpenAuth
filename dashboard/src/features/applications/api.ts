import type {
    Application, ClientApiAccess,
    CreateApplicationRequest,
    CreateApplicationResponse,
    UpdateApplicationConfigurationRequest, UpdateClientApiAccessRequest
} from "./types.ts";
import {http} from "../../http.ts";

export async function getApplications(): Promise<Application[]> {
    return await http<Application[]>("/api/clients");
}

export async function getApplication(id: string): Promise<Application> {
    return await http<Application>(`/api/clients/${id}`);
}

export async function getClientApiAccess(id: string): Promise<ClientApiAccess[]> {
    return await http<ClientApiAccess[]>(`/api/clients/${id}/apis/access`);
}

export async function setClientApiAccess(request: UpdateClientApiAccessRequest): Promise<void> {
    const { clientId, apiId, scopes } = request;

    return await http<void>(`/api/clients/${clientId}/apis/${apiId}`, {
        method: "PUT",
        body: JSON.stringify({ scopes } )
    })
}

export async function createApplication(request: CreateApplicationRequest): Promise<CreateApplicationResponse> {
    return await http<CreateApplicationResponse>("/api/clients", {
        method: "POST",
        body: JSON.stringify(request)
    });
}

export async function updateApplicationConfiguration(id: string, request: UpdateApplicationConfigurationRequest): Promise<void> {
    await http<void>(`/api/clients/${id}/configuration`, {
        method: "PUT",
        body: JSON.stringify(request)
    })
}
