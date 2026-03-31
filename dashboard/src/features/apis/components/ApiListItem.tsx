import {Link} from "react-router-dom";
import type {Api} from "../types.ts";
import {ResourceCopyField} from "../../../shared/components/ResourceCopyField.tsx";
import "./ApiListItem.css";

export function ApiListItem({ api }: { api: Api }) {
    return (
        <li>
            <div className="resource-card resource-card--interactive">
                <Link to={`/apis/${api.id}`} className="resource-card__primary">
                    <div className="resource-card__info">
                        <p className="resource-card__title">{api.name}</p>
                        <p className="resource-card__meta">{api.permissions.length} permissions</p>
                    </div>
                </Link>
                <div className="resource-card__copy">
                    <ResourceCopyField label="Audience" value={api.audience} />
                </div>
            </div>
        </li>
    );
}
