import {useApis} from "../../applications/hooks.tsx";
import {ApiListItem} from "../../applications/components/ApiListItem.tsx";
import {ListPage} from "../../../shared/pages/ListPage.tsx";

export default function ApiListPage() {
    const { data, loading, error } = useApis();

    return (
        <ListPage
            title="APIs"
            description="Review audiences and permissions exposed by your resource servers."
            items={data}
            loading={loading}
            error={error}
            emptyState="No APIs have been created yet."
            renderItem={(item) => (
                <ApiListItem key={item.id} api={item} />
            )}
        />
    );
}
