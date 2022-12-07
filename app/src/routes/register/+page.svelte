<script lang="ts">
	import { goto } from "$app/navigation";
    import { jwt } from "../../stores/auth"

    let username = "";
    let password = "";
    let password2 = "";
    let error = "";

    async function register() {
        if (error !== "") return;
        const resp = await fetch(`${import.meta.env.VITE_API_URL}/User`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json"
            },
            body: JSON.stringify({
                UserLoginName: username,
                UserPassword: password,
            }),
        });
        if (resp.ok) {
            await goto("/register/success");
        } else {
            console.log("register failed")
        }
    }
    

    $: if (password !== password2) {
        error = "Passwords do not match";
    } else {
        error = "";
    }
</script>

<main>
    <h1 class="text-xl font-bold">Register</h1>
    <hr class="my-2">
    <form on:submit|preventDefault={register}>
        <div>
            <div class="mb-2">
                <label class="w-32 inline-block" for="username">Username</label>
                <input type="text" name="username" id="username" bind:value={username} />
            </div>
            <div class="mb-2">
                <label class="w-32 inline-block" for="password">Password</label>
                <input type="password" name="password" id="password" bind:value={password} />
            </div>
            <div class="mb-2">
                <label class="w-32 inline-block" for="password">Password confirm</label>
                <input type="password" name="password" id="password" bind:value={password2} />
            </div>
        </div>
        {#if error !== ""}
            <div class="text-red-600 drop-shadow-md">
                {error}
            </div>
        {/if}
        <button class="rounded-xl bg-slate-200 p-2 drop-shadow-lg m-2" type="submit">Login</button>
    </form>
</main>