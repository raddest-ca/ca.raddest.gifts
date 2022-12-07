import { assertAuth } from '../../api/client';
import type { PageLoad } from './$types';

export const load: PageLoad = async () => {
    await assertAuth();
};