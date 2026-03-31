import {Outlet, useParams} from "react-router-dom";
import {useApplication} from "../../hooks/useApplication.tsx";
import {DetailTabs} from "../../../../shared/components/DetailTabs.tsx";
import {applicationTabs} from "../../routes.tsx";

export default function ApplicationLayout() {
    const path = useParams<{ id: string }>();
    const { data, loading, error } = useApplication(path.id);

    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error loading application</p>;

    return (
        <div>
            <DetailTabs basePath={`/applications/${path.id}`} tabs={applicationTabs} />
            <Outlet context={{ application: data }}/>
        </div>
    );
}
