import type {Application} from "../types.ts";
import {useCallback} from "react";
import {getApplication} from "../api.ts";
import {type QueryResult, useQuery} from "../../../shared/hooks/useQuery.tsx";

export function useApplication(id?: string): QueryResult<Application | null> {
    const query = useCallback(() => {
        if (!id) throw new Error("id is required");
        return getApplication(id!)
    }, [id]);

    return useQuery<Application | null>({queryFn: query, options: { enabled: !!id } });
}