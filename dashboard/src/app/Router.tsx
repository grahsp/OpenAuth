import { Navigate, createBrowserRouter} from "react-router-dom";
import {routes as applicationRoutes} from "../features/applications/routes.tsx";
import {routes as apiRoutes} from "../features/apis/routes.tsx";
import DashboardLayout from "./DashboardLayout.tsx";

export const router = createBrowserRouter([
    {
        element: <DashboardLayout />,
        children: [
            {
                index: true,
                element: <Navigate to="/applications" replace />
            },
            ...applicationRoutes,
            ...apiRoutes
        ]
    }
])
