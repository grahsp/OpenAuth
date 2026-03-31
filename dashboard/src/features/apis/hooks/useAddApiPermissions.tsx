import {useState} from "react";
import type {AddApiPermissionsRequest} from "../types.ts";
import {addApiPermissions} from "../api.ts";
import {ApiError} from "../../../http.ts";

export function useAddApiPermissions() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const add = async (request: AddApiPermissionsRequest) => {
        setLoading(true);
        setError(null);

        try {
            await addApiPermissions(request);
            return true;
        } catch (err) {
            if (err instanceof ApiError)
                setError(err.message);
            else
                setError("Failed to add API permissions.");

            return false;
        } finally {
            setLoading(false);
        }
    };

    return { add, loading, error };
}
