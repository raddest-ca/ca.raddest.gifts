<script lang="ts">
	import type { PageData } from "./$types";
	import { apiFetch } from "../../../api/client";
	import { invalidateAll } from "$app/navigation";
	import { page } from "$app/stores";
	import type { Card } from "../../../api/types";

    export let data: PageData;
    $: console.log(data);
    $: groupId = $page.params.id;
    $: inviteCode = `${groupId}:${data.group.password}`;

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

    let showWishlistAddCardEditor: Record<string,boolean> = {}
    let showWishlistAddOwnerEditor: Record<string,boolean> = {}
    let showWishlistNameEditor: Record<string,boolean> = {};
    let showCardContentEditor: Record<string,boolean> = {};
    let cardContentInputs: Record<string,string> = {};
    let wishlistNameInputs: Record<string,string> = {};
    let addOwnerUserIdInputs: Record<string,string> = {};
    let addCardContentInputs: Record<string,string> = {};
    Object.values(data?.wishlists ?? {}).forEach(wishlist => {
        showWishlistAddCardEditor[wishlist.id] = false;
        showWishlistNameEditor[wishlist.id] = false;
        addCardContentInputs[wishlist.id] = "";
        wishlistNameInputs[wishlist.id] = wishlist.displayName;
    });
    for (const [wishlistId, cards] of Object.entries(data?.cards ?? {})) {
        for (const [cardId, card] of Object.entries(cards)) {
            showCardContentEditor[cardId] = false;
            cardContentInputs[cardId] = card.content;
        }
    }
    async function addCard(wishlistId: string, content: string) {
        if (content.trim().length === 0) return;
        var resp = await apiFetch<Card>(fetch, `/group/${groupId}/wishlist/${wishlistId}/card`, {
            method: "POST",
            body: JSON.stringify({
                Content: content,
                VisibleToListOwners: true,
            }),
        });
        cardContentInputs[resp.id] = resp.content;
        showCardContentEditor[resp.id] = false;
        showWishlistAddCardEditor[wishlistId] = false;
        addCardContentInputs[wishlistId] = "";
        await invalidateAll();
    }
    async function addListOwner(wishlistId: string, ownerId: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}`, {
            method: "PATCH",
            body: JSON.stringify({
                Owners: data.wishlists[wishlistId].owners.concat(ownerId),
            }),
        });
        showWishlistAddOwnerEditor[wishlistId] = false;
        await invalidateAll();
    }
    async function updateWishlistName(wishlistId: string, displayName: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}`, {
            method: "PATCH",
            body: JSON.stringify({
                DisplayName: displayName,
            }),
        });
        showWishlistNameEditor[wishlistId] = false;
        await invalidateAll();
    }
    async function updateCardContent(wishlistId: string, cardId: string, content: string) {
        if (content.trim().length === 0) return await deleteCard(wishlistId, cardId);
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}/card/${cardId}`, {
            method: "PATCH",
            body: JSON.stringify({
                Content: content,
            }),
        });
        showCardContentEditor[cardId] = false;
        await invalidateAll();
    }
    async function deleteCard(wishlistId: string, cardId: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}/card/${cardId}`, {
            method: "DELETE",
        });
        delete data.cards[cardId];
        delete showCardContentEditor[cardId];
        await invalidateAll();
    }
    function initFocus(el: HTMLInputElement | HTMLSelectElement | HTMLTextAreaElement) {
        el.focus();
    }
</script>

<h1 class="text-2xl font-bold">Group - {data.group.displayName}</h1>
<hr class="my-1">
<section>
    <h1 class="text-xl font-bold mt-4">Wishlists</h1>
    <hr class="my-1">
    <div class="flex flex-wrap">
        {#each Object.values(data.wishlists) as wishlist}
        <div class="bg-slate-300 m-4 p-4 w-64">
            {#if showWishlistNameEditor[wishlist.id]}
                <form on:submit={()=>updateWishlistName(wishlist.id, wishlistNameInputs[wishlist.id])}>
                    <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={wishlistNameInputs[wishlist.id]} on:blur={()=>showWishlistNameEditor[wishlist.id] = false} use:initFocus/>
                </form>
            {:else}
                <button on:click={()=>showWishlistNameEditor[wishlist.id] = true}>{wishlist.displayName}</button>
            {/if}
            <hr>
            <ul>
                {#each Object.values(data.cards[wishlist.id]) as card}
                    <li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm">
                        {#if showCardContentEditor[card.id]}
                            <form on:submit|preventDefault={()=>updateCardContent(wishlist.id, card.id, cardContentInputs[card.id])}>
                                <textarea class="p-1 rounded-md shadow-lg" placeholder="beans" bind:value={cardContentInputs[card.id]} on:blur={()=>showCardContentEditor[card.id] = false} use:initFocus/>
                            </form>
                        {:else}
                            <button class="w-full text-left" on:click={()=>showCardContentEditor[card.id] = true}>
                                {card.content}
                            </button>
                        {/if}
                    </li>
                {/each}
            </ul>
            {#if showWishlistAddCardEditor[wishlist.id]}
                <div>
                    <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContentInputs[wishlist.id])}>
                        <textarea class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={addCardContentInputs[wishlist.id]} on:blur={()=>showWishlistAddCardEditor[wishlist.id] = false} use:initFocus/>
                        <button type="submit" class="hidden">Submit</button>
                    </form>

                </div>
            {/if}
            <button type="button" class="text-slate-400 hover:bg-slate-500 hover:text-gray-700 p-1 mt-1 rounded-sm" on:click={()=>showWishlistAddCardEditor[wishlist.id]=true}>Add a card</button>
            <button class="text-sm text-slate-400 hover:underline" on:click={()=>showWishlistAddOwnerEditor[wishlist.id]=true}>owned by {wishlist.owners.map(id => data.users[id].displayName).join(" and ")}</button>
            {#if showWishlistAddOwnerEditor[wishlist.id]}
                <div>
                    <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContentInputs[wishlist.id])}>
                        <select class="p-1 rounded-md shadow-lg" bind:value={addOwnerUserIdInputs[wishlist.id]} use:initFocus>
                            {#each Object.values(data.users) as user}
                                <option value={user.id}>{user.displayName}</option>
                            {/each}
                        </select>
                        <button type="button" class=" rounded-md p-1 bg-slate-200 hover:bg-slate-400" on:click={addListOwner(wishlist.id, addOwnerUserIdInputs[wishlist.id])}>Add owner</button>
                    </form>

                </div>
            {/if}
        </div>
        {/each}
    </div>
</section>
<section>
    <h1 class="text-xl font-bold mt-4">Add new list</h1>
    <hr class="my-1">
    <form>
        <label for="newWishlistName">Display name: </label>
        <input type="text" name="newWishlistName" id="newWishlistName" placeholder="love da mets" bind:value={newWishlistName}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click|preventDefault={()=>createWishlist(newWishlistName)}>Create</button>
    </form>
</section>
<section>
    <h1 class="text-xl font-bold mt-4">Invite someone</h1>
    <hr class="my-2">
    <span>Invite code: <input class="bg-gray-800 text-white p-2 rounded-md text-ellipsis" bind:value={inviteCode}/> <button class="underline text-blue-500" type="button" on:click={()=>navigator.clipboard.writeText(inviteCode)}>(copy)</button></span>
</section>
