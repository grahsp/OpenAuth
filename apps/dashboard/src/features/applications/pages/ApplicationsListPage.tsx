import {Link} from "react-router-dom";

export default function ApplicationsListPage() {
    const applications = [
        { id: 1, name: "App A" },
        { id: 2, name: "App B" },
    ];

    return (
        <div>
            <h1>Applications</h1>

            <ul>
                {applications.map(app => (
                    <li key={app.id}>
                        <Link to={`/applications/${app.id}`}>
                            {app.name}
                        </Link>
                    </li>
                ))}
            </ul>
        </div>
    );
}