import {useMemo} from "react";
import {useApi} from "../../../apis/hooks/useApi.tsx";

export function useApiPermissionOptions(apiId?: string) {
    const { data, loading, error } = useApi(apiId);

    const options = useMemo(() => {
        if (!data) return [];

        return data.permissions.map((permission) => ({
            value: permission.scope,
            label: permission.scope,
            description: permission.description
        }));
    }, [data]);

    return { options, loading, error };
}
