import { apiFetch, assertAuth } from '../../api/client';
import type { Group } from '../../api/types';
import type { PageLoad } from './$types';

export const load: PageLoad = async () => {
    await assertAuth();
    return await apiFetch<Group[]>("/group");
};