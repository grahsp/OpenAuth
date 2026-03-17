import {config} from "./lib/config.ts";

export async function http<T>(path: string, options?: RequestInit): Promise<T> {
    const res = await fetch(`${config.apiBaseUrl}${path}`, options);

    if (!res.ok) {
        throw new Error(`HTTP error: ${res.status}`);
    }

    return res.json();
}