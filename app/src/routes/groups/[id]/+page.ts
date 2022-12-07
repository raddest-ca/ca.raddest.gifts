import { apiFetch } from "../../../api/client";
import type { Group } from "../../../api/types";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params }) => {
	const group = await apiFetch<{
        group: Group,
        cards: any,
    }>(`/group/${params.id}`)
	return group;
};