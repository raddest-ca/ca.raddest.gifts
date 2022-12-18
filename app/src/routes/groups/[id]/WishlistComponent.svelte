<script lang="ts">
	import { invalidateAll } from "$app/navigation";
	import { apiFetch } from "../../../api/client";
	import type { Card, Group, User, Wishlist } from "../../../api/types";
	import { auth } from "../../../stores/auth";
    import CardComponent from "./CardComponent.svelte";

    function initFocus(el: any) {
        console.log("foc", el);
        el.focus();
    }
    function overrideShiftEnter(e: KeyboardEvent, callback: () => void) {
        if (e.key === "Enter" && !e.shiftKey) {
            e.preventDefault();
            callback();
        }
    }

    async function deleteWishlist() {
        if (!window.confirm(`Are you sure you want to delete this wishlist?`)) return;
        await apiFetch(fetch, `/group/${group.id}/wishlist/${wishlist.id}`, {
            method: "DELETE",
        });
        await invalidateAll();
    }

    async function addWishlistOwner() {
        await apiFetch(fetch, `/group/${group.id}/wishlist/${wishlist.id}`, {
            method: "PATCH",
            body: JSON.stringify({
                Owners: wishlist.owners.concat(addOwnerInput),
            }),
        });
        showOwnerEditor = false;
        await invalidateAll();
    }
    async function addCard(content: string) {
        if (content.trim().length === 0) return;
        await apiFetch<Card>(fetch, `/group/${group.id}/wishlist/${wishlist.id}/card`, {
            method: "POST",
            body: JSON.stringify({
                Content: content,
                VisibleToListOwners: canModifyWishlist,
            }),
        });
        showAddCardEditor = false;
        addCardContentInput = "";
        await invalidateAll();
    }
    
    async function removeWishlistOwner(ownerId: string) {
        await apiFetch(fetch, `/group/${group.id}/wishlist/${wishlist.id}`, {
            method: "PATCH",
            body: JSON.stringify({
                Owners: wishlist.owners.filter(id => id !== ownerId),
            }),
        });
        await invalidateAll();
    }

    async function updateDisplayName() {
        await apiFetch(fetch, `/group/${group.id}/wishlist/${wishlist.id}`, {
            method: "PATCH",
            body: JSON.stringify({
                DisplayName: displayNameInput,
            }),
        });
        showDisplayNameEditor = false;
        await invalidateAll();
    }
    
    export let group: Group;
    export let wishlist: Wishlist;
    export let cards: Record<string, Card>;
    export let users: Record<string, User>;

    let showOwnerEditor = false;
    let showAddCardEditor = false;
    let showDisplayNameEditor = false;
    let addCardContentInput = "";
    let addOwnerInput = "";
    let displayNameInput = wishlist.displayName;
    $: canModifyGroup = group.owners.includes($auth.userId!);
    $: canModifyWishlist = canModifyGroup || wishlist.owners.includes($auth.userId!)
</script>

<div class="bg-slate-300 m-4 p-4 w-64 flex-grow">
    <!-- wishlist title -->
    <div>
        {#if canModifyWishlist}
            {#if showDisplayNameEditor}
                <form on:submit|preventDefault={updateDisplayName}>
                    <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={displayNameInput} on:blur={()=>showDisplayNameEditor = false} use:initFocus/>
                </form>
            {:else}
                <button on:click={()=>showDisplayNameEditor = true}><h1>{wishlist.displayName.trim() === "" ? "untitled wishlist" : wishlist.displayName}</h1></button>
                {#if canModifyWishlist}
                    <button title="Remove from group" type="button" class="p-0.5 float-right hover:bg-slate-400 rounded-md" on:click={deleteWishlist}>
                        <i class="mi mi-circle-remove"><span class="u-sr-only">Remove from group</span></i>
                    </button>
                {/if}
            {/if}
        {:else}
            <h1>{wishlist.displayName.trim() === "" ? "untitled wishlist" : wishlist.displayName}</h1>
        {/if}
    </div>
    <hr>
    <!-- cards -->
    <ul>
        {#each Object.values(cards) as card}
            <CardComponent bind:card={card} bind:wishlist={wishlist}/>
        {/each}
    </ul>
    <!-- add card -->
    {#if showAddCardEditor}
        <div>
            <form on:submit|preventDefault={()=>addCard(addCardContentInput)}>
                <textarea
                    class="mt-1 p-1 rounded-md shadow-lg"
                    placeholder="beans"
                    bind:value={addCardContentInput}
                    on:blur={()=>showAddCardEditor = false}
                    on:keydown={e => overrideShiftEnter(e, ()=>addCard(addCardContentInput))}
                    use:initFocus
                />
                <button type="submit" class="hidden">Submit</button>
            </form>

        </div>
    {/if}
    <button type="button" class="text-slate-400 hover:bg-slate-500 hover:text-gray-700 p-1 mt-1 rounded-sm" on:click={()=>showAddCardEditor=true}>Add a card</button>
    
    <!-- manage owners -->
    {#if canModifyWishlist}
        <button class="text-sm text-slate-400 hover:underline" on:click={()=>showOwnerEditor=!showOwnerEditor}>owned by {wishlist.owners.map(id => id == $auth.userId ? "you" : users[id].displayName).join(" and ")}</button>
        {#if showOwnerEditor}
            <div>
                <form on:submit|preventDefault={()=>addCard(addCardContentInput)}>
                    <select class="p-1 rounded-md shadow-lg w-1/2" bind:value={addOwnerInput} use:initFocus>
                        {#each Object.values(users) as user}
                            <option value={user.id}>{user.displayName}</option>
                        {/each}
                    </select>
                    <button type="button" class=" rounded-md p-1 bg-slate-200 hover:bg-slate-400" on:click={addWishlistOwner}>Add owner</button>
                </form>
                <ul>
                    {#each wishlist.owners as ownerId}
                        <li class="mt-1">
                            <span class="inline-block w-1/2">{users[ownerId].displayName}</span>
                            <button type="button" class="rounded-md p-1 bg-slate-200 hover:bg-slate-400" on:click={()=>removeWishlistOwner(ownerId)}>Remove</button>
                        </li>
                    {/each}
                </ul>
            </div>
        {/if}
    {:else}
        <span class="text-sm text-slate-400">owned by {wishlist.owners.map(id => id == $auth.userId ? "you" : users[id].displayName).join(" and ")}</span>
    {/if}
</div>