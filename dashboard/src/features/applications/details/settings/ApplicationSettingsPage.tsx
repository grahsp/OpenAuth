import {useOutletContext} from "react-router-dom";
import type {Application, ApplicationType, UpdateApplicationConfigurationRequest} from "../../types.ts";
import {useState} from "react";
import {useUpdateApplicationConfiguration} from "../hooks/useUpdateApplicationConfiguration.ts";
import "./ApplicationSettingsPage.css";

type Draft = {
    name: string;
    applicationType: ApplicationType,
    redirectUris: string;
    allowedGrantTypes: string[];
    tokenLifetimeInSeconds: number;
};

export function ApplicationSettingsPage() {
    const {application} = useOutletContext<{ application: Application }>();
    const {update, error} = useUpdateApplicationConfiguration();

    const createDraft = (application: Application): Draft => ({
        name: application.name,
        applicationType: application.applicationType,
        redirectUris: application.redirectUris.join(", "),
        allowedGrantTypes: application.allowedGrantTypes,
        tokenLifetimeInSeconds: application.tokenLifetimeInSeconds
    });

    const [initialDraft] = useState<Draft>(() => createDraft(application));
    const [draft, setDraft] = useState<Draft>(initialDraft);

    const updateDraft = <K extends keyof Draft>(key: K, value: Draft[K]) => {
        setDraft(prev => ({...prev, [key]: value}));
    };

    const handleSave = async () => {
        const request: UpdateApplicationConfigurationRequest = {
            name: draft.name,
            applicationType: draft.applicationType,
            redirectUris: draft.redirectUris.split(',').map(x => x.trim()).filter(x => x !== ""),
            tokenLifetimeInSeconds: draft.tokenLifetimeInSeconds,
            allowedGrantTypes: draft.allowedGrantTypes
        };

        await update(application.id, request);
    };

    const toggleGrant = (grant: string) => {
        setDraft(prev => {
            const exists = prev.allowedGrantTypes.includes(grant);

            const allowedGrantTypes = exists
                ? prev.allowedGrantTypes.filter(g => g !== grant)
                : [...prev.allowedGrantTypes, grant].sort();

            return {...prev, allowedGrantTypes};
        });
    };

    const resetDraft = () => {
        setDraft(initialDraft);
    };

    return (
        <div className="application-page">
            {error && <p>{error}</p>}

            {/* ===== BASIC INFORMATION ===== */}
            <section className="section">
                <h2 className="section__title">Basic Information</h2>

                <div className="form-field">
                    <label>Name</label>
                    <input
                        value={draft.name}
                        onChange={(e) =>
                            updateDraft("name", e.target.value)}
                    />
                </div>

                <div className="form-field">
                    <label>Application Type</label>
                    <input value={application.applicationType} disabled/>
                </div>

                <div className="form-field">
                    <label>Client ID</label>
                    <div className="input-with-action">
                        <input value={application.id} disabled/>
                        <button
                            onClick={() => navigator.clipboard.writeText(application.id)}
                        >
                            Copy
                        </button>
                    </div>
                </div>
            </section>

            <div className="section-divider"/>

            {/* ===== CONFIGURATION ===== */}
            <section className="section">
                <h2 className="section__title">Configuration</h2>

                <div className="form-field">
                    <label>Redirect URIs</label>
                    <textarea
                        value={draft.redirectUris}
                        onChange={(e) =>
                            updateDraft("redirectUris", e.target.value)}
                        placeholder="https://example.com/callback"
                    />
                    <small>One URI per line</small>
                </div>

                <div className="form-field">
                    <label>Allowed Grant Types</label>
                    <div className="checkbox-group">
                        {application.availableGrantTypes.map((grant) => (
                            <label key={grant} className="checkbox">
                                <input
                                    type="checkbox"
                                    checked={draft.allowedGrantTypes.includes(grant)}
                                    onChange={() => toggleGrant(grant)}
                                />
                                {grant}
                            </label>
                        ))}
                    </div>
                </div>

                <div className="form-field">
                    <label>Token Lifetime (seconds)</label>
                    <input
                        type="number"
                        value={draft.tokenLifetimeInSeconds}
                        onChange={(e) =>
                            updateDraft("tokenLifetimeInSeconds", Number(e.target.value))}
                    />
                </div>
            </section>

            {/* ===== FOOTER ===== */}
            <div className="form-footer">
                <button className="btn btn--secondary" onClick={resetDraft}>
                    Discard
                </button>
                <button className="btn btn--primary" onClick={handleSave}>
                    Save
                </button>
            </div>
        </div>
    );
}
