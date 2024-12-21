import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import fetchApi from "./fetchAPI";
import { ShoppingList, ShoppingListItem } from "../redux/shoppingListSlice";
const shoppingListServiceUrl = process.env.NEXT_PUBLIC_SHOPPING_LIST_SERVICE_URL;

// Fetch all categories
export async function fetchShoppingListData(): Promise<ShoppingList[] | null> {
    return await fetchApi<ShoppingList[]>(shoppingListServiceUrl!, "/api/ShoppingLists", {
        method: "GET",
    });
}

export async function addNewShoppingList(shoppingListData: { heading: string, SKUs: string[] }): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(shoppingListServiceUrl!, "/api/ShoppingLists", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(shoppingListData),
    });
}

export async function deleteShoppingList(shoppingListId: string): Promise<boolean> {
    const result = await fetchApi<null>(shoppingListServiceUrl!, `/api/ShoppingLists/${shoppingListId}`, {
        method: "DELETE",
    });
    return result === null;
}

export async function updateShoppingList(shoppingListData: { id: string, heading: string, salesTax: number, isArchived: boolean }): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(shoppingListServiceUrl!, `/api/ShoppingLists/${shoppingListData.id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(shoppingListData),
    });
}

export async function updateShoppingListItem(shoppingListId: string, itemId: string, itemData: Partial<ShoppingListItem>): Promise<ShoppingList | null> {
    return await fetchApi<ShoppingList>(shoppingListServiceUrl!, `/api/ShoppingLists/${shoppingListId}/items/${itemId}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(itemData),
    });
}