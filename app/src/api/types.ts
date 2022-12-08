export interface Group {
	id: string;
	displayName: string;
	password: string;
	members: string[];
	owners: string[];
}

export interface Wishlist {
	id: string;
	displayName: string;
}

declare global {
    // eslint-disable-next-line @typescript-eslint/no-namespace
	namespace App {
		export interface Error {
			returnUrl?: string;
		}
	}
}
