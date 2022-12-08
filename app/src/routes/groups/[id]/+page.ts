import { apiFetch, assertAuth } from "../../../api/client";
import type { Group, Wishlist } from "../../../api/types";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ params, url }) => {
    await assertAuth(url);
	return await apiFetch<{
        group: Group,
        wishlists: Wishlist[],
    }>(fetch, `/group/${params.id}`);
};