<script lang="ts">
	import { goto } from "$app/navigation";
	import { page } from "$app/stores";
	import { onDestroy } from "svelte";
	import { apiFetch, assertAuth } from "../api/client";
    import "../app.css";
    import { name, loggedIn, jwtData, jwt, refreshToken } from "../stores/auth";
	import type { LayoutData } from "./$types";

    let remaining = 0;

    async function refreshJwt() {
        console.log("Refreshing JWT with refresh token");
        const resp = await apiFetch<{token:string}>(fetch, "/Token/Refresh", {
            method: "POST",
            body: JSON.stringify({
                refreshToken: $refreshToken,
            }),
        });
        if (resp.ok) {
            jwt.set(resp.data.token);
            return true;
        } else {
            jwt.set(null);
            return false;
        }
    }
    let debounce = false; // idk if this is necessary
    const interval = setInterval(async ()=>{
        if ($jwtData === null) return;
        remaining = $jwtData.exp - (Date.now()/1000);
        if (remaining <= 0 && !debounce) {
            debounce = true;
            if (!await refreshJwt()) {
                // refresh failed, direct to login page
                await goto(`/login?returnUrl=${$page.url.pathname}`);
                debounce = false;
            }
        }
    }, 1000);

    onDestroy(()=>clearInterval(interval));
</script>
  
<header class="sm:w-full md:w-2/3 bg-slate-500 m-auto">
    <nav>
        <a class="p-4 text-xl font-serif" href="/">Gift</a>
        <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/">Home</a>
        {#if $loggedIn}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/groups/">My groups</a>
            <span class="p-4 inline-block float-right">Logged in as <a href="/account" class="underline">{$name}</a></span>
        {:else}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/login?returnUrl={$page.url.pathname}">Login</a>
            <span class="float-right inline-block p-4">or</span>
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/register">Register</a>
        {/if}
    </nav>
</header>

<div class="sm:w-full md:w-2/3 bg-amber-100 m-auto p-4">
    <slot />
</div>
