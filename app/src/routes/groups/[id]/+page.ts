import { apiFetch, assertAuth } from "../../../api/client";
import type { Group, Wishlist, Card, User } from "../../../api/types";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, params, url }) => {
    await assertAuth(fetch, url);
    return await apiFetch<{
        group: Group;
        users: Record<string, User>;
        wishlists: Record<string, Wishlist>;
        cards: Record<string, Record<string, Card>>;
    }>(fetch, `/batch/group/${params.id}`);
};