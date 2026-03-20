import {useCallback, useMemo} from "react";
import {getApiDetails, getApiSummaries} from "./api.ts";
import type {ApiDetails, ApiSummary} from "./types.ts";
import {type QueryResult, useQuery} from "../../shared/hooks/useQuery.tsx";

export function useApiSummaries(): QueryResult<ApiSummary[]> {
    return useQuery({ queryFn: getApiSummaries });
}

export function useApiDetails(id?: string): QueryResult<ApiDetails | null> {
    const query = useCallback(() => {
        if (!id) throw new Error("id is required");
        return getApiDetails(id!)
    }, [id])

    return useQuery({ queryFn: query, options: { enabled: !!id } });
}

export function useApiPermissionOptions(apiId?: string) {
    const { data, loading, error } = useApiDetails(apiId);

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
