import { apiFetch, assertAuth, mergeResults } from "../../../api/client";
import type { Group, Wishlist } from "../../../api/types";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, params, url }) => {
    await assertAuth(url);
    return mergeResults({
        group: await apiFetch<Group>(fetch, `/group/${params.id}`),
        wishlists: await apiFetch<Wishlist[]>(fetch, `/group/${params.id}/wishlist`),
    })
};