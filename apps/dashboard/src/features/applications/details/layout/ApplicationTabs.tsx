import {NavLink} from "react-router-dom";
import "./ApplicationTabs.css"
import {applicationTabs} from "../../routes.tsx";

type Props = {
    basePath: string;
};

export function ApplicationTabs({ basePath }: Props) {
    return (
        <nav className="tabs">
            {applicationTabs.map(tab => {
                const to = tab.path ? `${basePath}/${tab.path}` : basePath;

                return (
                    <NavLink
                        key={tab.label}
                        to={to}
                        end={tab.path === ""}
                        className={({ isActive }) =>
                            `tab ${isActive ? "tab--active" : ""}`
                        }
                    >
                        {tab.label}
                    </NavLink>
                );
            })}
        </nav>
    );
}