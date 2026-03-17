import {Outlet} from "react-router-dom";
import Header from "./Header.tsx";

const DashboardLayout = () => {
    return (
        <div>
            <Header />
            <main>
                <h2>Main Content</h2>
                <Outlet />
            </main>
        </div>
    )
}

export default DashboardLayout;