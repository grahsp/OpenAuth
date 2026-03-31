import type {Application} from "../types.ts";
import {Link} from "react-router-dom";
import {ResourceCopyField} from "./ResourceCopyField.tsx";
import "./ApplicationListItem.css"

export function ApplicationListItem({application}: { application: Application }) {
    console.log(application);
    return (
        <li>
            <div className="resource-card resource-card--interactive">
                <Link to={`/applications/${application.id}`} className="resource-card__primary">
                    <div className="resource-card__info">
                        <p className="resource-card__title">{application.name}</p>
                        <p className="resource-card__meta">{getApplicationTypeLabel(application.applicationType)}</p>
                    </div>
                </Link>
                <ResourceCopyField label="Client ID" value={application.id} />
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
