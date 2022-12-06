# Gifts

```ts
interface Group {
    id: string;
    displayName: string;
    password: string;
    members: string[];
    owners: string[];
    lists: {
        id: string;
        owners: string[];
        cards: {
            id: string;
            hiddenToOwners: booelan
            claimers: string[];
        }[];
    }[]
}
```

Example use case:

- Family group
- Lists within group: Mom Wishlist, Dad, MomDad, Kid1, Kid2
- Example cards: "Mom Wishlist"."bike", claimed by {Dad, Kid1}