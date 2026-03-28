import {useCallback, useEffect, useState} from "react";
import {ApiError} from "../../http.ts";

export type Query<T> = {
    queryFn: () => Promise<T>,
    options?: QueryOptions,
}

export type QueryOptions = {
    enabled?: boolean,
}

export type QueryResult<T> = {
    data: T | null,
    loading: boolean,
    error: string | null,
    refresh: () => Promise<void>
};

export function useQuery<T>({queryFn, options: {enabled = true} = {}}: Query<T>): QueryResult<T> {
    const [data, setData] = useState<T | null>(null);
    const [loading, setLoading] = useState<boolean>(false);
    const [error, setError] = useState<string | null>(null);

    const query = useCallback(async () => {
        setLoading(true);
        setError(null);

        try {
            setData(await queryFn());
        } catch (err) {
            if (err instanceof ApiError)
                setError(err.message);
            else
                setError("Failed to fetch data.")
        } finally {
            setLoading(false);
        }
    }, [queryFn]);

    useEffect(() => {
        if (enabled) void query();
    }, [query, enabled])

    return {data, loading, error, refresh: query}
}