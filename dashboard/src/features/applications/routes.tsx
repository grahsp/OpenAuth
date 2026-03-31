import {Navigate, type RouteObject} from "react-router-dom";
import ApplicationListPage from "./pages/ApplicationListPage.tsx";
import ApiListPage from "./pages/ApiListPage.tsx";
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
    },
    {
        path: "apis",
        children: [
            { index: true, element: <ApiListPage /> }
        ]
    }
]

export const applicationTabs = [
    { label: "Settings", path: "settings" },
    { label: "APIs", path: "apis" },
];
