import { error } from "@sveltejs/kit";
import { headers as headersStore, loggedIn as loggedInStore } from "../stores/auth";

export interface DetailedError {
	type: string;
	title: string;
	status: number;
	detail: string;
	traceId: string;
	errors: Record<string, string>;
}
export type SuccessResponse<T> = { data: T; ok: true };
export type FailureResponse<T> = { errorMessage: string; ok: false };
export type ApiResponse<T> = SuccessResponse<T> | FailureResponse<T>;

let authHeaders: Record<string, string> | null = null;
headersStore.subscribe((value) => {
	authHeaders = value;
});
let loggedIn = false;
loggedInStore.subscribe((value) => {
	loggedIn = value;
});

export async function assertAuth(returnUrl: string | URL) {
	if (!loggedIn) {
		throw error(401, {
			message: "Not authenticated",
			returnUrl: returnUrl.toString(),
		});
	}
}

export function mergeResults<V, T extends Record<string, ApiResponse<V>>>(
	data: T,
): ApiResponse<Record<keyof T, V>> {
	const errors = Object.values(data)
		.filter((x): x is FailureResponse<T> => !x.ok)
		.map((x) => x.errorMessage);
	if (errors.length > 0) {
		return { errorMessage: errors.join("; "), ok: false };
	}
	return {
		data: Object.entries(data as Record<keyof T, SuccessResponse<V>>).reduce((map, v) => {
			map[v[0] as keyof T] = v[1].data;
			return map;
		}, {} as Record<keyof T, V>),
		ok: true,
	};
}

export async function apiFetch<T>(
	fetch: typeof window.fetch,
	path: string,
	options: RequestInit = {},
): Promise<ApiResponse<T>> {
	try {
		const resp = await fetch(`${import.meta.env.VITE_API_URL}${path}`, {
			...options,
			headers: {
				...(authHeaders ?? {}),
				"Content-Type": "application/json",
				...options.headers,
			},
		});
		if (resp.ok) {
			return { data: await resp.json(), ok: true };
		} else {
			const text = await resp.text();
			try {
				const error: DetailedError = JSON.parse(text);
				return { errorMessage: `${error.title} - ${error.detail}`, ok: false };
			} catch (e) {
				return {
					errorMessage: `API error: ${resp.status} ${resp.statusText}${
						text != "" ? ` - ${text}` : ""
					}`,
					ok: false,
				};
			}
		}
	} catch (e) {
		return { errorMessage: `Unknown API error - ${e}`, ok: false };
	}
}
