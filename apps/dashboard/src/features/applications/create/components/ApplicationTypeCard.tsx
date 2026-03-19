import "./ApplicationTypeCard.css"
import type {ApplicationType} from "../../types.ts";

export function ApplicationTypeSelector(
    {
        value,
        onChange
    }: {
        value: ApplicationType;
        onChange: (t: ApplicationType) => void;
    }) {
    return (
        <div className="type-selector">
            <ApplicationTypeCard
                selected={value === "spa"}
                title="Single Page Application"
                description="A JavaScript front-end app"
                onClick={() => onChange("spa")}
            />

            <ApplicationTypeCard
                selected={value === "web"}
                title="Regular Web Application"
                description="Traditional web app using redirects."
                onClick={() => onChange("web")}
            />

            <ApplicationTypeCard
                selected={value === "m2m"}
                title="Machine to Machine"
                description="Backend communication"
                onClick={() => onChange("m2m")}
            />
        </div>
    );
}

export function ApplicationTypeCard(
    {
        title,
        description,
        selected,
        onClick,
        className
    }: Props) {
    return (
        <button
            onClick={onClick}
            className={[
                "type-card",
                selected && "type-card--selected",
                className
            ]
                .filter(Boolean)
                .join(" ")}
        >
            <div className="type-card__icon" />

            <div className="type-card__title">
                {title}
            </div>

            <div className="type-card__description">
                {description}
            </div>
        </button>
    );
}

type Props = {
    title: string;
    description: string;
    selected: boolean;
    onClick: () => void;
    className?: string;
};