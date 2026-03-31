import {NavLink} from "react-router-dom";
import "./DetailTabs.css";

export type DetailTab = {
    label: string;
    path: string;
};

type Props = {
    basePath: string;
    tabs: DetailTab[];
};

export function DetailTabs({ basePath, tabs }: Props) {
    return (
        <nav className="tabs">
            {tabs.map((tab) => {
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
