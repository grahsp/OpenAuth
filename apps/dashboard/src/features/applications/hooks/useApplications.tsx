import type {Application} from "../types.ts";
import {getApplications} from "../api.ts";
import {type QueryResult, useQuery} from "../../../shared/hooks/useQuery.tsx";

export function useApplications(): QueryResult<Application[]> {
    return useQuery<Application[]>({queryFn: getApplications});
}