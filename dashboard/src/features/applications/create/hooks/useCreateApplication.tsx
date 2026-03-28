import {useState} from "react";
import type {CreateApplicationRequest} from "../../types.ts";
import {createApplication} from "../../api.ts";
import {ApiError} from "../../../../http.ts";

export function useCreateApplication() {
    const [loading, setLoading] = useState(false);
    const [error, setError] = useState<string | null>(null);

    const create = async (dto: CreateApplicationRequest) => {
        setLoading(true);
        setError(null);

        try {
            return await createApplication(dto);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to create application.");
            }

            throw err;
        } finally {
            setLoading(false);
        }
    };

    return {create, loading, error};
}