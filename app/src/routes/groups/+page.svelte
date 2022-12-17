<script lang="ts">
	import type { PageData } from "./$types";
    import { apiFetch, apiInvalidate } from "../../api/client";
	import { goto, invalidateAll } from "$app/navigation";
    import { auth } from "../../stores/auth";

    export let data: PageData;
    $: console.log(data);
    let groupInviteCode = "";
    let groupCreationDisplayName = "";
    let groupCreationPassword = "";

    async function joinGroup(inviteCode: string) {
        const [groupId, password] = inviteCode.split(":");
        await apiFetch(fetch, `/group/${groupId}/join`, {
            method: "POST",
            body: JSON.stringify({
                UserId: $auth.userId,
                GroupPassword: password,
            }),
        });
        await goto(`/groups/${groupId}`);
    }

    async function createGroup(displayName: string, password: string) {
        const resp = await apiFetch<{id: string}>(fetch, "/group", {
            method: "POST",
            body: JSON.stringify({
                GroupDisplayName: displayName,
                GroupPassword: password,
            }),
        });
        await goto(`/groups/${resp.id}`);
    }
</script>

<svelte:head>
    <title>Groups</title>
</svelte:head>

<section>
    <h1 class="text-xl font-bold">Join a group</h1>
    <hr class="my-1">
    <form>
        <label for="groupCode">Invite code: </label>
        <input type="text" name="groupCode" id="groupCode" placeholder="code" bind:value={groupInviteCode}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click={()=>joinGroup(groupInviteCode)}>Join</button>
    </form>
</section>
<section>
    <h1 class="text-xl font-bold">Create a group</h1>
    <hr class="my-1">
    <form>
        <label for="groupDisplayName">Display name: </label>
        <input type="text" name="groupDisplayName" id="groupDisplayName" placeholder="love da mets" bind:value={groupCreationDisplayName}>
        <label for="groupPassword">Password: </label>
        <input type="text" name="groupPassword" id="groupPassword" placeholder="beans" bind:value={groupCreationPassword}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click={()=>createGroup(groupCreationDisplayName, groupCreationPassword)}>Create</button>
    </form>
</section>
<section>
    <h1 class="text-xl font-bold">My Groups</h1>
    <hr>
    {#if data.groups.length === 0}
        <span>You are in no groups.</span>
    {:else}
        <ul>
            {#each data.groups as group}
            <li><a class="underline" href="/groups/{group.id}">{group.displayName}</a></li>
            {/each}
        </ul>
    {/if}
</section>