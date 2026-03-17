export default function ApplicationTypeCard(
    {
        title,
        description,
        selected,
        onClick,
    }: {
        title: string;
        description: string;
        selected: boolean;
        onClick?: () => void;
    }) {
    return (
        <div
            className={`type-card ${
                selected ? "selected" : ""
            }`}
            onClick={onClick}
        >
            <div className="type-card__title">
                {title}
            </div>
            <div className="type-card__description">
                {description}
            </div>
        </div>
    );
}