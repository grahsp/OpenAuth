import type {Application} from "../types.ts";
import {Link} from "react-router-dom";
import "./ApplicationListItem.css"

export function ApplicationListItem({application}: { application: Application }) {
    return (
        <Link to={`/applications/${application.id}`}>
        <div className="app-card">
                <div className="app-card__info">
                    <p className="app-card__title">{application.name}</p>
                    <p className="app-card__type">Machine to Machine</p>
                </div>
                <div className="app-card__center">
                    <span className="label">Client ID: </span>
                    <span className="client-id">{application.id}</span>
                </div>
        </div>
        </Link>
    );
}