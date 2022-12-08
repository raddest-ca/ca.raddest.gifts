<script lang="ts">
	import type { PageData } from "./$types";
	import ErrorMessage from "../../../components/ErrorMessage.svelte";
	import { apiFetch } from "../../../api/client";
	import { goto, invalidate } from "$app/navigation";
	import { page } from "$app/stores";

    export let data: PageData;
    $: error = data?.errorMessage;
    $: console.log(data);

    let newWishlistName = "";

    async function createWishlist(displayName: string) {
        const resp = await apiFetch(fetch, `/group/${data.data!.group.id}/wishlist`, {
            method: "POST",
            body: JSON.stringify({
                DisplayName: displayName,
            }),
        });
        if (resp.ok) {
            await invalidate($page.url);
        } else {
            error = resp.errorMessage;
        }
    }
</script>

<ErrorMessage {error}/>

{#if data.ok}
<h1 class="text-2xl font-bold">Group - {data.data.group.displayName}</h1>
<hr class="my-1">
<section>
    <h1 class="text-xl font-bold">Wishlists</h1>
    <hr class="my-1">

</section>
<section>
    <h1 class="text-xl font-bold">Add new list</h1>
    <hr class="my-1">
    <form>
        <label for="newWishlistName">Display name: </label>
        <input type="text" name="newWishlistName" id="newWishlistName" placeholder="love da mets" bind:value={newWishlistName}>
        <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg" on:click|preventDefault={()=>createWishlist(newWishlistName)}>Create</button>
    </form>
</section>
{/if}

