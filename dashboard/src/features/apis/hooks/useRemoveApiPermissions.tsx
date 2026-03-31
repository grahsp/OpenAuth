import {useState} from "react";
import type {RemoveApiPermissionsRequest} from "../types.ts";
import {removeApiPermissions} from "../api.ts";
import {ApiError} from "../../../http.ts";

export function useRemoveApiPermissions() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const remove = async (request: RemoveApiPermissionsRequest) => {
        setLoading(true);
        setError(null);

        try {
            await removeApiPermissions(request);
            return true;
        } catch (err) {
            if (err instanceof ApiError)
                setError(err.message);
            else
                setError("Failed to remove API permissions.");

            return false;
        } finally {
            setLoading(false);
        }
    };

    return { remove, loading, error };
}
