import type { PageLoad } from "./$types";

export const load: PageLoad = async () => {
    return {
        groups: [
            {
                id: 1,
                name: "first",
            },
            {
                id: 2,
                name: "second",
            },
            {
                id: 3,
                name: "third",
            }
        ]
    };
};