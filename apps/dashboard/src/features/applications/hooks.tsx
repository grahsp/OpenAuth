import {useEffect, useState} from "react";
import {getApplication, getApplications} from "./api.ts";
import type {Application} from "./types.ts";

export function useApplications() {
    const [data, setData] = useState<Application[]>([]);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        let isMounted = true;

        getApplications()
            .then(result => {
                if (isMounted) setData(result);
            })
            .catch(() => setError("Failed to load applications"))
            .finally(() => {
                if (isMounted) setLoading(false);
            });

        return () => {
            isMounted = false;
        };
    }, []);

    return { data, loading, error };
}

export function useApplication(id: string | undefined) {
    const [data, setData] = useState<Application | null>(null);
    const [loading, setLoading] = useState(true);
    const [error, setError] = useState<string | null>(null);

    useEffect(() => {
        if (!id) return;

        let isMounted = true;

        getApplication(id)
            .then(result => {
                if (isMounted) setData(result);
            })
            .catch(() => {
                if (isMounted) setError("Failed to load application");
            })
            .finally(() => {
                if (isMounted) setLoading(false);
            });

        return () => {
            isMounted = false;
        };
    }, [id]);

    return { data, loading, error };
}