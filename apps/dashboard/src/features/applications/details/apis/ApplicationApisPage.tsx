import {useMemo, useState} from "react";
import { useOutletContext } from "react-router-dom";
import type {Application} from "../../types.ts";
import "./ApplicationApisPage.css"
import {PermissionSelector} from "../../create/components/PermissionSelector.tsx";
import {useClientPermissions} from "../hooks/useClientPermissions.tsx";

import {useSetClientApiAccess} from "../hooks/useSetClientApiAccess.tsx";

export function ApplicationApisPage() {
    const { application } = useOutletContext<{ application: Application }>();

    const { data, loading, error, refresh } = useClientPermissions(application.id);
    const { update, loading: saving, error: saveError } = useSetClientApiAccess();

    const [selectedApiId, setSelectedApiId] = useState<string | null>(null);

    // --- STATE ---
    const serverState = useMemo<Record<string, string[]>>(() => {
        if (!data) return {};

        return Object.fromEntries(
            data.map(api => [api.apiId, api.selectedScopes])
        );
    }, [data]);

    const [draft, setDraft] = useState<Record<string, string[]> | null>(null);
    const effectiveScopes = draft ?? serverState;

    // --- SELECTED API ---
    const selectedClientApi = useMemo(() => {
        if (!data || !selectedApiId) return null;

        const api = data.find(x => x.apiId === selectedApiId);
        if (!api) return null;

        return {
            ...api,
            selectedScopes: effectiveScopes[api.apiId] ?? []
        };
    }, [data, selectedApiId, effectiveScopes]);

    // --- UPDATE ---
    const updateScopes = (apiId: string, scopes: string[]) => {
        setDraft(prev => {
            const base = prev ?? serverState;

            return {
                ...base,
                [apiId]: scopes
            };
        });
    };

    const resetDraft = () => {
        setDraft(null);
    };

    const handleSave = async () => {
        if (!selectedApiId || !draft)
            return;

        const current = effectiveScopes[selectedApiId] ?? [];
        const original = serverState[selectedApiId] ?? [];

        if (equalScopes(current, original))
            return;

        await update({
            clientId: application.id,
            apiId: selectedApiId,
            scopes: current
        });

        await refresh();
        setDraft(null);
    };

    function equalScopes(a: string[], b: string[]) {
        if (a.length !== b.length) return false;

        const set = new Set(a);
        return b.every(x => set.has(x));
    }

    if (loading) return <p>Loading...</p>;
    if (error || !data) return <p>Error loading api access</p>;

    return (
        <div className="api-access-page">
            <div className="api-access">
                {/* SIDEBAR */}
                <aside className="api-access__sidebar">
                    <h3 className="api-access__title">APIs</h3>

                    <div className="api-access__list">
                        {data.map(api => {
                            const scopes = effectiveScopes[api.apiId] ?? [];
                            const isSelected = selectedApiId === api.apiId;
                            const isAuthorized = scopes.length > 0;

                            return (
                                <button
                                    key={api.apiId}
                                    className={`api-item ${isSelected ? "active" : ""}`}
                                    onClick={() => setSelectedApiId(api.apiId)}
                                >
                                    <div className="api-item__name">{api.name}</div>

                                    <span
                                        className={`api-item__badge ${isAuthorized
                                            ? "authorized"
                                            : "unauthorized"}`}
                                    >
                                    {isAuthorized ? "Authorized" : "Unauthorized"}
                                </span>
                                </button>
                            );
                        })}
                    </div>
                </aside>

                {/* CONTENT */}
                <section className="api-access__content">
                    {!selectedClientApi && (
                        <div className="api-access__placeholder">
                            Select an API to manage permissions
                        </div>
                    )}

                    {selectedClientApi && (
                        <>
                            <div className="api-access__header">
                                <h2>{selectedClientApi.name}</h2>
                                <p className="api-access__subtitle">
                                    Configure scopes for this API
                                </p>
                            </div>

                            <PermissionSelector
                                options={selectedClientApi.availableScopes}
                                value={selectedClientApi.selectedScopes}
                                onChange={(scopes) =>
                                    updateScopes(selectedClientApi.apiId, scopes)
                                }
                            />
                        </>
                    )}
                </section>
            </div>


            <div className="form-footer">
                {saveError && <p className="error">{saveError}</p>}

                <button
                    className="btn btn--secondary"
                    onClick={resetDraft}
                    disabled={!draft}
                >
                    Cancel
                </button>

                <button
                    className="btn btn--primary"
                    onClick={handleSave}
                    disabled={!selectedApiId || saving}
                >
                    Save
                </button>
            </div>
        </div>
    );
}