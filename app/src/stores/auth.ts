import { writable } from "svelte/store";
import { browser } from "$app/environment";
import jwt_decode from "jwt-decode";

export interface JWT {
    sub: string;
    jti: string;
    iat: string;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier': string;
    'http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name': string;
    exp: number;
    iss: string;
    aud: string;
}

const existingJwt = browser ? localStorage.getItem("auth") : null;
const existingJwtRefresh = browser ? localStorage.getItem("auth-refresh") : null;
export const jwt = writable<string | null>(existingJwt ?? null);
export const refreshToken = writable<string | null>(existingJwtRefresh ?? null);
export const jwtData = writable<JWT | null>(null);
export const loggedIn = writable<boolean>(false);
export const name = writable<string | null>(null);
export const headers = writable<Record<string, string>>({});


if (browser) {
    jwt.subscribe((value) => {
        if (value === null) {
            jwtData.set(null);
            loggedIn.set(false);
            name.set(null);
            headers.set({});
            localStorage.removeItem("auth");
        } else {
            const data = jwt_decode<JWT>(value);
            jwtData.set(data);
            loggedIn.set(true);
            name.set(data["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
            headers.set({
                "Authorization": `Bearer ${value}`,
            });
            localStorage.setItem("auth", value);
        }
    });
    refreshToken.subscribe((value) => {
        if (value === null) {
            localStorage.removeItem("auth-refresh");
        } else {
            localStorage.setItem("auth-refresh", value);
        }
    });
}
