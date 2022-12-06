<script lang="ts">
	import type { PageData } from "./$types";


    
    export let data: PageData;
</script>

<h1 class="text-xl font-bold">My Groups</h1>
<hr>
<ul>
    {#each data.groups as group}
        <li><a class="underline" href="/groups/{group.id}">{group.name}</a></li>
    {/each}
</ul>