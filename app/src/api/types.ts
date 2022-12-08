export interface Group {
	displayName: string;
	password: string;
	members: string[];
	owners: string[];
	id: string;
}

declare global {
    // eslint-disable-next-line @typescript-eslint/no-namespace
	namespace App {
		export interface Error {
			returnUrl?: string;
		}
	}
}
