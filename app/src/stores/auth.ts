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

const existing = browser ? localStorage.getItem("auth") : null;
export const jwt = writable<string | null>(existing ?? null);
export const jwtData = writable<JWT | null>(null);
export const loggedIn = writable<boolean>(false);
export const name = writable<string | null>(null);
jwt.subscribe((value) => {
    console.log(`current jwt: ${value}`)
    if (value === null) {
        jwtData.set(null);
        loggedIn.set(false);
        name.set(null);
    } else {
        const data = jwt_decode<JWT>(value);
        jwtData.set(data);
        loggedIn.set(true);
        name.set(data["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"]);
    }
});

if (browser) {
    jwt.subscribe((value) => {
        if (value === null) {
            localStorage.removeItem("auth");
        } else {
            localStorage.setItem("auth", value);
        }
    });
}
