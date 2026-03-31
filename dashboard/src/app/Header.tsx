import {NavLink} from "react-router-dom";
import "./Header.css";

const navigationItems = [
    { label: "Applications", to: "/applications" },
    { label: "APIs", to: "/apis" }
];

export default function Header() {
    return (
        <header className="dashboard-header">
            <nav className="dashboard-header__nav" aria-label="Primary">
                {navigationItems.map((item) => (
                    <NavLink
                        key={item.to}
                        to={item.to}
                        className={({ isActive }) =>
                            `dashboard-header__link${isActive ? " dashboard-header__link--active" : ""}`
                        }
                    >
                        {item.label}
                    </NavLink>
                ))}
            </nav>
        </header>
    );
}
