import {useApis} from "../../../apis/hooks/useApis.tsx";
import "./ApiSelector.css";

type ApiSelectorProps = {
    value?: string;
    onChange: (apiId?: string) => void;
};

export function ApiSelector({ value, onChange }: ApiSelectorProps) {
    const { data, loading, error } = useApis();

    return (
        <div className="api-selector">
            <label className="api-selector__label">
                API <span className="label__required">*</span>
            </label>

            {loading && <p className="api-selector__status">Loading APIs...</p>}
            {error && <p className="error">{error}</p>}

            {!loading && !error && data && (
                <div className="api-selector__control">
                    <select
                        className="api-selector__input"
                        value={value ?? ""}
                        onChange={(e) =>
                            onChange(
                                e.target.value === "" ? undefined : e.target.value
                            )
                        }
                    >
                        <option value="">Select an API</option>

                        {data.map(api => (
                            <option key={api.id} value={api.id}>
                                {api.name}
                            </option>
                        ))}
                    </select>

                    {value && (
                        <button
                            type="button"
                            className="api-selector__clear"
                            onClick={() => onChange(undefined)}
                        >
                            ✕
                        </button>
                    )}
                </div>
            )}
        </div>
    );
}
