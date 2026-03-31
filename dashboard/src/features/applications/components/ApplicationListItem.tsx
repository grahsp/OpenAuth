import type {Application} from "../types.ts";
import {Link} from "react-router-dom";
import {ResourceCopyField} from "../../../shared/components/ResourceCopyField.tsx";
import "./ApplicationListItem.css"

export function ApplicationListItem({application}: { application: Application }) {
    return (
        <li>
            <div className="resource-card resource-card--interactive">
                <Link to={`/applications/${application.id}`} className="resource-card__primary">
                    <div className="resource-card__info">
                        <p className="resource-card__title">{application.name}</p>
                        <p className="resource-card__meta">{getApplicationTypeLabel(application.applicationType)}</p>
                    </div>
                </Link>
                <div className="resource-card__copy">
                    <ResourceCopyField label="Client ID" value={application.id} />
                </div>
            </div>
        </li>
    );
}

function getApplicationTypeLabel(type: Application["applicationType"]) {
    switch (type) {
        case "m2m":
            return "Machine to Machine";
        case "spa":
            return "Single Page App";
        case "web":
            return "Regular Web App";
        default:
            return type;
    }
}
