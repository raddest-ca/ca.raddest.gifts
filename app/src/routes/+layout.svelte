<script lang="ts">
	import { onDestroy } from "svelte";
    import "../app.css";
    import { name, loggedIn, jwtData, jwt } from "../stores/auth";
	import type { LayoutData } from "./$types";

    let remaining = 0;

    const interval = setInterval(()=>{
        if ($jwtData === null) return;
        remaining = $jwtData.exp - (Date.now()/1000);
        if (remaining <= 0) {
            jwt.set(null);
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
            <span class="p-4 inline-block float-right">Logged in as <a href="/account" class="underline">{$name}</a> ({remaining.toFixed(0)} remaining)</span>
        {:else}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/login">Login</a>
        {/if}
    </nav>
</header>

<div class="sm:w-full md:w-2/3 bg-amber-100 m-auto p-4">
    <slot />
</div>
