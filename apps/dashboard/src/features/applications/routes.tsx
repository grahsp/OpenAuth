import { type RouteObject } from "react-router-dom";
import ApplicationListPage from "./pages/ApplicationListPage.tsx";
import ApplicationDetailsPage from "./pages/ApplicationDetailsPage.tsx";

export const routes: RouteObject[] = [
    {
        path: "applications",
        children: [
            { index: true, element: <ApplicationListPage /> },
            { path: ":id", element: <ApplicationDetailsPage />}
        ]
    }
]