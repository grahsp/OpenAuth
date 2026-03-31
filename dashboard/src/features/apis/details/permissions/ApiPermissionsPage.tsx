import {useOutletContext} from "react-router-dom";
import type {Api} from "../../types.ts";
import {useState} from "react";
import {useAddApiPermissions} from "../../hooks/useAddApiPermissions.tsx";
import {useRemoveApiPermissions} from "../../hooks/useRemoveApiPermissions.tsx";
import {useApi} from "../../hooks/useApi.tsx";
import "./ApiPermissionsPage.css";

export function ApiPermissionsPage() {
    const { api } = useOutletContext<{ api: Api }>();
    const [scope, setScope] = useState("");
    const [description, setDescription] = useState("");
    const { data, loading: apiLoading, error: apiError, refresh } = useApi(api.id);
    const { add, loading: adding, error: addError } = useAddApiPermissions();
    const { remove, loading: removing, error: removeError } = useRemoveApiPermissions();

    const permissions = data?.permissions ?? api.permissions;

    const handleDelete = async (scope: string) => {
        const success = await remove({
            apiId: api.id,
            scopes: [scope]
        });

        if (success) {
            await refresh();
        }
    };

    const handleAdd = async () => {
        const trimmedScope = scope.trim();
        const trimmedDescription = description.trim();

        if (!trimmedScope) {
            return;
        }

        const success = await add({
            apiId: api.id,
            permissions: {
                [trimmedScope]: trimmedDescription || null
            }
        });

        if (success) {
            setScope("");
            setDescription("");
            await refresh();
        }
    };

    return (
        <div className="api-permissions-page">
            <section className="api-permissions-section">
                <h2 className="api-permissions-section__title">Add a Permission</h2>
                <p className="api-permissions-section__description">
                    Define the permissions (scopes) that this API uses.
                </p>

                {addError && <p className="api-permissions-empty">{addError}</p>}

                <div className="api-permissions-form">
                    <div className="api-permissions-form__field">
                        <label htmlFor="permission-scope">Permission *</label>
                        <input
                            id="permission-scope"
                            value={scope}
                            placeholder="read:appointments"
                            onChange={(event) => setScope(event.target.value)}
                        />
                    </div>

                    <div className="api-permissions-form__field">
                        <label htmlFor="permission-description">Description</label>
                        <input
                            id="permission-description"
                            value={description}
                            placeholder="Read your appointments"
                            onChange={(event) => setDescription(event.target.value)}
                        />
                    </div>

                    <button
                        type="button"
                        className="api-permissions-form__action"
                        onClick={() => void handleAdd()}
                        disabled={adding}
                    >
                        Add
                    </button>
                </div>
            </section>

            <section className="api-permissions-section">
                <h2 className="api-permissions-section__title">List of Permissions</h2>
                <p className="api-permissions-section__description">
                    These are all the permissions that this API uses.
                </p>

                {apiError && <p className="api-permissions-empty">{apiError}</p>}
                {removeError && <p className="api-permissions-empty">{removeError}</p>}

                {apiLoading ? (
                    <p className="api-permissions-empty">Loading permissions...</p>
                ) : permissions.length > 0 ? (
                    <div className="api-permissions-table">
                        <div className="api-permissions-table__header">
                            <div>Permission</div>
                            <div>Description</div>
                            <div />
                        </div>

                        <div className="api-permissions-table__body">
                            {permissions.map((permission) => (
                                <div key={permission.scope} className="api-permissions-table__row">
                                    <div className="api-permissions-table__scope">
                                        {permission.scope}
                                    </div>
                                    <div className="api-permissions-table__description-cell">
                                        {permission.description || "No description provided."}
                                    </div>
                                    <div className="api-permissions-table__actions">
                                        <button
                                            type="button"
                                            className="api-permissions-table__delete"
                                            aria-label={`Delete permission ${permission.scope}`}
                                            onClick={() => void handleDelete(permission.scope)}
                                            disabled={removing}
                                        >
                                            <svg viewBox="0 0 16 16" aria-hidden="true">
                                                <path
                                                    d="M6.5 2.75h3A1.75 1.75 0 0 1 11.25 4.5V5h2a.75.75 0 0 1 0 1.5h-.56l-.53 6.34A1.75 1.75 0 0 1 10.42 14H5.58a1.75 1.75 0 0 1-1.74-1.16L3.31 6.5h-.56a.75.75 0 0 1 0-1.5h2v-.5A1.75 1.75 0 0 1 6.5 2.75Zm3.25 2.25v-.5a.25.25 0 0 0-.25-.25h-3a.25.25 0 0 0-.25.25V5h3.5Zm-3.02 2.25a.75.75 0 0 1 .75.75v3.5a.75.75 0 0 1-1.5 0V8a.75.75 0 0 1 .75-.75Zm2.54 0a.75.75 0 0 1 .75.75v3.5a.75.75 0 0 1-1.5 0V8a.75.75 0 0 1 .75-.75Z"
                                                    fill="currentColor"
                                                />
                                            </svg>
                                        </button>
                                    </div>
                                </div>
                            ))}
                        </div>
                    </div>
                ) : (
                    <p className="api-permissions-empty">No permissions configured for this API.</p>
                )}
            </section>
        </div>
    );
}
