import { useParams } from "react-router-dom";
export default function ApplicationDetailsPage() {
    const { id } = useParams();

    return (
        <div>
            <h1>Application Details</h1>
            <p>ID: {id}</p>
        </div>
    );
}