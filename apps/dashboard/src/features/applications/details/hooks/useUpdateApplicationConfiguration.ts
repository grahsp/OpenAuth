import {useState} from "react";
import type {UpdateApplicationConfigurationRequest} from "../../types.ts";
import {updateApplicationConfiguration} from "../../api.ts";
import {ApiError} from "../../../../http.ts";

export function useUpdateApplicationConfiguration() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const update = async (id: string, request: UpdateApplicationConfigurationRequest) => {
        setLoading(true);
        setError(null);

        try {
            await updateApplicationConfiguration(id, request);
            return true;
        } catch (err) {
            if (err instanceof ApiError)
                setError(err.message);
            else
                setError("Failed to update application");

            return false;
        } finally {
            setLoading(false);
        }
    };

    return { update, loading, error };
}