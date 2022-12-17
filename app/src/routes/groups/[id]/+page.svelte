<script lang="ts">
	import type { PageData } from "./$types";
	import { apiFetch } from "../../../api/client";
	import { goto, invalidateAll } from "$app/navigation";
	import { page } from "$app/stores";
	import type { Card } from "../../../api/types";
    import SvelteMarkdown from "svelte-markdown";
	import { auth } from "../../../stores/auth";

    export let data: PageData;
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
                VisibleToListOwners: data.wishlists[wishlistId].owners.includes($auth.userId),
            }),
        });
        cardContentInputs[resp.id] = resp.content;
        showCardContentEditor[resp.id] = false;
        showWishlistAddCardEditor[wishlistId] = false;
        addCardContentInputs[wishlistId] = "";
        await invalidateAll();
    }
    async function addWishlistOwner(wishlistId: string, ownerId: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}`, {
            method: "PATCH",
            body: JSON.stringify({
                Owners: data.wishlists[wishlistId].owners.concat(ownerId),
            }),
        });
        showWishlistAddOwnerEditor[wishlistId] = false;
        await invalidateAll();
    }
    async function removeWishlistOwner(wishlistId: string, ownerId: string) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}`, {
            method: "PATCH",
            body: JSON.stringify({
                Owners: data.wishlists[wishlistId].owners.filter(id => id !== ownerId),
            }),
        });
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
    async function setVisibleToOwners(wishlistId: string, cardId: string, visible: boolean) {
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}/card/${cardId}`, {
            method: "PATCH",
            body: JSON.stringify({
                VisibleToListOwners: visible,
            }),
        });
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

    async function deleteWishlist(wishlistId: string) {
        if (!window.confirm(`Are you sure you want to delete this wishlist?`)) return;
        await apiFetch(fetch, `/group/${groupId}/wishlist/${wishlistId}`, {
            method: "DELETE",
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
    
</script>

<svelte:head>
    <title>{data.group.displayName} - Group</title>
</svelte:head>

{#if showGroupDisplayNameEditor}
    <form on:submit|preventDefault={()=>updateGroupDisplayName(groupDisplayNameInput)}>
        <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={groupDisplayNameInput} on:blur={()=>showGroupDisplayNameEditor = false} use:initFocus/>
    </form>
{:else}
    <button type="button" on:click={()=>showGroupDisplayNameEditor=true}>
        <h1 class="text-2xl font-bold">Group - {data.group.displayName}</h1>
    </button>
{/if}
<hr class="my-1">
<section>
    <h1 class="text-xl font-bold mt-4">Members</h1>
    <hr class="my-1">
    <div class="flex flex-wrap">
        {#each data.group.members as memberId}
            {@const member = data.users[memberId]}
            <div class="bg-slate-300 m-4 p-4 w-64">
                <div class="flex flex-row">
                    <div class="flex-grow relative">
                        <h1 class="text-lg">{member.displayName}</h1>
                        <p class="text-sm">{`${memberId === $auth.userId ? "you, " : ""}${data.group.owners.includes(memberId) ? "owner" : "member"}`}</p>
                        <div class="absolute right-0 top-0">
                            {#if memberId === $auth.userId || data.group.owners.includes($auth.userId)}
                                <button title="Remove from group" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>removeUserFromGroup(memberId)}>
                                    <i class="mi mi-circle-remove"><span class="u-sr-only">Remove from group</span></i>
                                </button>
                            {/if}
                            {#if data.group.owners.includes($auth.userId)}
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
            {@const canModifyWishlist = data.group.owners.includes($auth.userId) || wishlist.owners.includes($auth.userId)}
            <div class="bg-slate-300 m-4 p-4 w-64">
                <!-- wishlist title -->
                <div>
                    {#if canModifyWishlist}
                        {#if showWishlistNameEditor[wishlist.id]}
                            <form on:submit|preventDefault={()=>updateWishlistName(wishlist.id, wishlistNameInputs[wishlist.id])}>
                                <input class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={wishlistNameInputs[wishlist.id]} on:blur={()=>showWishlistNameEditor[wishlist.id] = false} use:initFocus/>
                            </form>
                        {:else}
                            <button on:click={()=>showWishlistNameEditor[wishlist.id] = true}><h1>{wishlist.displayName.trim() === "" ? "untitled wishlist" : wishlist.displayName}</h1></button>
                            {#if wishlist.owners.includes($auth.userId) || data.group.owners.includes($auth.userId)}
                                <button title="Remove from group" type="button" class="p-0.5 float-right hover:bg-slate-400 rounded-md" on:click={()=>deleteWishlist(wishlist.id)}>
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
                    {#each Object.values(data.cards[wishlist.id]) as card}
                        <li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm card relative" class:hidden-from-owner={!card.visibleToListOwners}>
                            {#if showCardContentEditor[card.id]}
                                <form on:submit|preventDefault={()=>updateCardContent(wishlist.id, card.id, cardContentInputs[card.id])}>
                                    <textarea
                                        class="p-1 rounded-md shadow-lg w-full"
                                        placeholder="beans"
                                        bind:value={cardContentInputs[card.id]}
                                        on:blur={()=>showCardContentEditor[card.id] = false}
                                        on:keydown={e => overrideShiftEnter(e, ()=>updateCardContent(wishlist.id, card.id, cardContentInputs[card.id]))}
                                        use:initFocus
                                    />
                                </form>
                            {:else}
                                <button class="text-left" on:click={()=>showCardContentEditor[card.id] = true}>
                                    <div class="w-full h-full prose">
                                        <SvelteMarkdown bind:source={card.content} />
                                    </div>
                                </button>
                                <div class="card-actions absolute right-1 top-1">
                                    <button title="Add a tag"  type="button" class="p-0.5 hover:bg-slate-400 rounded-md">
                                        <i class="mi mi-tag"><span class="u-sr-only">Tag</span></i>
                                    </button>
                                    <!-- don't let owners hide cards from themselves to prevent mis-clicks -->
                                    {#if !wishlist.owners.includes($auth.userId)}
                                        <button title="Toggle owner visibility" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>setVisibleToOwners(wishlist.id, card.id, !card.visibleToListOwners)}>
                                            {#if card.visibleToListOwners}
                                                <i class="mi mi-eye"><span class="u-sr-only">Hide from owner</span></i>
                                            {:else}
                                                <i class="mi mi-eye-off"><span class="u-sr-only">Show to owner</span></i>
                                            {/if}
                                        </button>
                                    {/if}
                                </div>
                            {/if}
                        </li>
                    {/each}
                </ul>
                <!-- add card -->
                {#if showWishlistAddCardEditor[wishlist.id]}
                    <div>
                        <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContentInputs[wishlist.id])}>
                            <textarea
                                class="mt-1 p-1 rounded-md shadow-lg"
                                placeholder="beans"
                                bind:value={addCardContentInputs[wishlist.id]}
                                on:blur={()=>showWishlistAddCardEditor[wishlist.id] = false}
                                on:keydown={e => overrideShiftEnter(e, ()=>addCard(wishlist.id, addCardContentInputs[wishlist.id]))}
                                use:initFocus
                            />
                            <button type="submit" class="hidden">Submit</button>
                        </form>

                    </div>
                {/if}
                <button type="button" class="text-slate-400 hover:bg-slate-500 hover:text-gray-700 p-1 mt-1 rounded-sm" on:click={()=>showWishlistAddCardEditor[wishlist.id]=true}>Add a card</button>
                
                <!-- manage owners -->
                {#if canModifyWishlist}
                    <button class="text-sm text-slate-400 hover:underline" on:click={()=>showWishlistAddOwnerEditor[wishlist.id]=!showWishlistAddOwnerEditor[wishlist.id]}>owned by {wishlist.owners.map(id => id == $auth.userId ? "you" : data.users[id].displayName).join(" and ")}</button>
                    {#if showWishlistAddOwnerEditor[wishlist.id]}
                        <div>
                            <form on:submit|preventDefault={()=>addCard(wishlist.id, addCardContentInputs[wishlist.id])}>
                                <select class="p-1 rounded-md shadow-lg w-1/2" bind:value={addOwnerUserIdInputs[wishlist.id]} use:initFocus>
                                    {#each Object.values(data.users) as user}
                                        <option value={user.id}>{user.displayName}</option>
                                    {/each}
                                </select>
                                <button type="button" class=" rounded-md p-1 bg-slate-200 hover:bg-slate-400" on:click={()=>addWishlistOwner(wishlist.id, addOwnerUserIdInputs[wishlist.id])}>Add owner</button>
                            </form>
                            <ul>
                                {#each wishlist.owners as ownerId}
                                    <li class="mt-1">
                                        <span class="inline-block w-1/2">{data.users[ownerId].displayName}</span>
                                        <button type="button" class="rounded-md p-1 bg-slate-200 hover:bg-slate-400" on:click={()=>removeWishlistOwner(wishlist.id, ownerId)}>Remove</button>
                                    </li>
                                {/each}
                            </ul>
                        </div>
                    {/if}
                {:else}
                    <span class="text-sm text-slate-400">owned by {wishlist.owners.map(id => id == $auth.userId ? "you" : data.users[id].displayName).join(" and ")}</span>
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
    <h1 class="text-xl font-bold mt-4">Invitations</h1>
    <hr class="my-2">
    {#if data.group.owners.includes($auth.userId)}
        <form class="my-2" on:submit|preventDefault={()=>updateGroupPassword(groupPasswordInput)}>
            <label for="groupPassword">Password: </label>
            <input id="groupPassword" class="mt-1 p-1 rounded-md shadow-lg" placeholder="beans" bind:value={groupPasswordInput}/>
            <button class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" type="submit">Apply</button>
        </form>
    {/if}
    <span>Invite code: <input class="bg-gray-800 text-white p-2 rounded-md text-ellipsis" bind:value={inviteCode}/>
    <button title="Copy code" type="button" on:click={()=>navigator.clipboard.writeText(inviteCode)}>
        <i class="mi mi-clipboard"><span class="u-sr-only">Copy code</span></i>
    </button></span>
</section>
{#if data.group.owners.includes($auth.userId)}
    <section>
        <h1 class="text-xl font-bold mt-4">Group actions</h1>
        <hr class="my-2">
        <button class="rounded-xl bg-red-200 p-2 drop-shadow-lg" on:click|preventDefault={()=>deleteGroup()}>Delete group</button>
    </section>
{/if}

<style>
    .card > .card-actions {
        display: none;
    }
    .card:hover > .card-actions {
        display: block;
    }

    .card.hidden-from-owner {
        background: repeating-linear-gradient(
            45deg,
            #606cbc4f,
            #606cbc4f 10px,
            #4652980e 10px,
            #4652980e 20px
        );
    }
</style>