import {useCallback} from "react";
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