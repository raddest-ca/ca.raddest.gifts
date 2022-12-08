import { apiFetch, assertAuth, mergeResults } from "../../../api/client";
import type { Group, Wishlist, Card } from "../../../api/types";
import type { PageLoad } from "./$types";

export const load: PageLoad = async ({ fetch, params, url }) => {
    await assertAuth(url);
    const main = mergeResults({
        group: await apiFetch<Group>(fetch, `/group/${params.id}`),
        wishlists: await apiFetch<Wishlist[]>(fetch, `/group/${params.id}/wishlist`),
    });
    if (!main.ok) return main;
    
    const rtn = {
        group: main.data.group,
        wishlists: main.data.wishlists,
        cards: {} as Record<string, Card[]>
    };
    
    for (const wishlist of main.data.wishlists) {
        const resp = await apiFetch<Card[]>(fetch, `/group/${params.id}/wishlist/${wishlist.id}/card`);
        if (!resp.ok) return resp;
        rtn.cards[wishlist.id] = resp.data;
    }

    return {
        ok: true, 
        data: rtn,
    };
};