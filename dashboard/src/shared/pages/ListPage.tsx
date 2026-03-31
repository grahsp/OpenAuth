import * as React from "react";

type ListPageProps<T> = {
    title: string
    items?: T[]
    loading: boolean
    error?: string
    create?: React.ReactNode
    renderItem: (item: T) => React.ReactNode
}

export function ListPage<T>(
    {
        title,
        items,
        loading,
        error,
        create,
        renderItem
    }: ListPageProps<T>) {

    if (loading) return <p>Loading...</p>
    if (error) return <p>{error}</p>

    return (
        <div>
            <h1>{title}</h1>

            {create}

            <ul>
                {items?.map(renderItem)}
            </ul>
        </div>
    )
}