<script lang="ts">
    import "../app.css";
    import { name, loggedIn } from "../stores/auth";
	import type { LayoutData } from "./$types";

</script>
  
<header class="sm:w-full md:w-2/3 bg-slate-500 m-auto">
    <nav>
        <a class="p-4 text-xl font-serif" href="/">Gift</a>
        <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/">Home</a>
        {#if $loggedIn}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/groups/">My groups</a>
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block" href="/groups/join">Join a group</a>
            <span class="p-4 inline-block float-right">Logged in as <a href="/account" class="underline">{$name}</a> </span>
        {:else}
            <a class="p-4 hover:bg-slate-700 border-b-4 border-b-transparent hover:border-b-blue-300 underline inline-block float-right" href="/login">Login</a>
        {/if}
    </nav>
</header>

<div class="sm:w-full md:w-2/3 bg-amber-100 m-auto h-48 p-4">
    <slot />
</div>
