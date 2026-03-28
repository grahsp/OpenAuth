import {useState} from "react";
import {SecretReveal} from "./components/SecretReveal.tsx";
import {PermissionsStep} from "./steps/PermissionsStep.tsx";
import {BasicInfoStep} from "./steps/BasicInfoStep.tsx";
import {useCreateApplication} from "./hooks/useCreateApplication.tsx";
import type {ApplicationType, CreateApplicationResponse} from "../types.ts";
import "./CreateApplicationModal.css"

const Steps = {
    Define: "define",
    Authorize: "authorize",
    Complete: "complete"
} as const;

type Step = typeof Steps[keyof typeof Steps]

export type Draft = {
    name: string;
    type: ApplicationType;
    apiId?: string;
    scopes: string[];
};

type Props = {
    onSuccess: (id: string) => void;
    onCancel: () => void;
}

export default function CreateApplicationModal({onSuccess, onCancel}: Props) {
    const { create, error } = useCreateApplication();

    const [step, setStep] = useState<Step>(Steps.Define);
    const [result, setResult] = useState<CreateApplicationResponse>();

    const [draft, setDraft] = useState<Draft>({
        name: "My App",
        type: "spa",
        apiId: undefined,
        scopes: []
    });


    // --- SUBMIT ---
    const submit = async () => {
        const result = await create({
            name: draft.name,
            type: draft.type,
            apiId: draft.apiId,
            scopes: draft.scopes
        });

        if (!result)
            return;

        if (!result.secret) {
            onSuccess(result.id);
            return;
        }

        setResult(result);
        setStep(Steps.Complete);
    };

    // --- STEP TRANSITIONS ---
    const next = async () => {
        if (step === Steps.Define) {
            if (draft.type === "m2m") {
                setStep(Steps.Authorize);
                return;
            }

            await submit();
            return;
        }

        if (step === Steps.Authorize) {
            await submit();
        }
    };

    const back = () => {
        setDraft(draft => ({ ...draft, apiId: undefined, scopes: [] }));
        setStep(Steps.Define);
    };

    // --- RENDER ---
    return (
        <div className="form">
            {step === Steps.Define && (
                <>
                    <h2 className="form__title">Create application</h2>

                    <BasicInfoStep
                        draft={draft}
                        setDraft={setDraft}
                        onNext={next}
                        onCancel={onCancel}
                        error={error}
                    />
                </>
            )}

            {step === Steps.Authorize && (
                <>
                    <h2 className="form__title">Authorize Machine to Machine Application</h2>

                    <PermissionsStep
                        draft={draft}
                        setDraft={setDraft}
                        onBack={back}
                        onSubmit={submit}
                        error={error}
                    />
                </>
            )}

            {step === Steps.Complete && result && (
                <>
                    <h2 className="form__title">Success!</h2>
                    <SecretReveal
                        secret={result.secret}
                        onContinue={() => onSuccess(result.id)}
                    />
                </>
            )}
        </div>
    );
}
