import {ApplicationListItem} from "../components/ApplicationListItem.tsx";
import {useApplications} from "../hooks/useApplications.tsx";
import {CreateApplicationTrigger} from "../create";
import {ListPage} from "../../../shared/pages/ListPage.tsx";

export default function ApplicationListPage() {
    const { data, loading, error } = useApplications();

    return (
        <ListPage
            title={"Applications"}
            items={data}
            loading={loading}
            error={error}
            create={<CreateApplicationTrigger />}
            renderItem={(item) =>
                <ApplicationListItem key={item.id} application={item} />
            }
        />
    );
}
