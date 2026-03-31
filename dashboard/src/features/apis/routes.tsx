import {Navigate, type RouteObject} from "react-router-dom";
import ApiListPage from "./pages/ApiListPage.tsx";
import ApiLayout from "./details/layout/ApiLayout.tsx";
import {ApiSettingsPage} from "./details/settings/ApiSettingsPage.tsx";
import {ApiPermissionsPage} from "./details/permissions/ApiPermissionsPage.tsx";

export const routes: RouteObject[] = [
    {
        path: "apis",
        children: [
            { index: true, element: <ApiListPage /> },
            {
                path: ":id",
                element: <ApiLayout />,
                children: [
                    { index: true, element: <Navigate to="settings" replace /> },
                    { path: "settings", element: <ApiSettingsPage /> },
                    { path: "permissions", element: <ApiPermissionsPage /> }
                ]
            }
        ]
    }
];

export const apiTabs = [
    { label: "Settings", path: "settings" },
    { label: "Permissions", path: "permissions" }
];
