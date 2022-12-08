<script lang="ts">
    import { page } from "$app/stores";

    $: params = $page?.error?.returnUrl ? "returnUrl=" + new URL($page?.error?.returnUrl).pathname : "";
</script>

{#if $page.status === 401}
    <div class="sm:w-full md:w-2/3 bg-amber-100 m-auto p-4">
        <h1 class="text-2xl font-serif">You must be logged in to view this page</h1>
        <p class="text-xl">Please <a href="/login?{params}" class="underline">login</a> to continue</p>
    </div>
{:else}
    <h1 class="text-xl font-bold">Error - {$page?.error?.message}</h1>
{/if}