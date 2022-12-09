<script lang="ts">
	import { goto } from "$app/navigation";
	import { apiFetch, refreshJwt } from "../../api/client";
	import { auth, AuthData } from "../../stores/auth";

    async function logout() {
        auth.set(new AuthData());
        await goto("/");
    }
    async function updateName(newName: string) {
        const res = await apiFetch(fetch, "/user/displayName", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                displayName: newName,
                UserId: $auth.userId,
            }),
        });
        await refreshJwt();
    }
    let displayName = $auth.name ?? "";
</script>

<h1 class="text-xl font-bold">Account - {$auth.name}</h1>
<hr>
<button type="button" class="underline" on:click={logout}>Log out</button>
<form class="mt-2 bg-white p-2" on:submit|preventDefault={()=>updateName(displayName)}>
    <label for="name">Display name</label>
    <input type="text" class="bg-slate-200" id="name" bind:value={displayName} />
    <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg">Save</button>
</form>
