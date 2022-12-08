<script lang="ts">
	import { goto } from "$app/navigation";
	import { page } from "$app/stores";
	import { apiFetch } from "../../api/client";
	import ErrorMessage from "../../components/ErrorMessage.svelte";
    import { jwt, loggedIn, refreshToken } from "../../stores/auth"

    let username = "";
    let password = "";
    let error:string|null=null;

    const returnUrl = $page.url.searchParams.get("returnUrl") ?? "/";

    async function login() {
        error = "";
        const resp = await apiFetch<{token:string; refreshToken: string}>(fetch, "/Token", {
            method: "POST",
            body: JSON.stringify({
                UserLoginName: username,
                UserPassword: password,
            }),
        });
        if (resp.ok) {
            jwt.set(resp.data.token);
            refreshToken.set(resp.data.refreshToken);
            await goto(returnUrl);
        } else {
            error=resp.errorMessage;
        }
    }

    // If we're already logged in, redirect to the return URL
    if ($loggedIn) {
        goto(returnUrl);
    }
</script>

<main>
    <h1 class="text-xl font-bold">Login</h1>
    <hr class="my-2">
    <form on:submit|preventDefault={login}>
        <div>
            <div class="mb-2">
                <label class="w-32 inline-block" for="username">Username</label>
                <input type="text" name="username" id="username" bind:value={username} />
            </div>
            <div class="mb-2">
                <label class="w-32 inline-block" for="password">Password</label>
                <input type="password" name="password" id="password" bind:value={password} />
            </div>
        </div>
        <ErrorMessage {error} />
        <button class="rounded-xl bg-slate-200 p-2 drop-shadow-lg m-2" type="submit">Login</button>
    </form>
</main>