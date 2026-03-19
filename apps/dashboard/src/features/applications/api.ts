import type {
    ApiDetails,
    ApiSummary,
    Application,
    CreateApplicationRequest,
    CreateApplicationResponse
} from "./types.ts";
import {http} from "../../http.ts";

export async function getApplications(): Promise<Application[]> {
    return await http<Application[]>("/api/clients");
}

export async function getApplication(id: string): Promise<Application> {
    return await http<Application>(`/api/clients/${id}`);
}

export async function createApplication(dto: CreateApplicationRequest): Promise<CreateApplicationResponse> {
    return await http<CreateApplicationResponse>("/api/clients", {
        method: "POST",
        body: JSON.stringify(dto)
    });
}

export async function getApiSummaries(): Promise<ApiSummary[]> {
    return await http<ApiSummary[]>("/api/apis");
}

export async function getApiDetails(id: string): Promise<ApiDetails> {
    return await http<ApiDetails>(`/api/apis/${id}`);
}