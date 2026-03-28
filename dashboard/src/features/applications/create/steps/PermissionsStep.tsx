import {type Dispatch, type SetStateAction} from "react";
import {ApiSelector} from "../components/ApiSelector.tsx";
import {PermissionSelector} from "../components/PermissionSelector.tsx";
import type {Draft} from "../CreateApplicationModal.tsx";
import {useApiPermissionOptions} from "../../hooks.tsx";

type Props = {
    draft: Draft;
    setDraft: Dispatch<SetStateAction<Draft>>;
    onBack: () => void;
    onSubmit: () => void;
    error: string | null;
}

export function PermissionsStep({ draft, setDraft, onBack, onSubmit, error }: Props) {
    const { options, loading, error: loadError } = useApiPermissionOptions(draft.apiId);
    const isValid = !!draft.apiId && draft.scopes.length > 0;

    return (
        <>
            <ApiSelector
                value={draft.apiId}
                onChange={(id) => setDraft(prev => ({ ...prev, apiId: id, scopes: [] }))}
            />

            {draft.apiId && (
                <>
                    {loading && <p>Loading permissions...</p>}
                    {loadError && <p className="error">{loadError}</p>}

                    {!loading && !loadError && (
                        <PermissionSelector
                            options={options}
                            value={draft.scopes}
                            onChange={(scopes) =>
                                setDraft(prev => ({ ...prev, scopes }))}
                        />
                    )}
                </>
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