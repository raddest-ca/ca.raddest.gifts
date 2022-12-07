<script lang="ts">
	import type { PageData } from "./$types";
	import ErrorMessage from "../../../components/ErrorMessage.svelte";

    export let data: PageData;
    $: error = data?.errorMessage;
    $: console.log(data);
</script>

<ErrorMessage {error}/>

{#if data.ok}
<span>{data.data.group.displayName}</span>
{/if}
