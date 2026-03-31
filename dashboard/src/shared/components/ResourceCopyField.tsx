import {useState} from "react";
import "./ResourceCopyField.css";

export function ResourceCopyField(
    {
        label,
        value
    }: {
        label?: string;
        value: string;
    }) {
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        await navigator.clipboard.writeText(value);
        setCopied(true);
        window.setTimeout(() => setCopied(false), 2000);
    };

    return (
        <div
            className="resource-copy-field"
            onClick={(event) => event.stopPropagation()}
            onMouseDown={(event) => event.stopPropagation()}
            onPointerDown={(event) => event.stopPropagation()}
        >
            { label && <span className="resource-copy-field__label">{label}:</span> }
            <div className="resource-copy-field__value">
                <span className="resource-copy-field__text">{value}</span>
            </div>
            <CopyButton
                copied={copied}
                label={label}
                onClick={handleCopy}
            />
        </div>
    );
}

function CopyButton(
    {
        copied,
        label,
        onClick
    }: {
        copied: boolean;
        label?: string;
        onClick: () => Promise<void>;
    }) {
    return (
        <button
            type="button"
            className="resource-copy-field__copy"
            aria-label={`Copy ${label ?? "value"}`}
            onClick={() => {
                void onClick();
            }}
        >
            {copied ? (
                <svg viewBox="0 0 16 16" aria-hidden="true" className="resource-copy-field__icon">
                    <path
                        d="M13.53 4.47a.75.75 0 0 1 0 1.06l-6 6a.75.75 0 0 1-1.06 0l-3-3a.75.75 0 1 1 1.06-1.06L7 9.94l5.47-5.47a.75.75 0 0 1 1.06 0Z"
                        fill="currentColor"
                    />
                </svg>
            ) : (
                <svg viewBox="0 0 16 16" aria-hidden="true" className="resource-copy-field__icon">
                    <path
                        d="M5.5 2A1.5 1.5 0 0 0 4 3.5V4H3.5A1.5 1.5 0 0 0 2 5.5v7A1.5 1.5 0 0 0 3.5 14h5A1.5 1.5 0 0 0 10 12.5V12h.5A1.5 1.5 0 0 0 12 10.5v-7A1.5 1.5 0 0 0 10.5 2h-5Zm3 10.5a.5.5 0 0 1-.5.5h-4a.5.5 0 0 1-.5-.5v-6a.5.5 0 0 1 .5-.5H4v4.5A1.5 1.5 0 0 0 5.5 12H8v.5Zm2-.5h-5A.5.5 0 0 1 5 11.5v-8a.5.5 0 0 1 .5-.5h5a.5.5 0 0 1 .5.5v8a.5.5 0 0 1-.5.5Z"
                        fill="currentColor"
                    />
                </svg>
            )}
        </button>
    );
}
