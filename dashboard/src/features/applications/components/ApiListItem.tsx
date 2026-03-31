import type {Api} from "../types.ts";
import {ResourceCopyField} from "./ResourceCopyField.tsx";
import "./ApiListItem.css";

export function ApiListItem({ api }: { api: Api }) {
    return (
        <li>
            <div className="resource-card resource-card--static">
                <div className="resource-card__primary">
                    <div className="resource-card__info">
                        <p className="resource-card__title">{api.name}</p>
                        <p className="resource-card__meta">{api.permissions.length} permissions</p>
                    </div>
                </div>
                <ResourceCopyField label="Audience" value={api.audience} />
            </div>
        </li>
    );
}
