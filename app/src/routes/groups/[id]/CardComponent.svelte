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
    
    export let wishlist: WishlistType;
    export let card: Card;
    let showContentEditor = false;
    let contentInput = card.content;
    $: isOwner = wishlist.owners.includes($auth.userId!);
</script>
<li class="mt-1 p-1 bg-slate-200 text-gray-700 rounded-sm card relative" class:hidden-from-owner={!card.visibleToListOwners}>
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
            <button title="Add a tag"  type="button" class="p-0.5 hover:bg-slate-400 rounded-md">
                <i class="mi mi-tag"><span class="u-sr-only">Tag</span></i>
            </button>
            <!-- don't let owners hide cards from themselves to prevent mis-clicks -->
            {#if !isOwner}
                <button title="Toggle owner visibility" type="button" class="p-0.5 hover:bg-slate-400 rounded-md" on:click={toggleVisibleToOwners}>
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