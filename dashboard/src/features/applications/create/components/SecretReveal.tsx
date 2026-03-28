import {useState} from "react";
import "./SecretReveal.css"

export function SecretReveal(
    {
        secret,
        onContinue
    }: {
        secret?: string;
        onContinue: () => void;
    }) {
    return (
        <div className="secret-reveal">
            {secret && (
                <>
                    <div className="secret-reveal__group">
                        <div className="secret-reveal__field">
                            <span className="secret-reveal__label">Client Secret</span>
                            <div className="secret-reveal__value">
                                <span>{secret}</span>
                                <CopyButton value={secret}/>
                            </div>
                        </div>
                    </div>

                    <div className="secret-reveal__warning">
                        This is the only time you will see your client secret. Store it securely.
                    </div>
                </>
            )}

            <div className="secret-reveal__footer">
                <button
                    className="btn primary"
                    onClick={onContinue}
                >
                    Continue
                </button>
            </div>
        </div>
    );
}

function CopyButton({value}: { value: string }) {
    const [copied, setCopied] = useState(false);

    const handleCopy = async () => {
        await navigator.clipboard.writeText(value);
        setCopied(true);

        setTimeout(() => setCopied(false), 2000);
    };

    return (
        <button
            type="button"
            className="secret-reveal__copy"
            onClick={handleCopy}
        >
            {copied ? "Copied ✓" : "Copy"}
        </button>
    );
}