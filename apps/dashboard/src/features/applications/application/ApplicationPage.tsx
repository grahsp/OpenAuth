import {useState} from "react";
import {useParams} from "react-router-dom";
import {useApplication} from "../hooks/useApplication.tsx";
import type {Application, UpdateApplicationConfigurationRequest} from "../types.ts";
import "./ApplicationPage.css"
import {useUpdateApplicationConfiguration} from "./hooks/useUpdateApplicationConfiguration.ts";

export default function ApplicationPage() {
    const path = useParams<{ id: string }>();
    const { data, loading, error } = useApplication(path.id);

    if (loading) return <p>Loading...</p>
    if (error || !data) return <p>Error loading application</p>;

    return <ApplicationView application={data} />
}

export function ApplicationView({ application }: { application: Application }) {
    const { update, error } = useUpdateApplicationConfiguration();

    const [name, setName] = useState(application.name);
    const [redirectUris, setRedirectUris] = useState(application.redirectUris.join(", "));
    const [allowedGrants, setAllowedGrants] = useState<string[]>(application.allowedGrantTypes);
    const [tokenLifetime, setTokenLifetime] = useState(application.tokenLifetimeInSeconds);

    const handleSave = async () => {
        const request: UpdateApplicationConfigurationRequest = {
            name: name,
            applicationType: application.applicationType,
            redirectUris: redirectUris.split(',').map(x => x.trim()).filter(x => x !== ""),
            tokenLifetimeInSeconds: tokenLifetime,
            allowedGrantTypes: allowedGrants
        };

        await update(application.id, request);
    };

    const toggleGrant = (grant: string) => {
        setAllowedGrants(prev => {
            if (prev.includes(grant))
                return prev.filter(g => g !== grant);

            return [...prev, grant].sort();
        });
    };

    const handleDiscard = () => {
        setName(application.name);
        setRedirectUris(application.redirectUris.join("\n"));
        setTokenLifetime(Number(application.tokenLifetimeInSeconds));
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
                        value={name}
                        onChange={(e) => setName(e.target.value)}
                    />
                </div>

                <div className="form-field">
                    <label>Application Type</label>
                    <input value={application.applicationType} disabled />
                </div>

                <div className="form-field">
                    <label>Client ID</label>
                    <div className="input-with-action">
                        <input value={application.id} disabled />
                        <button
                            onClick={() => navigator.clipboard.writeText(application.id)}
                        >
                            Copy
                        </button>
                    </div>
                </div>
            </section>

            <div className="section-divider" />

            {/* ===== CONFIGURATION ===== */}
            <section className="section">
                <h2 className="section__title">Configuration</h2>

                <div className="form-field">
                    <label>Redirect URIs</label>
                    <textarea
                        value={redirectUris}
                        onChange={(e) => setRedirectUris(e.target.value)}
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
                                    checked={allowedGrants.includes(grant)}
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
                        value={tokenLifetime}
                        onChange={(e) => setTokenLifetime(Number(e.target.value))}
                    />
                </div>
            </section>

            {/* ===== FOOTER ===== */}
            <div className="form-footer">
                <button className="btn btn--secondary" onClick={handleDiscard}>
                    Discard
                </button>
                <button className="btn btn--primary" onClick={handleSave}>
                    Save
                </button>
            </div>
        </div>
    );
}