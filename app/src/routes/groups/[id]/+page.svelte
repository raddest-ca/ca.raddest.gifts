<script lang="ts">
	import type { PageData } from "./$types";
	import ErrorMessage from "../../../components/ErrorMessage.svelte";
	import { apiFetch, apiInvalidate } from "../../../api/client";
	import { goto, invalidate, invalidateAll } from "$app/navigation";
	import { page } from "$app/stores";

    export let data: PageData;
    $: console.log(data);
    $: groupId = $page.params.id;
    $: inviteCode = data.ok ? `${groupId}:${data.data.group.password}` : "failed to load code";

    let postError = "";
    let newWishlistName = "";

    async function createWishlist(displayName: string) {
        const resp = await apiFetch(fetch, `/group/${groupId}/wishlist`, {
            method: "POST",
            body: JSON.stringify({
                DisplayName: displayName,
            }),
        });
        if (resp.ok) {
            await apiInvalidate(`/group/${groupId}/wishlist`);
        } else {
            postError = resp.errorMessage;
        }
    }

    let addCardActive: Record<string,boolean> = {}
    let addCardContent: Record<string,string> = {};
    data.data?.wishlists.forEach(wishlist => {
        addCardActive[wishlist.id] = false;
        addCardContent[wishlist.id] = "";
    });

    function initAddCardInput(el: HTMLInputElement) {
        el.focus();
    }

    async function addCard(wishlistId: string, content: string) {
        const resp = await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}/card`, {
            method: "POST",
            body: JSON.stringify({
                Content: content,
                VisibleToListOwners: true,
            }),
        });
        if (resp.ok) {
            addCardActive[wishlistId] = false;
            addCardContent[wishlistId] = "";
            await apiInvalidate(`/group/${groupId}/wishlist/${wishlistId}/card`);
        } else {
            postError = resp.errorMessage;
        }
    }
</script>

<ErrorMessage error={!data.ok ? data.errorMessage : null}/>
<ErrorMessage error={postError}/>

{#if data.ok}
<h1 class="text-2xl font-bold">Group - {data.data.group.displayName}</h1>
<hr class="my-1">
<section>
    <h1 class="text-xl font-bold mt-4">Wishlists</h1>
    <hr class="my-1">
    <div class="flex flex-wrap">
        {#each data.data.wishlists as wishlist}
        <div class="bg-slate-300 m-4 p-4 w-64">
            <h2>{wishlist.displayName}</h2>
            <hr>
            <ul>
                {#each data.data.cards[wishlist.id] as card}
                    <li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm">{card.content}</li>
                {/each}
            </ul>
            {#if addCardActive[wishlist.id]}
                <div>
                    <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContent[wishlist.id])}>
                        <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={addCardContent[wishlist.id]} use:initAddCardInput/>
                        <button type="submit" class="hidden">Submit</button>
                    </form>

                </div>
            {/if}
            <button type="button" class="text-slate-400 hover:bg-slate-500 hover:text-gray-700 p-1 mt-1 rounded-sm" on:click={()=>{addCardActive[wishlist.id]=true;}}>Add a card</button>
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
{/if}

