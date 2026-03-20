import {Navigate, type RouteObject} from "react-router-dom";
import ApplicationListPage from "./pages/ApplicationListPage.tsx";
import ApplicationLayout from "./details/layout/ApplicationLayout.tsx";
import {ApplicationSettingsPage} from "./details/settings/ApplicationSettingsPage.tsx";

export const routes: RouteObject[] = [
    {
        path: "applications",
        children: [
            { index: true, element: <ApplicationListPage /> },
            { path: ":id", element: <ApplicationLayout />, children: [
                    { index: true, element: <Navigate to="settings" replace /> },
                    { path: "settings", element: <ApplicationSettingsPage /> }
                ]}
        ]
    }
]

export const applicationTabs = [
    { label: "Settings", path: "settings" },
];
