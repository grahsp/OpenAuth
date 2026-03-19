import {type Dispatch, type SetStateAction, useState} from "react";
import {ApplicationTypeSelector} from "../components/ApplicationTypeCard.tsx";
import type {Draft} from "../CreateApplicationModal.tsx";

export function BasicInfoStep(
    {
        draft,
        setDraft,
        onNext,
        onCancel,
        error
    }: {
        draft: Draft;
        setDraft: Dispatch<SetStateAction<Draft>>;
        onNext: () => void;
        onCancel: () => void;
        error: string | null;
    }) {
    const isNameValid = draft.name.trim().length > 0;
    const [touched, setTouched] = useState(false);

    return (
        <>
            {/* NAME */}
            <div className="form__field">
                <label className="label">
                    Name <span className="label__required">*</span>
                </label>

                <input
                    className={`input ${!isNameValid && touched ? "input--error" : ""}`}
                    value={draft.name}
                    onBlur={() => setTouched(true)}
                    onChange={(e) =>
                        setDraft(draft => ({...draft, name: e.target.value}))
                    }
                />

                {touched && !isNameValid && (
                    <p className="error">Name is required</p>
                )}
            </div>

            {/* TYPE */}
            <ApplicationTypeSelector
                value={draft.type}
                onChange={(type) => setDraft(draft => ({...draft, type}))}
            />

            {/* FOOTER */}
            <div className="form__footer">
                {error && <p className="form__footer-error">{error}</p>}

                <div className="form__footer-actions">
                    <button className="btn secondary" onClick={onCancel}>
                        Cancel
                    </button>

                    <button
                        className="btn primary"
                        disabled={!isNameValid}
                        onClick={onNext}
                    >
                        Create
                    </button>
                </div>
            </div>
        </>
    );
}