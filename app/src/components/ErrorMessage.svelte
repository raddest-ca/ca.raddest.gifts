<script lang="ts">
    export let error: string | null | undefined;
</script>

{#if error && error != ""}
    <div class="text-red-600 drop-shadow-md">
        {error}
    </div>
{/if}