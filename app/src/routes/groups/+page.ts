import { apiFetch, assertAuth } from '../../api/client';
import type { Group } from '../../api/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({fetch, url}) => {
    await assertAuth(fetch, url);
    return {
        groups: await apiFetch<Group[]>(fetch, "/group"),
    };
};