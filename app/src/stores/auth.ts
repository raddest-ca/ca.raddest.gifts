import { writable } from "svelte/store";
import { browser } from "$app/environment";
import jwt_decode from "jwt-decode";

export interface JWT {
	sub: string;
	jti: string;
	iat: string;
	"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier": string;
	"http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name": string;
	exp: number;
	iss: string;
	aud: string;
}

export class AuthData {
	jwt: string | null = null;
	refreshToken: string | null = null;

	get name(): string | null {
		return this.jwtData?.["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] ?? null;
	}

	get userId(): string | null {
		return this.jwtData?.sub ?? null;
	}

	get jwtData(): JWT | null {
		if (this.jwt == null) return null;
		try {
			return jwt_decode<JWT>(this.jwt);
		} catch (e) {
			return null;
		}
	}

	get secondsUntilExpiry(): number {
		if (!this.loggedIn) return 0;
		// eslint-disable-next-line @typescript-eslint/no-non-null-assertion
		return this.jwtData!.exp - Math.floor(Date.now() / 1000);
	}

	get expired(): boolean {
		return this.secondsUntilExpiry <= 0;
	}

	get loggedIn(): boolean {
		return this.jwt != null && this.jwtData != null;
	}

	get headers(): Record<string, string> {
		if (!this.loggedIn) return {};
		return {
			Authorization: `Bearer ${this.jwt}`,
		};
	}
}

let $data = new AuthData();
if (browser) {
	// load from local storage
	$data.jwt = localStorage.getItem("auth") ?? null;
	$data.refreshToken = localStorage.getItem("auth-refresh") ?? null;
}
export const auth = writable<AuthData>($data);
auth.subscribe((value) => {
    $data = value;
});
if (browser) {
	// persist to local storage
	auth.subscribe((value) => {
		localStorage.setItem("auth", value.jwt ?? "");
		localStorage.setItem("auth-refresh", value.refreshToken ?? "");
	});
}
