<script lang="ts">
	import type { PageData } from "./$types";
	import ErrorMessage from "../../../components/ErrorMessage.svelte";
	import { apiFetch, apiInvalidate } from "../../../api/client";
	import { goto, invalidate, invalidateAll } from "$app/navigation";
	import { page } from "$app/stores";

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
        await apiInvalidate(`/group/${groupId}/wishlist`);
    }

    let addCardActive: Record<string,boolean> = {}
    let addOwnerActive: Record<string,boolean> = {}
    let addCardContent: Record<string,string> = {};
    Object.values(data?.wishlists ?? {}).forEach(wishlist => {
        addCardActive[wishlist.id] = false;
        addCardContent[wishlist.id] = "";
    });
    async function addCard(wishlistId: string, content: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}/card`, {
            method: "POST",
            body: JSON.stringify({
                Content: content,
                VisibleToListOwners: true,
            }),
        });
        addCardActive[wishlistId] = false;
        addCardContent[wishlistId] = "";
        await invalidateAll();
    }
    function initAddCardInput(el: HTMLInputElement) {
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
            <h2>{wishlist.displayName}</h2>
            <hr>
            <ul>
                {#each Object.values(data.cards[wishlist.id]) as card}
                    <li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm">{card.content}</li>
                {/each}
            </ul>
            {#if addCardActive[wishlist.id]}
                <div>
                    <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContent[wishlist.id])}>
                        <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={addCardContent[wishlist.id]} on:blur={()=>addCardActive[wishlist.id] = false} use:initAddCardInput/>
                        <button type="submit" class="hidden">Submit</button>
                    </form>

                </div>
            {/if}
            <button type="button" class="text-slate-400 hover:bg-slate-500 hover:text-gray-700 p-1 mt-1 rounded-sm" on:click={()=>addCardActive[wishlist.id]=true}>Add a card</button>
            <button class="text-sm text-slate-400 hover:underline" on:click={()=>addOwnerActive[wishlist.id]=true}>owned by {wishlist.owners.map(id => data.users[id].displayName).join(" and ")}</button>

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
