<script lang="ts">
	import ContentWaiter from "../../../components/ContentWaiter.svelte";

    import { apiFetch } from "../../../api/client";
	import type { Group } from "../../../api/types";
	import type { PageData } from "./$types";
	import ErrorMessage from "../../../components/ErrorMessage.svelte";
	import AuthRequired from "../../../components/AuthRequired.svelte";

    export let data: PageData;
    $: error = data?.errorMessage;
    $: console.log(data);
</script>

<AuthRequired>
    <ErrorMessage {error}/>

    {#if data.ok}
    <span>{data.data.group.displayName}</span>
    {/if}
</AuthRequired>
