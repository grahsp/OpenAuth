import {useMemo, useState} from "react";
import {useOutletContext} from "react-router-dom";
import type {Api} from "../../types.ts";
import {PermissionSelector} from "../../../applications/create/components/PermissionSelector.tsx";
import {useSetClientApiAccess} from "../../../applications/details/hooks/useSetClientApiAccess.tsx";
import {useApiClientPermissions} from "../hooks/useApiClientPermissions.tsx";
import "./ApiApplicationsPage.css";

export function ApiApplicationsPage() {
    const { api } = useOutletContext<{ api: Api }>();
    const { data, loading, error, refresh } = useApiClientPermissions(api.id);
    const { update, loading: saving, error: saveError } = useSetClientApiAccess();

    const [selectedClientId, setSelectedClientId] = useState<string | null>(null);

    const serverState = useMemo<Record<string, string[]>>(() => {
        if (!data) return {};

        return Object.fromEntries(
            data.map((client) => [client.clientId, client.selectedScopes])
        );
    }, [data]);

    const [draft, setDraft] = useState<Record<string, string[]> | null>(null);
    const effectiveScopes = draft ?? serverState;

    const sortedClients = useMemo(() => {
        if (!data) return [];

        return [...data].sort((a, b) => {
            const aScopes = serverState[a.clientId] ?? [];
            const bScopes = serverState[b.clientId] ?? [];

            const aAuthorized = aScopes.length > 0;
            const bAuthorized = bScopes.length > 0;

            if (aAuthorized !== bAuthorized) {
                return aAuthorized ? -1 : 1;
            }

            return a.name.localeCompare(b.name);
        });
    }, [data, serverState]);

    const resolvedSelectedClientId = useMemo(() => {
        if (sortedClients.length === 0) return null;

        if (!selectedClientId) return sortedClients[0].clientId;

        const exists = sortedClients.some((client) => client.clientId === selectedClientId);
        return exists ? selectedClientId : sortedClients[0].clientId;
    }, [selectedClientId, sortedClients]);

    const selectedClient = useMemo(() => {
        if (!data || !resolvedSelectedClientId) return null;

        const client = data.find((entry) => entry.clientId === resolvedSelectedClientId);
        if (!client) return null;

        return {
            ...client,
            selectedScopes: effectiveScopes[client.clientId] ?? []
        };
    }, [data, effectiveScopes, resolvedSelectedClientId]);

    const permissionOptions = useMemo(() => {
        return api.permissions.map((permission) => ({
            label: permission.scope,
            value: permission.scope,
            description: permission.description
        }));
    }, [api.permissions]);

    const updateScopes = (clientId: string, scopes: string[]) => {
        setDraft((previous) => {
            const base = previous ?? serverState;

            return {
                ...base,
                [clientId]: scopes
            };
        });
    };

    const resetDraft = () => {
        setDraft(null);
    };

    const handleSave = async () => {
        if (!resolvedSelectedClientId || !draft) return;

        const current = effectiveScopes[resolvedSelectedClientId] ?? [];
        const original = serverState[resolvedSelectedClientId] ?? [];

        if (equalScopes(current, original)) return;

        await update({
            clientId: resolvedSelectedClientId,
            apiId: api.id,
            scopes: current
        });

        await refresh();
        setDraft(null);
    };

    if (loading) return <p>Loading...</p>;
    if (error || !data) return <p>Error loading client access</p>;

    return (
        <div className="api-access-page">
            <div className="api-access">
                <aside className="api-access__sidebar">
                    <h3 className="api-access__title">Applications</h3>

                    <div className="api-access__list">
                        {sortedClients.map((client) => {
                            const scopes = effectiveScopes[client.clientId] ?? [];
                            const isSelected = resolvedSelectedClientId === client.clientId;
                            const hasAccess = scopes.length > 0;

                            return (
                                <button
                                    key={client.clientId}
                                    className={`api-item ${isSelected ? "active" : ""}`}
                                    onClick={() => setSelectedClientId(client.clientId)}
                                >
                                    <div className="api-item__name">{client.name}</div>
                                    <span
                                        className={`api-item__badge ${hasAccess ? "authorized" : "unauthorized"}`}
                                    >
                                        {hasAccess ? "Authorized" : "Unauthorized"}
                                    </span>
                                </button>
                            );
                        })}
                    </div>
                </aside>

                <section className="api-access__content">
                    {selectedClient && (
                        <>
                            <div className="api-access__header">
                                <h2>{selectedClient.name}</h2>
                                <p className="api-access__subtitle">
                                    Configure scopes for this client
                                </p>
                            </div>

                            <PermissionSelector
                                options={permissionOptions}
                                value={selectedClient.selectedScopes}
                                onChange={(scopes) =>
                                    updateScopes(selectedClient.clientId, scopes)
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
                    onClick={() => void handleSave()}
                    disabled={saving}
                >
                    Save
                </button>
            </div>
        </div>
    );
}

function equalScopes(a: string[], b: string[]) {
    if (a.length !== b.length) return false;

    const set = new Set(a);
    return b.every((scope) => set.has(scope));
}
