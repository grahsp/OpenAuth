import {useCallback, useMemo} from "react";
import {getApi, getApis} from "./api.ts";
import type {Api} from "./types.ts";
import {type QueryResult, useQuery} from "../../shared/hooks/useQuery.tsx";

export function useApis(): QueryResult<Api[]> {
    return useQuery({ queryFn: getApis });
}

export function useApi(id?: string): QueryResult<Api | null> {
    const query = useCallback(() => {
        if (!id)
            throw new Error("id is required");

        return getApi(id!)
    }, [id])

    return useQuery({ queryFn: query, options: { enabled: !!id } });
}

export function useApiPermissionOptions(apiId?: string) {
    const { data, loading, error } = useApi(apiId);

    const options = useMemo(() => {
        if (!data) return [];

        return data.permissions.map(p => ({
            value: p.scope,
            label: p.scope,
            description: p.description
        }));
    }, [data]);

    return { options, loading, error };
}
