import {config} from "./lib/config.ts";

export async function http<T>(path: string, options?: RequestInit): Promise<T> {
    const res = await fetch(`${config.apiBaseUrl}${path}`, {
        credentials: "include",
        headers: {
            "Content-Type": "application/json",
            ...options?.headers
        },
        ...options
    });

    // Authentication expired
    if (res.status === 401) {
        const returnUrl = encodeURIComponent(window.location.pathname);
        window.location.href = `/account/login?ReturnUrl=${returnUrl}`;

        throw new Error("Unauthorized");
    }

    let body: unknown;

    try {
        body = await res.json();
    } catch {
        // No JSON body
    }

    if (!res.ok) {
        const errorBody = body as { message?: string; code?: string };

        const code = errorBody?.code;
        const message = errorBody?.message ?? "Something went wrong. Try again later.";

        throw new ApiError(message, code);
    }

    return body as T;
}

export class ApiError extends Error {
    code?: string;

    constructor(message: string, code?: string) {
        super(message);
        this.code = code;
    }
}