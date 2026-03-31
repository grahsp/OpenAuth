import type {Api} from "./types.ts";
import {http} from "../../http.ts";

export async function getApis(): Promise<Api[]> {
    return await http<Api[]>("/api/apis");
}

export async function getApi(id: string): Promise<Api> {
    return await http<Api>(`/api/apis/${id}`);
}
