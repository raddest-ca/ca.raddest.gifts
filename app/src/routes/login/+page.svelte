<script lang="ts">
	import { goto } from "$app/navigation";
    import { jwt } from "../../stores/auth"

    let username = "";
    let password = "";

    async function login() {
        const resp = await fetch(`${import.meta.env.VITE_API_URL}/Token`, {
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
            const { token } = await resp.json();
            jwt.set(token);
            await goto("/");
        } else {
            console.error("Login failed", resp);
        }
    }
</script>

<main>
    <h1 class="text-xl font-bold">Login</h1>
    <form class="flex flex-col" on:submit|preventDefault={login}>
        <label for="username">Username</label>
        <input type="text" name="username" id="username" bind:value={username} />
        <label for="password">Password</label>
        <input type="password" name="password" id="password" bind:value={password} />
        <button type="submit">Login</button>
    </form>
</main>