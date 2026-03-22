import {Navigate, type RouteObject} from "react-router-dom";
import ApplicationListPage from "./pages/ApplicationListPage.tsx";
import ApplicationLayout from "./details/layout/ApplicationLayout.tsx";
import {ApplicationSettingsPage} from "./details/settings/ApplicationSettingsPage.tsx";
import {ApplicationApisPage} from "./details/apis/ApplicationApisPage.tsx";

export const routes: RouteObject[] = [
    {
        path: "applications",
        children: [
            { index: true, element: <ApplicationListPage /> },
            { path: ":id", element: <ApplicationLayout />, children: [
                    { index: true, element: <Navigate to="settings" replace /> },
                    { path: "settings", element: <ApplicationSettingsPage /> },
                    { path: "apis", element: <ApplicationApisPage /> }
                ]}
        ]
    }
]

export const applicationTabs = [
    { label: "Settings", path: "settings" },
    { label: "APIs", path: "apis" },
];
