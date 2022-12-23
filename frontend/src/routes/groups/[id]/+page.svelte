<script lang="ts">
	import type { PageData } from "./$types";
	import { apiFetch } from "../../../api/client";
	import { goto, invalidateAll } from "$app/navigation";
	import { page } from "$app/stores";
    import SvelteMarkdown from "svelte-markdown";
	import { auth } from "../../../stores/auth";
    import WishlistComponent from "./WishlistComponent.svelte";

    export let data: PageData;

    function initFocus(el: any) {
        console.log("foc", el);
        el.focus();
    }

    $: console.log(data);
    $: groupId = $page.params.id;
    $: inviteCode = `${groupId}:${data.group.password}`;

    let groupDisplayNameInput = data.group.displayName;
    let showGroupDisplayNameEditor = false;

    let groupPasswordInput = data.group.password;

    let newWishlistName = "";
    async function createWishlist(displayName: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist`, {
            method: "POST",
            body: JSON.stringify({
                DisplayName: displayName,
            }),
        });
        await invalidateAll();
    }

    async function removeUserFromGroup(userId: string) {
        if (!window.confirm(`Are you sure you want to remove ${data.users[userId].displayName} from this group?`)) return;
        await apiFetch(fetch, `/group/${groupId}/member/${userId}`, {
            method: "DELETE",
        });
        if (userId == $auth.userId) {
            await goto("/groups");
        } else {
            await invalidateAll();
        }
    }

    async function setOwner(userId: string, owner: boolean) {
        await apiFetch(fetch, `/group/${groupId}/member/${userId}`, {
            method: "PATCH",
            body: JSON.stringify({
                IsOwner: owner,
            })
        });
        await invalidateAll();
    }

    async function deleteGroup() {
        if (!window.confirm(`Are you sure you want to delete this group?`)) return;
        if (!window.confirm(`Are you sure you're sure you want to delete this group?`)) return;
        await apiFetch(fetch, `/group/${groupId}`, {
            method: "DELETE",
        });
        await goto("/groups");
    }

    async function updateGroupDisplayName(displayName: string) {
        await apiFetch(fetch, `/group/${groupId}`, {
            method: "PATCH",
            body: JSON.stringify({
                DisplayName: displayName,
            })
        });
        showGroupDisplayNameEditor = false;
        await invalidateAll();
    }
    
    async function updateGroupPassword(password: string) {
        await apiFetch(fetch, `/group/${groupId}`, {
            method: "PATCH",
            body: JSON.stringify({
                Password: password,
            })
        });
        showGroupDisplayNameEditor = false;
        await invalidateAll();
    }
    
    $: canModifyGroup = data.group.owners.includes($auth.userId!);
</script>

<svelte:head>
    <title>{data.group.displayName} - Group</title>
</svelte:head>

{#if showGroupDisplayNameEditor}
    <form on:submit|preventDefault={()=>updateGroupDisplayName(groupDisplayNameInput)}>
        <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={groupDisplayNameInput} on:blur={()=>showGroupDisplayNameEditor = false} use:initFocus/>
    </form>
{:else}
    {#if canModifyGroup}
        <button type="button" on:click={()=>showGroupDisplayNameEditor=true}>
            <h1 class="text-2xl font-bold">Group - {data.group.displayName}</h1>
        </button>
    {:else}
        <h1 class="text-2xl font-bold">Group - {data.group.displayName}</h1>
    {/if}
{/if}
<button title="Refresh" type="button" class="p-0.5 hover:bg-slate-400 rounded-md absolute right-4 top-16" on:click={invalidateAll}>
    <i class="mi mi-refresh"><span class="u-sr-only">Refresh</span></i>
</button>
<hr class="my-1">

<p>Cards use <a class="underline text-blue-400" href="https://www.markdownguide.org/cheat-sheet/">markdown</a> syntax.</p>

<section>
    <h1 class="text-xl font-bold mt-4">Members</h1>
    <hr class="my-1">
    <div class="flex flex-wrap">
        {#each data.group.members as memberId}
            {@const member = data.users[memberId]}
            <div class="member-card bg-slate-300 m-4 p-4 w-64 flex-grow">
                <div class="flex flex-row">
                    <div class="relative flex-grow">
                        <h1 class="text-lg">{member.displayName}</h1>
                        <p class="text-sm">{`${memberId === $auth.userId ? "you, " : ""}${data.group.owners.includes(memberId) ? "owner" : "member"}`}</p>
                        <div class="member-actions absolute right-0 top-0">
                            {#if memberId === $auth.userId || canModifyGroup}
                                <button title="Remove from group" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>removeUserFromGroup(memberId)}>
                                    <i class="mi mi-circle-remove"><span class="u-sr-only">Remove from group</span></i>
                                </button>
                            {/if}
                            {#if canModifyGroup}
                                {#if data.group.owners.includes(memberId)}
                                    <button title="Demote" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>setOwner(memberId, false)}>
                                        <i class="mi mi-chevron-double-down"><span class="u-sr-only">Demote</span></i>
                                    </button>
                                {:else}
                                    <button title="Promote" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>setOwner(memberId, true)}>
                                        <i class="mi mi-chevron-double-up"><span class="u-sr-only">Promote</span></i>
                                    </button>
                                {/if}
                            {/if}
                        </div>
                    </div>
                </div>
            </div>
        {/each}
</section>
<section>
    <h1 class="text-xl font-bold mt-4">Wishlists</h1>
    <hr class="my-1">
    <div class="flex flex-wrap">
        {#each Object.values(data.wishlists) as wishlist}
            <WishlistComponent
                bind:group={data.group}
                bind:wishlist={wishlist}
                bind:users={data.users}
                bind:cards={data.cards[wishlist.id]}
            />
        {/each}
    </div>
</section>
<section>
    <h1 class="text-xl font-bold mt-4">Add new list</h1>
    <hr class="my-1">
    <form>
        <label for="newWishlistName">Display name: </label>
        <input type="text" name="newWishlistName" class="p-1 rounded-md shadow-lg" id="newWishlistName" placeholder="love da mets" bind:value={newWishlistName}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click|preventDefault={()=>createWishlist(newWishlistName)}>Create</button>
    </form>
</section>
<section>
    <h1 class="text-xl font-bold mt-4">Invitations</h1>
    <hr class="my-2">
    {#if canModifyGroup}
        <form class="my-2" on:submit|preventDefault={()=>updateGroupPassword(groupPasswordInput)}>
            <label for="groupPassword">Password: </label>
            <input id="groupPassword" class="p-1 rounded-md shadow-lg" placeholder="beans" bind:value={groupPasswordInput}/>
            <button class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" type="submit">Apply</button>
        </form>
    {/if}
    <span>Invite code: <input class="bg-gray-800 text-white p-2 rounded-md text-ellipsis" bind:value={inviteCode}/>
    <button title="Copy code" type="button" on:click={()=>navigator.clipboard.writeText(inviteCode)}>
        <i class="mi mi-clipboard"><span class="u-sr-only">Copy code</span></i>
    </button></span>
</section>
{#if canModifyGroup}
    <section>
        <h1 class="text-xl font-bold mt-4">Group actions</h1>
        <hr class="my-2">
        <button class="rounded-xl bg-red-200 p-2 drop-shadow-lg" on:click|preventDefault={()=>deleteGroup()}>Delete group</button>
    </section>
{/if}


<style>
    .member-card:not(:hover) .member-actions {
        display: none;
    }
</style>