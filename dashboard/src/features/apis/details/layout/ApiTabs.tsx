import {NavLink} from "react-router-dom";
import "./ApiTabs.css";
import {apiTabs} from "../../routes.tsx";

type Props = {
    basePath: string;
};

export function ApiTabs({ basePath }: Props) {
    return (
        <nav className="tabs">
            {apiTabs.map((tab) => {
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
