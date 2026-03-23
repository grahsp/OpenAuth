import {useState} from "react";
import type {UpdateClientApiAccessRequest} from "../../types.ts";
import {setClientApiAccess} from "../../api.ts";
import {ApiError} from "../../../../http.ts";

export function useSetClientApiAccess() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const update = async (dto: UpdateClientApiAccessRequest) => {
        setLoading(true);
        setError(null);

        try {
            return await setClientApiAccess(dto);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to update client-api access.");
            }

            throw err;
        } finally {
            setLoading(false);
        }
    };

    return {update, loading, error};
}