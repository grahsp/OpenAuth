import {useOutletContext} from "react-router-dom";
import type {Api} from "../../types.ts";
import {ResourceCopyField} from "../../../../shared/components/ResourceCopyField.tsx";
import "./ApiSettingsPage.css";

export function ApiSettingsPage() {
    const { api } = useOutletContext<{ api: Api }>();

    return (
        <div className="application-page">
            <section className="section">
                <h2 className="section__title">General Settings</h2>

                <div className="form-field">
                    <label>Name</label>
                    <input value={api.name} disabled />
                </div>

                <div className="form-field">
                    <label>Audience</label>
                    <ResourceCopyField value={api.audience} />
                </div>
            </section>
        </div>
    );
}
