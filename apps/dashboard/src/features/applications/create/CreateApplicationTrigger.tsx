import {useState} from "react";
import { useNavigate } from "react-router-dom";
import Modal from "../components/Modal.tsx";
import CreateApplicationModal from "./CreateApplicationModal.tsx";
import "./CreateApplicationTrigger.css"

export function CreateApplicationTrigger() {
    const [open, setOpen] = useState(false);
    const navigate = useNavigate();

    const handleSuccess = (id: string) => {
        setOpen(false);
        navigate(`/applications/${id}`);
    };

    return (
        <>
            <button
                className="create-app-trigger"
                onClick={() => setOpen(true)}
            >
                <span className="create-app-trigger__icon">+</span>
                <span>Create Application</span>
            </button>

            {open && (
                <Modal open onClose={() => setOpen(false)}>
                    <CreateApplicationModal
                        onCancel={() => setOpen(false)}
                        onSuccess={handleSuccess}
                    />
                </Modal>
            )}
        </>
    );
}