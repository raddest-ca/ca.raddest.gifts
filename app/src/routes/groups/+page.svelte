<script lang="ts">
	import type { PageData } from "./$types";
    import { headers, loggedIn } from "../../stores/auth";
	import AuthRequired from "../../components/AuthRequired.svelte";

    interface Group {
        displayName: string;
        password: string;
        members: string[];
        owners: string[];
        id: string;
    }

    let groups: Group[] = [];
    let error = "";
    async function load() {
        const resp = await fetch(`${import.meta.env.VITE_API_URL}/groups`, {
            headers: {
                "Content-Type": "application/json",
                ...$headers,
            },
        });
        if (resp.ok) {
            groups = await resp.json();
            console.dir(groups);
        } else {
            error = resp.statusText;
        }
    }
    load();
    
    let groupInviteCode = "";
    let groupCreationDisplayName = "";
    let groupCreationPassword = "";

    async function joinGroup(inviteCode: string) {

    }

    async function createGroup(displayName: string, password: string) {
        const resp = await fetch(`${import.meta.env.VITE_API_URL}/groups`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                ...$headers,
            },
            body: JSON.stringify({
                GroupDisplayName: displayName,
                GroupPassword: password,
            }),
        });
        if (resp.ok) {
            await load();
        } else {
            error = resp.statusText;
        }
    }
</script>

<AuthRequired>

{#if error}
    <span class="text-xl">{error}</span>
{/if}

<section>
    <h1 class="text-xl font-bold">Join a group</h1>
    <hr class="my-1">
    <form>
        <label for="groupCode">Invite code: </label>
        <input type="text" name="groupCode" id="groupCode" placeholder="code" bind:value={groupInviteCode}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click={()=>joinGroup(groupInviteCode)}>Join</button>
    </form>
</section><section>
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
    {#if groups.length === 0}
        <span>You are in no groups.</span>
    {:else}
        <ul>
            {#each groups as group}
            <li><a class="underline" href="/groups/{group.id}">{group.displayName}</a></li>
            {/each}
        </ul>
    {/if}
</section>
</AuthRequired>
