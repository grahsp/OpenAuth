import {useEffect, useState} from "react";
import {createApplication, getApplication, getApplications} from "./api.ts";
import type {Application, CreateApplicationRequest} from "./types.ts";
import {ApiError} from "../../http.ts";

export function useApplications() {
    const [data, setData] = useState<Application[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchApplications = async () => {
        setLoading(true);
        setError(null);

        try {
            const result = await getApplications();
            setData(result);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to fetch applications.")
            }
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchApplications()
    }, []);

    return { data, loading, error, reload: fetchApplications };
}

export function useApplication(id: string | undefined) {
    const [data, setData] = useState<Application | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchApplication = async (id: string) => {
        setLoading(true);
        setError(null);

        try {
            const result = await getApplication(id);
            setData(result);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to fetch application.");
            }
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        if (!id) {
            setLoading(false);
            return;
        }

        fetchApplication(id);
    }, [id]);

    return { data, loading, error, reload: fetchApplication };
}