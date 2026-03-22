import {useMemo, useState} from "react";
import { useOutletContext } from "react-router-dom";
import type {Application} from "../../types.ts";
import "./ApplicationApisPage.css"
import {PermissionSelector} from "../../create/components/PermissionSelector.tsx";

import {useClientPermissions} from "../hooks/useClientPermissions.tsx";

export function ApplicationApisPage() {
    const { application } = useOutletContext<{ application: Application }>();

    const { data, loading, error } = useClientPermissions(application.id);

    const [selectedApiId, setSelectedApiId] = useState<string | null>(null);

    // --- SERVER STATE ---
    const serverState = useMemo<Record<string, string[]>>(() => {
        if (!data) return {};

        return Object.fromEntries(
            data.map(api => [api.apiId, api.selectedScopes])
        );
    }, [data]);

    // --- DRAFT STATE ---
    const [draftState, setDraftState] = useState<Record<string, string[]> | null>(null);

    // --- EFFECTIVE STATE ---
    const state = draftState ?? serverState;

    // --- SELECTED API ---
    const selectedClientApi = useMemo(() => {
        if (!data || !selectedApiId) return null;

        const api = data.find(x => x.apiId === selectedApiId);
        if (!api) return null;

        return {
            ...api,
            selectedScopes: state[api.apiId] ?? []
        };
    }, [data, selectedApiId, state]);

    // --- UPDATE ---
    const updateScopes = (apiId: string, scopes: string[]) => {
        setDraftState(prev => ({
            ...(prev ?? serverState),
            [apiId]: scopes
        }));
    };

    if (loading) return <p>Loading...</p>;
    if (error || !data) return <p>Error loading api access</p>;

    return (
        <div className="api-access">
            {/* SIDEBAR */}
            <aside className="api-access__sidebar">
                <h3 className="api-access__title">APIs</h3>

                <div className="api-access__list">
                    {data.map(api => {
                        const scopes = state[api.apiId] ?? [];
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
    );
}