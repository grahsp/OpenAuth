import type {ReactNode} from "react";
import "./Modal.css";

export default function Modal({open, onClose, children}: { open: boolean; onClose: () => void; children: ReactNode; }) {
    if (!open) return null;

    return (
        <div className="modal-overlay" onClick={onClose}>
            <div className="modal" onClick={(e) => e.stopPropagation()}>
                <button className="modal__close-button" onClick={onClose}>x</button>
                {children}
            </div>
        </div>
    );
}