import {useCallback, useMemo, useState} from "react";
import "./PermissionSelector.css"

export type PermissionOption = {
    label: string;
    value: string;
    description?: string;
}

type Props = {
    options: PermissionOption[];
    value: string[];
    onChange: (value: string[]) => void;
};

export function PermissionSelector({ options, value, onChange }: Props) {
    // --- FILTER ---
    const [filter, setFilter] = useState("");
    const filtered = useMemo(() => {
        if (!filter) return options;

        const f = filter.toLowerCase();

        return options.filter(o =>
            o.value.toLowerCase().includes(f) ||
            o.label.toLowerCase().includes(f) ||
            o.description?.toLowerCase().includes(f)
        );
    }, [options, filter]);

    // --- SELECTION ---
    const toggle = useCallback((scope: string) => {
        if (value.includes(scope)) {
            onChange(value.filter(s => s !== scope));
        } else {
            onChange([...new Set([...value, scope])]);
        }
    }, [value, onChange]);

    const selectAll = () => {
        onChange(filtered.map(o => o.value));
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

            {(
                <div className="permission-selector__list">
                    {filtered.length === 0 && (
                        <p className="permission-selector__empty">
                            No permissions found
                        </p>
                    )}

                    {filtered.map(o => {
                        const checked = value.includes(o.value);

                        return (
                            <label
                                key={o.value}
                                className={`permission-item ${
                                    checked ? "permission-item--selected" : ""
                                }`}
                            >
                                <input
                                    type="checkbox"
                                    checked={checked}
                                    onChange={() => toggle(o.value)}
                                />

                                <span>{o.label}</span>
                            </label>
                        );
                    })}
                </div>
            )}
        </div>
    );
}
