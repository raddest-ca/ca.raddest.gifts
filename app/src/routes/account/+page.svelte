<script lang="ts">
	import { goto } from "$app/navigation";
    import { name, jwt } from "../../stores/auth";

    async function logout() {
        jwt.set(null);
        await goto("/");
    }
</script>

<h1 class="text-xl font-bold">Account - {$name}</h1>
<hr>
<button type="button" class="underline" on:click={logout}>Log out</button>
