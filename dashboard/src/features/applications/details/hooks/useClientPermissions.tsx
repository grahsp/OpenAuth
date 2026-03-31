import {useClientApiAccess} from "./useClientApiAccess.tsx";
import {useMemo} from "react";
import {useApis} from "../../../apis/hooks/useApis.tsx";

export function useClientPermissions(clientId?: string) {
    const apis = useApis();
    const access = useClientApiAccess(clientId);

    const data = useMemo(() => {
        const apisData = apis.data;
        const accessData = access.data

        if (!apisData || !accessData)
            return [];

        return apisData.map(api => {
            const match = accessData.find(a => a.id === api.id);

            return {
                apiId: api.id,
                name: api.name,
                authorized: !!match,
                availableScopes: api.permissions.map(p => ({
                    label: p.scope,
                    value: p.scope,
                    description: p.description
                })),
                selectedScopes: match?.scopes ?? []
            }
        })
    }, [apis.data, access.data]);

    return {
        data,
        loading: apis.loading || access.loading,
        error: apis.error || access.error,
        refresh: access.refresh
    };
}
