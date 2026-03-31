import {ApplicationListItem} from "../components/ApplicationListItem.tsx";
import {useApplications} from "../hooks/useApplications.tsx";
import {CreateApplicationTrigger} from "../create";
import {ListPage} from "../../../shared/pages/ListPage.tsx";

export default function ApplicationListPage() {
    const { data, loading, error } = useApplications();

    return (
        <ListPage
            title={"Applications"}
            description={"Configure clients, credentials, and downstream API access from one place."}
            items={data}
            loading={loading}
            error={error}
            action={<CreateApplicationTrigger />}
            emptyState={"No applications have been created yet."}
            renderItem={(item) =>
                <ApplicationListItem key={item.id} application={item} />
            }
        />
    );
}
