import {Outlet} from "react-router-dom";
import Header from "./Header.tsx";

const DashboardLayout = () => {
    return (
        <div>
            <Header />
            <main>
                <Outlet />
            </main>
        </div>
    )
}

export default DashboardLayout;