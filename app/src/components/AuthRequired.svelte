<script lang="ts">
    import { loggedIn } from "../stores/auth";
</script>

{#if $loggedIn}
    <slot />
{:else}
    <div class="sm:w-full md:w-2/3 bg-amber-100 m-auto p-4">
        <h1 class="text-2xl font-serif">You must be logged in to view this page</h1>
        <p class="text-xl">Please <a href="/login" class="underline">login</a> to continue</p>
    </div>
{/if}