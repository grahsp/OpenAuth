import * as React from "react";
import "./ListPage.css";

type ListPageProps<T> = {
    title: string
    description?: string
    items?: T[]
    loading: boolean
    error?: string
    action?: React.ReactNode
    emptyState?: React.ReactNode
    renderItem: (item: T) => React.ReactNode
}

export function ListPage<T>(
    {
        title,
        description,
        items,
        loading,
        error,
        action,
        emptyState,
        renderItem
    }: ListPageProps<T>) {
    const hasItems = (items?.length ?? 0) > 0;

    return (
        <section className="list-page">
            <div className="list-page__hero">
                <div className="list-page__header">
                    <div className="list-page__title-row">
                        <h1>{title}</h1>
                        {action && <div className="list-page__action">{action}</div>}
                    </div>
                    {description && <p className="list-page__description">{description}</p>}
                </div>
            </div>

            {loading ? (
                <div className="list-page__state">Loading...</div>
            ) : error ? (
                <div className="list-page__state list-page__state--error">{error}</div>
            ) : hasItems ? (
                <ul className="list-page__list">
                    {items?.map(renderItem)}
                </ul>
            ) : (
                <div className="list-page__state">
                    {emptyState ?? "No items found."}
                </div>
            )}
        </section>
    )
}
