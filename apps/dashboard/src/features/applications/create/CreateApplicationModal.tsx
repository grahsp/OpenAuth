import type {ApplicationType, CreateApplicationResponse} from "../types.ts";
import {useEffect, useState} from "react";
import {SecretReveal} from "./components/SecretReveal.tsx";
import {PermissionsStep} from "./steps/PermissionsStep.tsx";
import {BasicInfoStep} from "./steps/BasicInfoStep.tsx";

import "./CreateApplicationModal.css"
import {useCreateApplication} from "./hooks/useCreateApplication.tsx";

type Step = "basic" | "permissions" | "success";

export type Draft = {
    name: string;
    type: ApplicationType;
    apiId?: string;
    scopes: string[];
};

export default function CreateApplicationModal(
    {
        onSuccess,
        onCancel
    }: {
        onSuccess: (id: string) => void;
        onCancel: () => void;
    }) {
    const { create, error } = useCreateApplication();
    const [result, setResult] = useState<CreateApplicationResponse>();

    const [step, setStep] = useState<Step>("basic");

    const [draft, setDraft] = useState<Draft>({
        name: "My App",
        type: "spa",
        apiId: undefined,
        scopes: []
    });


    // --- SUBMIT ---
    const handleCreate = async () => {
        const result = await create({
            name: draft.name,
            type: draft.type,
            apiId: draft.apiId,
            scopes: draft.scopes
        });

        if (!result)
            return;

        setResult(result);
        setStep("success");
    };

    // --- STEP TRANSITIONS ---
    useEffect(() => {
        if (step === "success" && result && !result.secret) {
            onSuccess(result.id);
        }
    }, [step, result, onSuccess]);

    const nextFromBasic = () => {
        if (draft.type === "m2m") {
            setStep("permissions");
        } else {
            handleCreate();
        }
    };

    const backToBasic = () => {
        setDraft(draft => ({ ...draft, apiId: undefined, scopes: [] }));
        setStep("basic");
    };

    // --- RENDER ---
    return (
        <div className="form">
            {step === "basic" && (
                <>
                    <h2 className="form__title">Create application</h2>

                    <BasicInfoStep
                        draft={draft}
                        setDraft={setDraft}
                        onNext={nextFromBasic}
                        onCancel={onCancel}
                        error={error}
                    />
                </>
            )}

            {step === "permissions" && (
                <>
                    <h2 className="form__title">Authorize Machine to Machine Application</h2>

                    <PermissionsStep
                        draft={draft}
                        setDraft={setDraft}
                        onBack={backToBasic}
                        onSubmit={handleCreate}
                        error={error}
                    />
                </>
            )}

            {step === "success" && result && (
                <>
                    <h2 className="form__title">Success!</h2>
                    <SecretReveal
                        secret={result.secret}
                        onContinue={() => {
                            onSuccess(result.id);
                        }}
                    />
                </>
            )}
        </div>
    );
}
