import {Outlet, useParams} from "react-router-dom";
import {useApi} from "../../../applications/hooks.tsx";
import "../settings/ApiSettingsPage.css";
import {ApiTabs} from "./ApiTabs.tsx";

export default function ApiLayout() {
    const path = useParams<{ id: string }>();
    const { data, loading, error } = useApi(path.id);

    if (loading) return <p>Loading...</p>;
    if (error || !data) return <p>Error loading api</p>;

    return (
        <div>
            <ApiTabs basePath={`/apis/${path.id}`} />
            <Outlet context={{ api: data }} />
        </div>
    );
}
