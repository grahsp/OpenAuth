import {useApplications} from "../hooks.tsx";
import {ApplicationListItem} from "../components/ApplicationListItem.tsx";
import {useState} from "react";
import CreateApplicationModal from "../create/CreateApplicationModal.tsx";
import {useNavigate} from "react-router-dom";
import Modal from "../components/Modal.tsx";

export default function ApplicationListPage() {
    const [isCreateOpen, setIsCreateOpen] = useState(false);
    const navigate = useNavigate();

    const { data, loading, error } = useApplications();

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <button onClick={() => setIsCreateOpen(true)}>
                Create Application
            </button>

                <Modal open={isCreateOpen} onClose={() => setIsCreateOpen(false)}>
                    <CreateApplicationModal
                        onCancel={() => setIsCreateOpen(false)}
                        onSuccess={(id) => {
                            setIsCreateOpen(false);
                            navigate(`/applications/${id}`)
                        }}
                    />
                </Modal>

            <h1>Applications</h1>

            <ul>
                {data.map(app => (
                    <ApplicationListItem key={app.id} application={app} />
                ))}
            </ul>
        </div>
    );
}

