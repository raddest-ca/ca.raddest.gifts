<script lang="ts">
	import { goto } from "$app/navigation";
	import { page } from "$app/stores";
	import { apiFetch } from "../../api/client";
	import { auth } from "../../stores/auth";
    let username = "";
    let password = "";

    const returnUrl = $page.url.searchParams.get("returnUrl") ?? "/";

    async function login() {
        try {
            const resp = await apiFetch<{token:string; refreshToken: string}>(fetch, "/Token", {
                method: "POST",
                body: JSON.stringify({
                    UserLoginName: username,
                    UserPassword: password,
                }),
            });
            $auth.jwt = resp.token;
            $auth.refreshToken = resp.refreshToken;
            auth.set($auth);
            await goto(returnUrl);
        } catch (e: any) {
            alert(e.body.title + " -- " + e.body.detail);
        }
    }

    // If we're already logged in, redirect to the return URL
    if ($auth.loggedIn) {
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
        <button class="rounded-xl bg-slate-200 p-2 drop-shadow-lg m-2" type="submit">Login</button>
    </form>
</main>