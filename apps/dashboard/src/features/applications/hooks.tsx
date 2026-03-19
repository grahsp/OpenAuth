import {useEffect, useState} from "react";
import {createApplication, getApiDetails, getApiSummaries, getApplication, getApplications} from "./api.ts";
import type {ApiDetails, ApiSummary, Application, CreateApplicationRequest} from "./types.ts";
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

    return { create: create, loading, error };
}

export function useApiSummaries() {
    const [data, setData] = useState<ApiSummary[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchApis = async () => {
        setLoading(true);
        setError(null);

        try {
            const result = await getApiSummaries();
            setData(result);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to fetch APIs.")
            }
        } finally {
            setLoading(false);
        }
    }

    useEffect(() => {
        fetchApis();
    }, []);

    return { data, loading, error, reload: fetchApis };
}

export function useApiDetails(id?: string) {
    const [data, setData] = useState<ApiDetails>();
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    const fetchApi = async (id: string) => {
        setLoading(true);
        setError(null);

        try {
            const result = await getApiDetails(id);
            setData(result);
        } catch (err) {
            if (err instanceof ApiError) {
                setError(err.message);
            } else {
                setError("Failed to fetch API.");
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

        fetchApi(id);
    }, [id]);

    return { data, loading, error, reload: fetchApi };
}