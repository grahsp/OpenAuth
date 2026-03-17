import {useState} from "react";
import type {FormEvent} from "react";
import {useCreateApplication} from "../hooks.tsx";
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
    const [nameBlurred, setNameBlurred] = useState<boolean>(false);
    const isNameValid = name.trim().length > 0;

    // TODO: application should not require redirect uris on creation.
    const redirectUris = "https://google.com/";

    const handleSubmit = async (e: FormEvent) => {
        e.preventDefault();

        if (!isNameValid)
            return;

        const uris = redirectUris
            .split(',')
            .map(uri => uri.trim())
            .filter(uri => uri.length > 0);

        try {
            await create({name, type, redirectUris: uris});
            onSuccess();
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
                    onBlur={() => setNameBlurred(true)}
                    onChange={(e) => {
                        setName(e.target.value);
                        if (nameBlurred) setNameBlurred(false);
                    }}
                />

                {nameBlurred && !isNameValid &&<p className="error">"Name" is not allowed to be empty</p>}
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
                >
                    {loading ? "Creating..." : "Create"}
                </button>
            </div>
        </form>
    );
}