import {CreateApplicationForm} from "./CreateApplicationForm.tsx";
import Modal from "./Modal.tsx";

export default function CreateApplicationModal({ open, onClose, onCreated }: { open: boolean; onClose: () => void; onCreated: () => void }) {
    return (
        <Modal open={open} onClose={onClose}>
            <CreateApplicationForm
                onSuccess={() => {
                    onCreated();
                    onClose();
                }}
                onCancel={onClose} />
        </Modal>
    )
}
