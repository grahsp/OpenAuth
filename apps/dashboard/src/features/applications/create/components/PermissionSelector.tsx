import { useMemo, useState } from "react";
import {useApiDetails} from "../../hooks.tsx";
import "./PermissionSelector.css"

type Props = {
    apiId: string;
    value: string[];
    onChange: (scopes: string[]) => void;
};

export default function PermissionSelector({apiId, value, onChange}: Props) {
    const { data, loading, error } = useApiDetails(apiId);

    const [filter, setFilter] = useState("");

    // --- FILTER ---
    const filtered = useMemo(() => {
        const permissions = data?.permissions ?? [];

        if (!filter)
            return permissions;

        const f = filter.toLowerCase();
        return permissions.filter(p =>
            p.scope.toLowerCase().includes(f)
        );
    }, [data?.permissions, filter]);

    // --- SELECTION ---
    const toggle = (scope: string) => {
        if (value.includes(scope)) {
            onChange(value.filter(s => s !== scope));
        } else {
            onChange([...value, scope]);
        }
    };

    const selectAll = () => {
        onChange(data?.permissions.map(p => p.scope) ?? []);
    };

    const clearAll = () => {
        onChange([]);
    };

    return (
        <div className="permission-selector">
            {/* HEADER */}
            <div className="permission-selector__header">
                <span>Permissions</span>

                <div className="permission-selector__actions">
                    <span>Select:</span>

                    <button
                        type="button"
                        className="link"
                        onClick={selectAll}
                    >
                        All
                    </button>

                    <button
                        type="button"
                        className="link"
                        onClick={clearAll}
                    >
                        None
                    </button>

                    <input
                        className="permission-selector__filter"
                        placeholder="Filter permissions"
                        value={filter}
                        onChange={(e) => setFilter(e.target.value)}
                    />
                </div>
            </div>

            {/* STATES */}
            {loading && (
                <p className="permission-selector__status">
                    Loading permissions...
                </p>
            )}

            {error && <p className="error">{error}</p>}

            {/* LIST */}
            {!loading && !error && (
                <div className="permission-selector__list">
                    {filtered.length === 0 && (
                        <p className="permission-selector__empty">
                            No permissions found
                        </p>
                    )}

                    {filtered.map(permission => {
                        const checked = value.includes(permission.scope);

                        return (
                            <label
                                key={permission.scope}
                                className={`permission-item ${
                                    checked ? "permission-item--selected" : ""
                                }`}
                            >
                                <input
                                    type="checkbox"
                                    checked={checked}
                                    onChange={() => toggle(permission.scope)}
                                />

                                <span>{permission.scope}</span>
                            </label>
                        );
                    })}
                </div>
            )}
        </div>
    );
}
