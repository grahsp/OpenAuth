import {Outlet, useParams} from "react-router-dom";
import {useApi} from "../../../applications/hooks.tsx";
import {DetailTabs} from "../../../../shared/components/DetailTabs.tsx";
import {apiTabs} from "../../routes.tsx";

export default function ApiLayout() {
    const path = useParams<{ id: string }>();
    const { data, loading, error } = useApi(path.id);

    if (loading) return <p>Loading...</p>;
    if (error || !data) return <p>Error loading api</p>;

    return (
        <div>
            <DetailTabs basePath={`/apis/${path.id}`} tabs={apiTabs} />
            <Outlet context={{ api: data }} />
        </div>
    );
}
