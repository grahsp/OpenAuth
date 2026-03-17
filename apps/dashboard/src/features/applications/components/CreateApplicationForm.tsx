import {useState} from "react";
import * as React from "react";
import {useCreateApplication} from "../hooks.tsx";

export default function CreateApplicationForm({ onCreated }: { onCreated: () => void }) {
    const { create, loading, error } = useCreateApplication();

    const [type, setType] = useState<"spa" | "m2m">("spa");
    const [name, setName] = useState("");
    const [redirectUris, setRedirectUris] = useState("");

    const handleSubmit = async (e: React.FormEvent) => {
        e.preventDefault();

        const uris = redirectUris
            .split(',')
            .map(uri => uri.trim())
            .filter(uri => uri.length > 0);

        await create({ name, type, redirectUris: uris });

        onCreated();
    };

    return (
        <form onSubmit={handleSubmit}>
            <h2>Create Application</h2>

            <input
                placeholder="Name"
                value={name}
                onChange={e => setName(e.target.value)}
            />

            <select value={type} onChange={e => setType(e.target.value as "spa" | "m2m")}>
                <option value="spa">SPA</option>
                <option value="m2m">M2M</option>
            </select>

            <textarea
                placeholder="Redirect URIs (separated by commas)"
                value={redirectUris}
                onChange={e => setRedirectUris(e.target.value)}
            />

            <button disabled={loading} type="submit">
                {loading ? "Creating.." : "Create"}
            </button>
        </form>
    );
}