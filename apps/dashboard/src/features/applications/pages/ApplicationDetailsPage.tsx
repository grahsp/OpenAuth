import { useParams } from "react-router-dom";
import {useApplication} from "../hooks.tsx";

export default function ApplicationDetailsPage() {
    const { id } = useParams();
    const { data, loading, error } = useApplication(id);

    if (loading) return <p>Loading...</p>;
    if (error) return <p>{error}</p>

    return (
        <div>
            <h1>Application Details</h1>
            <h1>{data.name}</h1>
            <p>ID: {data.id}</p>
        </div>
    );
}