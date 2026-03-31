import type {Api} from "../types.ts";
import {getApis} from "../api.ts";
import {type QueryResult, useQuery} from "../../../shared/hooks/useQuery.tsx";

export function useApis(): QueryResult<Api[]> {
    return useQuery<Api[]>({ queryFn: getApis });
}
