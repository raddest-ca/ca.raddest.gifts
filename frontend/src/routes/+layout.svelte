<script lang="ts">
    import "../app.css";
	import { page } from "$app/stores";
	import { auth } from "../stores/auth";
	import { onDestroy } from "svelte";

    let remaining = 0;
    const interval = setInterval(async ()=>{
        remaining = $auth.secondsUntilExpiry;
    }, 1000);

    onDestroy(()=>clearInterval(interval));

</script>

<svelte:head>
	<link rel="stylesheet" href="https://unpkg.com/mono-icons@1.0.5/iconfont/icons.css" >
</svelte:head>
  
<header class="sm:w-full lg:w-2/3 bg-slate-500 m-auto">
    <nav>
        <a class="p-4 text-xl font-serif" href="/">Gift</a>
        <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/">Home</a>
        {#if $auth.loggedIn}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/groups/">My groups</a>
            <span class="p-4 inline-block float-right">Logged in as <a href="/account" class="underline">{$auth.name}</a> ({remaining.toFixed(0)} remaining)</span>
        {:else}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/login?returnUrl={$page.url.pathname}">Login</a>
            <span class="float-right inline-block p-4">or</span>
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/register">Register</a>
        {/if}
    </nav>
</header>

<div class="sm:w-full lg:w-2/3 bg-amber-100 m-auto p-4">
    <slot />
</div>