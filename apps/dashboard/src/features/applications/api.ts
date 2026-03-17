import type {Application} from "./types.ts";
import {http} from "../../http.ts";

export async function getApplications() {
    return await http<Application[]>("/api/clients");
}

export async function getApplication(id: string) {
    return await http<Application>(`/api/clients/${id}`);
}