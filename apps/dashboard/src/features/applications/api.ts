import type {
    Api,
    Application, ClientApiAccess,
    CreateApplicationRequest,
    CreateApplicationResponse,
    UpdateApplicationConfigurationRequest
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

export async function getApis(): Promise<Api[]> {
    return await http<Api[]>("/api/apis");
}

export async function getApi(id: string): Promise<Api> {
    return await http<Api>(`/api/apis/${id}`);
}