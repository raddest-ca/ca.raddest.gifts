<script lang="ts">
	import { goto } from "$app/navigation";
    import { loggedIn, name, jwt } from "../../stores/auth";

    $: if (!loggedIn) {
        goto("/login");
    }

    async function logout() {
        jwt.set(null);
        await goto("/");
    }
</script>

{#if $loggedIn}
<h1 class="text-xl font-bold">Account - {$name}</h1>
<hr>
<button type="button" class="underline" on:click={logout}>Log out</button>
{:else}
<h1 class="text-xl font-bold">Account - You are not logged in</h1>
{/if}
