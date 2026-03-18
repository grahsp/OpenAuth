import {useState} from "react";
import type {FormEvent} from "react";
import {useApiDetails, useApiSummaries, useCreateApplication} from "../hooks.tsx";
import ApplicationTypeCard from "./ApplicationTypeCard.tsx";
import "./CreateApplicationForm.css"
import type {ApplicationType} from "../types.ts";

export function CreateApplicationForm(
    {
        onSuccess,
        onCancel
    }: {
        onSuccess: () => void;
        onCancel: () => void;
    }) {
    const {create, loading, error} = useCreateApplication();

    // Type
    const [type, setType] = useState<ApplicationType>("spa");

    // Name
    const [name, setName] = useState<string>("My App");
    const isNameValid = name.trim().length > 0;

    // Summaries
    const { data: summaries, loading: summariesLoading, error: summariesError } = useApiSummaries();
    const [selectedApi, setSelectedApi] = useState<string>("");
    const isApiSelected = type !== "m2m" || selectedApi.length !== 0;

    // Details
    const { data: details, loading: detailsLoading, error: detailsError } = useApiDetails(selectedApi);
    const [selectedPermissions, setSelectedPermissions] = useState<string[]>([]);
    const isPermissionsValid = isApiSelected && selectedPermissions.length > 0;

    const togglePermission = (scope: string) => {
        setSelectedPermissions(prev =>
            prev.includes(scope)
                ? prev.filter(p => p !== scope)
                : [...prev, scope]
        );
    };

    const isValid = isNameValid && isPermissionsValid;
    const [touched, setTouched] = useState({
        name: false,
        api: false,
        permissions: false,
    });

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (!isValid) {
            setTouched({
                name: true,
                api: true,
                permissions: true,
            })
            return;
        }

        try {
            const application = await create({name, type, apiId: selectedApi, scopes: selectedPermissions});
            onSuccess();

            console.log("Created:", application);
        } catch {
            // already handled in hook
        }

    };

    return (
        <form className="form" onSubmit={handleSubmit}>
            <h2 className="form-title">Create application</h2>

            {/* NAME */}
            <div className="form-field">
                <label className="label">
                    Name <span className="label__required">*</span>
                </label>

                <input
                    className={`input ${!isNameValid ? "input--error" : ""}`}
                    value={name}
                    onBlur={() => setTouched(t => ({ ...t, name: true }))}
                    onChange={(e) => {
                        setName(e.target.value);
                        setTouched(t => ({ ...t, name: false }));
                    }}
                />

                {touched.name && !isNameValid &&<p className="error">"Name" is not allowed to be empty</p>}
            </div>

            {/* TYPE */}
            <div className="type-selector">
                <ApplicationTypeCard
                    selected={type === "spa"}
                    title="Single Page Application"
                    description="A JavaScript front-end app"
                    onClick={() => setType("spa")}
                />

                <ApplicationTypeCard
                    selected={type === "m2m"}
                    title="Machine to Machine"
                    description="Backend communication"
                    onClick={() => setType("m2m")}
                />
            </div>

            {/*API*/}
            {type === "m2m" && (
                <>
                    {/* API Selector */}
                    <div className="form-field">
                        <label className="label">
                            API <span className="label__required">*</span>
                        </label>

                        {summariesLoading && <p>Loading APIs...</p>}
                        {summariesError && <p className="error">{summariesError}</p>}

                        {!summariesLoading && !summariesError && (
                            <select
                                className="input"
                                value={selectedApi ?? ""}
                                onChange={(e) => setSelectedApi(e.target.value)}
                            >
                                <option value="">Select an API</option>

                                {summaries.map(api => (
                                    <option key={api.id} value={api.id}>
                                        {api.name}
                                    </option>
                                ))}
                            </select>
                        )}
                    </div>

                    {/* Permissions */}
                    {selectedApi && (
                        <div className="permissions">
                            <label className="permissions__label">
                                Permissions <span className="permissions__required">*</span>
                            </label>

                            {detailsLoading && <p className="permissions__status">Loading permissions...</p>}
                            {detailsError && <p className="permissions__error">{detailsError}</p>}

                            {!detailsLoading && details && (
                                <>
                                    {details.permissions.length === 0 ? (
                                        <p className="permissions__empty">No permissions available</p>
                                    ) : (
                                        <div className="permissions__list">
                                            {details.permissions.map(permission => {
                                                const isChecked = selectedPermissions.includes(permission.scope);

                                                return (
                                                    <label
                                                        key={permission.scope}
                                                        className={`permission-chip ${isChecked ? "permission-chip--active" : ""}`}
                                                    >
                                                        <input
                                                            type="checkbox"
                                                            checked={isChecked}
                                                            onChange={() => togglePermission(permission.scope)}
                                                        />

                                                        <span className="permission-chip__box" />
                                                        <span className="permission-chip__text">{permission.scope}</span>
                                                    </label>
                                                );
                                            })}
                                        </div>
                                    )}
                                </>
                            )}
                        </div>
                    )}
                </>
            )}

            {error && <p className="error">{error}</p>}

            {/* FOOTER */}
            <div className="form-footer">
                <button
                    type="button"
                    className="btn secondary"
                    onClick={onCancel}
                >
                    Cancel
                </button>

                <button
                    type="submit"
                    className="btn primary"
                    disabled={!isValid}
                    onClick={() => setTouched({ name: true, api: true, permissions: true })}
                >
                    {loading ? "Creating..." : "Create"}
                </button>
            </div>
        </form>
    );
}