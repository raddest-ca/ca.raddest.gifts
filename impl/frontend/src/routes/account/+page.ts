import { assertAuth } from '../../api/client';
import type { PageLoad } from './$types';

export const load: PageLoad = async ({fetch, url}) => {
    await assertAuth(fetch, url);
};