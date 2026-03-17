import {Link} from "react-router-dom";
import {useApplications} from "../hooks.tsx";

export default function ApplicationsListPage() {
    const { data, loading, error } = useApplications();

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <h1>Applications</h1>

            <ul>
                {data.map(app => (
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