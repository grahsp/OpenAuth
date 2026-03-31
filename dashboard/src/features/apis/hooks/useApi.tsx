import {useCallback} from "react";
import type {Api} from "../types.ts";
import {getApi} from "../api.ts";
import {type QueryResult, useQuery} from "../../../shared/hooks/useQuery.tsx";

export function useApi(id?: string): QueryResult<Api> {
    const query = useCallback(() => {
        if (!id) throw new Error("id is required");
        return getApi(id);
    }, [id]);

    return useQuery<Api>({ queryFn: query, options: { enabled: !!id } });
}
