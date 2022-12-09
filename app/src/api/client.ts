import { invalidate } from "$app/navigation";
import { error } from "@sveltejs/kit";
import { auth, AuthData } from "../stores/auth";

export interface DetailedError {
	type: string;
	title: string;
	status: number;
	detail: string;
	traceId: string;
	errors: Record<string, string>;
}
export type SuccessResponse<T> = { data: T; ok: true };
export type FailureResponse = { errorMessage: string; ok: false };
export type ApiResponse<T> = SuccessResponse<T> | FailureResponse;


let $authData = new AuthData();
auth.subscribe(v => $authData = v);

export async function assertAuth(fetch: typeof window.fetch, returnUrl: string | URL) {
	if ($authData.expired && !(await refreshJwt(fetch))) {
		throw error(401, {
			message: "Not authenticated",
			returnUrl: returnUrl.toString(),
		});
	}
}

export async function refreshJwt(fetch: typeof window.fetch = window.fetch) {
	console.log("Refreshing JWT with refresh token");
	if ($authData.refreshToken === null) {
		console.log("refresh token not found uwu");
		return false;
	}
	const resp = await apiFetch<{ token: string }>(fetch, "/Token/Refresh", {
		method: "POST",
		body: JSON.stringify({
			refreshToken: $authData.refreshToken,
		}),
	});
    $authData.jwt = resp.token;
	auth.set($authData);
}

export async function apiFetch<T>(
	fetch: typeof window.fetch,
	path: string,
	options: RequestInit = {},
): Promise<T> {
	try {
		const resp = await fetch(`${import.meta.env.VITE_API_URL}${path}`, {
			...options,
			headers: {
				...$authData.headers,
				"Content-Type": "application/json",
				...options.headers,
			},
		});
		if (resp.ok) {
			return await resp.json()
		} else {
			throw await error(resp.status, await resp.text());
		}
	} catch (e) {
		throw await error(400, Object.prototype.toString.call(e));
	}
}

export async function apiInvalidate(path: string) {
	await invalidate(`${import.meta.env.VITE_API_URL}${path}`);
}
export function apiDepends(depends:(...deps: string[]) => void, ...deps: string[]) {
	for(const path of deps) {
		depends(`${import.meta.env.VITE_API_URL}${path}`);
	}
}




// // thank u copilot for helping with this lmao
// type MergedApiResponse<
// 	T extends Record<string, ApiResponse<T[keyof T] extends ApiResponse<infer V> ? V : never>>,
// > = ApiResponse<{
// 	[K in keyof T]: T[K] extends ApiResponse<infer V> ? V : never;
// }>;

// export function mergeResults<T extends Record<string, ApiResponse<unknown>>>(
// 	data: T,
// ): MergedApiResponse<T> {
// 	const errors = Object.values(data)
// 		.filter((x): x is FailureResponse<T> => !x.ok)
// 		.map((x) => x.errorMessage);
// 	if (errors.length > 0) {
// 		return { errorMessage: errors.join("; "), ok: false };
// 	}
// 	return {
// 		data: Object.entries(
// 			data as Record<keyof T, SuccessResponse<T[keyof T] extends ApiResponse<infer V> ? V : never>>,
// 		).reduce(
// 			(map, v) => {
// 				map[v[0] as keyof T] = v[1].data;
// 				return map;
// 			},
// 			{} as {
// 				[K in keyof T]: T[K] extends ApiResponse<infer V> ? V : never;
// 			},
// 		),
// 		ok: true,
// 	};
// }
