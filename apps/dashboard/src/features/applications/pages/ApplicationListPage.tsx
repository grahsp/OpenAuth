import {useApplications} from "../hooks.tsx";
import CreateApplicationForm from "../components/CreateApplicationForm.tsx";
import {ApplicationListItem} from "../components/ApplicationListItem.tsx";

export default function ApplicationListPage() {
    const { data, loading, error, reload } = useApplications();

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <CreateApplicationForm onCreated={reload}/>
            <h1>Applications</h1>

            <ul>
                {data.map(app => (
                    <ApplicationListItem application={app} />
                ))}
            </ul>
        </div>
    );
}

