<script lang="ts">
    export let ready: boolean;
</script>

{#if ready}
    <slot/>
{:else}
    <div class="flex flex-col items-center justify-center h-full">
        <div class="animate-spin rounded-full h-32 w-32 border-t-2 border-b-2 border-gray-900"></div>
        <div class="text-2xl font-bold">Loading...</div>
    </div>
{/if}