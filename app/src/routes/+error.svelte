<script lang="ts">
	import { goto } from "$app/navigation";
    import { page } from "$app/stores";
	import { loggedIn } from "../stores/auth";

    $: returnUrl = $page?.error?.returnUrl ? new URL($page?.error?.returnUrl).pathname : "/";
    $: params = "returnUrl=" + returnUrl;

    $: if ($page.status === 401 && $loggedIn) {
        goto(returnUrl);
    }
</script>

{#if $page.status === 401}
    <div class="sm:w-full md:w-2/3 bg-amber-100 m-auto p-4">
        <h1 class="text-2xl font-serif">You must be logged in to view this page</h1>
        <p class="text-xl">Please <a href="/login?{params}" class="underline">login</a> to continue</p>
    </div>
{:else}
    <h1 class="text-xl font-bold">Error - {$page?.error?.message}</h1>
{/if}