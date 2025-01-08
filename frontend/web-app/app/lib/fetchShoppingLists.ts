import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import fetchApi from "./fetchAPI";
import { CatalogItem, ShoppingList, ShoppingListItem } from "../redux/shoppingListSlice";
const gatewayUrl = process.env.NEXT_PUBLIC_GATEWAY_URL;

// Fetch all categories
export async function fetchShoppingListData(): Promise<ShoppingList[] | null> {
    return await fetchApi<ShoppingList[]>(gatewayUrl!, "/shoppingList", {
        method: "GET",
    });
}

export async function addNewShoppingList(shoppingListData: { heading: string, SKUs: string[] }): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(gatewayUrl!, "/shoppinglist", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(shoppingListData),
    });
}

export async function deleteShoppingList(shoppingListId: string): Promise<boolean> {
    const result = await fetchApi<null>(gatewayUrl!, `/shoppinglist/${shoppingListId}`, {
        method: "DELETE",
    });
    return result === null;
}

export async function updateShoppingList(shoppingListData: { id: string, heading: string, salesTax: number, isArchived: boolean }): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListData.id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(shoppingListData),
    });
}

export async function updateShoppingListItem(shoppingListId: string, itemId: string, itemData: Partial<ShoppingListItem>): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items/${itemId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(itemData),
    });
}

export async function searchShoppingListItems(searchTerm: string): Promise<Item[] | null> {
    return await fetchApi<Item[]>(gatewayUrl!, `/shoppinglist/catalogitems/search?query=${searchTerm}`, {
        method: "GET",
    });
}

export async function addShoppingListItems(shoppingListId: string, itemData: { skus: string[] }): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(itemData),
    });
}

export async function deleteShoppingListItem(shoppingListId: string, itemId: string): Promise<boolean> {
    const result = await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items/${itemId}`, {
        method: "DELETE",
    });

    return result === null;
}

export async function getFrequentyBoughtItems(): Promise<CatalogItem[] | null> {
    return await fetchApi<CatalogItem[]>(gatewayUrl!, "/shoppinglist/catalogitems/freq-bought", {
        method: "GET",
    });
}