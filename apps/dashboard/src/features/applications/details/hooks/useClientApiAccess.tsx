import {useCallback} from "react";
import {getClientApiAccess} from "../../api.ts";
import {useQuery} from "../../../../shared/hooks/useQuery.tsx";

export function useClientApiAccess(clientId?: string) {
    const query = useCallback(() => {
        if (!clientId)
            throw new Error("id is required");

        return getClientApiAccess(clientId!)
    }, [clientId])

    return useQuery({queryFn: query, options: {enabled: !!clientId}});
}