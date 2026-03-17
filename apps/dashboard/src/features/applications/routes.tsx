import { type RouteObject } from "react-router-dom";
import ApplicationsListPage from "./pages/ApplicationsListPage.tsx";
import ApplicationDetailsPage from "./pages/ApplicationDetailsPage.tsx";

export const routes: RouteObject[] = [
    {
        path: "applications",
        children: [
            { index: true, element: <ApplicationsListPage /> },
            { path: ":id", element: <ApplicationDetailsPage />}
        ]
    }
]