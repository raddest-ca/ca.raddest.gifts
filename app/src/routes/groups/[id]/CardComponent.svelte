<script lang="ts">
	import { apiFetch } from "../../../api/client";
	import { auth } from "../../../stores/auth";
	import { invalidateAll } from "$app/navigation";
	import SvelteMarkdown from "svelte-markdown";
	import type { Card, Wishlist as WishlistType } from "../../../api/types";

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
    async function deleteCard() {
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}`, {
            method: "DELETE",
        });
        await invalidateAll();
    }
    async function updateCardContent() {
        if (contentInput.trim().length === 0) return await deleteCard();
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}`, {
            method: "PATCH",
            body: JSON.stringify({
                Content: contentInput,
            }),
        });
        showContentEditor = false;
        await invalidateAll();
    }
    async function toggleVisibleToOwners() {
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}`, {
            method: "PATCH",
            body: JSON.stringify({
                VisibleToListOwners: !card.visibleToListOwners,
            }),
        });
        await invalidateAll();
    }
    async function addTag() {
        if (newTagInput.trim().length === 0) return;
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}/tag`, {
            method: "POST",
            body: JSON.stringify({
                Tag: newTagInput,
                VisibleToListOwners: isOwner,
            }),
        });
        newTagInput = "";
        showTagsEditor = false;
        await invalidateAll();
    }
    async function deleteTag(tag: string) {
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}/tag/${encodeURIComponent(tag)}`, {
            method: "DELETE",
        });
        await invalidateAll();
    }
    async function setTagVisibility(tag: string, visible: boolean) {
        await apiFetch(fetch, `/group/${card.groupId}/wishlist/${card.wishlistId}/card/${card.id}/tag/${encodeURIComponent(tag)}`, {
            method: "PATCH",
            body: JSON.stringify({
                VisibleToListOwners: visible,
            }),
        });
        await invalidateAll();
    }
    
    export let wishlist: WishlistType;
    export let card: Card;
    let showContentEditor = false;
    let contentInput = card.content;
    let showTagsEditor = false;
    let newTagInput = "";
    $: isOwner = wishlist.owners.includes($auth.userId!);
</script>
<li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm card relative" class:card-hidden-from-owner={!card.visibleToListOwners}>
    {#if showContentEditor}
        <form on:submit|preventDefault={updateCardContent}>
            <textarea
                class="p-1 rounded-md shadow-lg w-full"
                placeholder="beans"
                bind:value={contentInput}
                on:blur={()=>showContentEditor = false}
                on:keydown={e => overrideShiftEnter(e, updateCardContent)}
                use:initFocus
            />
        </form>
    {:else}
        <button class="w-full h-full" on:click={()=>showContentEditor = true}>
            <div class="text-left prose">
                <SvelteMarkdown bind:source={card.content} />
            </div>
        </button>
        <div class="card-actions absolute right-1 top-1">
            <!-- show content -->
            <button title="Add a tag"  type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>showTagsEditor=!showTagsEditor}>
                <i class="mi mi-tag"><span class="u-sr-only">Tag</span></i>
            </button>
            <!-- hide visibility actions from owner -->
            {#if !isOwner}
                <button title="Toggle owner visibility" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={toggleVisibleToOwners}>
                    {#if card.visibleToListOwners}
                        <i class="mi mi-eye"><span class="u-sr-only">Hide from owner</span></i>
                    {:else}
                        <i class="mi mi-eye-off"><span class="u-sr-only">Show to owner</span></i>
                    {/if}
                </button>
            {/if}
            
            <button title="Delete card" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>deleteCard()}>
                <i class="mi mi-circle-remove"><span class="u-sr-only">Delete card</span></i>
            </button>
        </div>
        <!-- show tags -->
        <div class="flex">
            {#each Object.entries(card.tags) as [tag, visible]}
                <div class="tag flex m-0.5 p-0.5 px-1 bg-yellow-300 rounded-md items-center" class:tag-hidden-from-owner={!visible}>
                    <span class="text-xs mr-1">{tag}</span>
                    <!-- hide visibility actions from owner -->
                    {#if !isOwner}
                        <button title="Toggle owner visibility" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={()=>setTagVisibility(tag, !visible)}>
                            {#if visible}
                                <i class="mi mi-eye"><span class="u-sr-only">Hide from owner</span></i>
                            {:else}
                                <i class="mi mi-eye-off"><span class="u-sr-only">Show to owner</span></i>
                            {/if}
                        </button>
                    {/if}
                    <button title="Delete tag" type="button" class="p-0.5 float-right hover:bg-slate-400 rounded-md" on:click={()=>deleteTag(tag)}>
                        <i class="mi mi-circle-remove"><span class="u-sr-only">Delete tag</span></i>
                    </button>
                </div>
            {/each}
        </div>
    {/if}
    {#if showTagsEditor}
        <div class="absolute right-0 bg-slate-500 rounded-lg p-2 z-10">
            <form on:submit|preventDefault={addTag}>
                <input
                    class="mt-1 p-1 rounded-md shadow-lg w-full"
                    placeholder="beans"
                    bind:value={newTagInput}
                    on:blur={()=>showTagsEditor = false}
                    use:initFocus
                />
                <button type="submit" class="hidden">Submit</button>
            </form>
        </div>
    {/if}
</li>


<style>
    .card > .card-actions {
        display: none;
    }
    .card:hover > .card-actions {
        display: block;
    }

    .card-hidden-from-owner {
        background: repeating-linear-gradient(
            45deg,
            #606cbc4f,
            #606cbc4f 10px,
            #4652980e 10px,
            #4652980e 20px
        );
    }
    
    .tag-hidden-from-owner {
        background: repeating-linear-gradient(
            45deg,
            #20b8ff,
            #20b8ff 10px,
            #e8ff16 10px,
            #e8ff16 20px
        );
    }

    .tag:not(:hover) > button {
        display:none
    }
</style>