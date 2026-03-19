import {ApplicationListItem} from "../components/ApplicationListItem.tsx";
import {useApplications} from "../hooks/useApplications.tsx";
import {CreateApplicationTrigger} from "../create";

export default function ApplicationListPage() {
    const { data, loading, error } = useApplications();

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <h1>Applications</h1>

            <CreateApplicationTrigger />

            <ul>
                {data?.map(app => (
                    <ApplicationListItem key={app.id} application={app} />
                ))}
            </ul>
        </div>
    );
}

