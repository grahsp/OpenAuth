import {useCallback} from "react";
import {type QueryResult, useQuery} from "../../../../shared/hooks/useQuery.tsx";
import {getApplications, getClientApiAccess} from "../../../applications/api.ts";

type ApiClientPermission = {
    clientId: string;
    name: string;
    authorized: boolean;
    selectedScopes: string[];
}

export function useApiClientPermissions(apiId?: string): QueryResult<ApiClientPermission[]> {
    const query = useCallback(async () => {
        if (!apiId) {
            throw new Error("apiId is required");
        }

        const applications = await getApplications();

        const clientPermissions = await Promise.all(
            applications.map(async (application) => {
                const access = await getClientApiAccess(application.id);
                const match = access.find((entry) => entry.id === apiId);

                return {
                    clientId: application.id,
                    name: application.name,
                    authorized: !!match,
                    selectedScopes: match?.scopes ?? []
                };
            })
        );

        return clientPermissions;
    }, [apiId]);

    return useQuery({ queryFn: query, options: { enabled: !!apiId } });
}
