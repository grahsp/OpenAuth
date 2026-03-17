import {useApplications} from "../hooks.tsx";
import {ApplicationListItem} from "../components/ApplicationListItem.tsx";
import {useState} from "react";
import CreateApplicationModal from "../components/CreateApplicationModal.tsx";

export default function ApplicationListPage() {
    const [isCreateOpen, setIsCreateOpen] = useState(false);

    const { data, loading, error, reload } = useApplications();

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <button onClick={() => setIsCreateOpen(true)}>
                Create Application
            </button>

            { isCreateOpen && (
                <CreateApplicationModal
                    open={isCreateOpen}
                    onClose={() => setIsCreateOpen(false)}
                    onCreated={reload}
                />
            )}

            <h1>Applications</h1>

            <ul>
                {data.map(app => (
                    <ApplicationListItem application={app} />
                ))}
            </ul>
        </div>
    );
}

