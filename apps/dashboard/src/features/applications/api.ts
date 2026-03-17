import type {Application, CreateApplicationRequest} from "./types.ts";
import {http} from "../../http.ts";

export async function getApplications(): Promise<Application[]> {
    return await http<Application[]>("/api/clients");
}

export async function getApplication(id: string): Promise<Application> {
    return await http<Application>(`/api/clients/${id}`);
}

export async function createApplication(dto: CreateApplicationRequest): Promise<Application> {
    return await http<Application>("/api/clients", {
        method: "POST",
        body: JSON.stringify(dto)
    });
}