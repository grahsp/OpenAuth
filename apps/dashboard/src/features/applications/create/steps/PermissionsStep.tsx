import type {Dispatch, SetStateAction} from "react";
import {ApiSelector} from "../components/ApiSelector.tsx";
import {PermissionSelector} from "../components/PermissionSelector.tsx";
import type {Draft} from "../CreateApplicationModal.tsx";

export function PermissionsStep(
    {
        draft,
        setDraft,
        onBack,
        onSubmit,
        error,
    }: {
        draft: Draft;
        setDraft: Dispatch<SetStateAction<Draft>>;
        onBack: () => void;
        onSubmit: () => void;
        error: string | null;
    }) {
    const isValid = !!draft.apiId && draft.scopes.length > 0;

    return (
        <>
            <ApiSelector
                value={draft.apiId}
                onChange={(id) => setDraft(draft => ({...draft, apiId: id, scopes: []}))}
            />

            {draft.apiId && (
                <PermissionSelector
                    apiId={draft.apiId}
                    value={draft.scopes}
                    onChange={(scopes) => setDraft(draft => ({...draft, scopes}))}
                />
            )}

            {/* FOOTER */}
            <div className="form__footer">
                {error && <p className="form__footer-error">{error}</p>}

                <div className="form__footer-actions">
                    <button className="btn secondary" onClick={onBack}>
                        Back
                    </button>

                    <button
                        className="btn primary"
                        disabled={!isValid}
                        onClick={onSubmit}
                    >
                        Authorize
                    </button>
                </div>
            </div>
        </>
    );
}