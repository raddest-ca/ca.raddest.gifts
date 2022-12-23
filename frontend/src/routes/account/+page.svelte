<script lang="ts">
	import { goto } from "$app/navigation";
	import { apiFetch, refreshJwt } from "../../api/client";
	import { auth, AuthData } from "../../stores/auth";

    async function logout() {
        auth.set(new AuthData());
        await goto("/");
    }
    async function updateName() {
        const res = await apiFetch(fetch, `/user/${$auth.userId}`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                displayName: displayName,
            }),
        });
        await refreshJwt(fetch);
    }
    async function updatePassword() {
        if (password !== passwordVerify) return;
        const res = await apiFetch(fetch, `/user/${$auth.userId}`, {
            method: "PATCH",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({
                password: password,
            }),
        });
        await refreshJwt(fetch);
    }
    let displayName = $auth.name ?? "";
    let password = "";
    let passwordVerify = "";
    $: warningText = password !== passwordVerify ? "Passwords do not match" : "";
</script>

<h1 class="text-xl font-bold">Account - {$auth.name}</h1>
<hr>
<button type="button" class="underline" on:click={logout}>Log out</button>
<form class="mt-2 bg-white p-2" on:submit|preventDefault={updateName}>
    <label for="name">Display name</label>
    <input type="text" class="bg-slate-200" id="name" bind:value={displayName} />
    <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg">Save</button>
</form>
<form class="mt-2 bg-white p-2" on:submit|preventDefault={updatePassword}>
    <div>
        <label class="w-32 inline-block" for="password">Password</label>
        <input type="password" class="bg-slate-200" id="password" bind:value={password} />
    </div>
    <div class="mt-2">
        <label class="w-32 inline-block" for="password-verify">Password verify</label>
        <input type="password-verify" class="bg-slate-200" id="password" bind:value={passwordVerify} />
    </div>
    <span class="text-red-500">{warningText}</span>
    <br/>
    <button type="submit" class="rounded-xl bg-slate-200 p-2 drop-shadow-lg">Save</button>
</form>
