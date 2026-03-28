import {Outlet, useParams} from "react-router-dom";
import {useApplication} from "../../hooks/useApplication.tsx";
import "../settings/ApplicationSettingsPage.css"
import {ApplicationTabs} from "./ApplicationTabs.tsx";

export default function ApplicationLayout() {
    const path = useParams<{ id: string }>();
    const { data, loading, error } = useApplication(path.id);

    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error loading application</p>;

    return (
        <div>
            <ApplicationTabs basePath={`/applications/${path.id}`} />
            <Outlet context={{ application: data }}/>
        </div>
    );
}

