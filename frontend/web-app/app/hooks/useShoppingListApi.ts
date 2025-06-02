import { useCallback } from "react";
import { Item } from "../redux/catalogSlice";
import { CatalogItem, ShoppingList, ShoppingListItem } from "../redux/shoppingListSlice";
import { useFetchApi } from "./useFetchApi";
const gatewayUrl = process.env.NEXT_PUBLIC_GATEWAY_URL;

// Fetch all categories

export function useShoppingListApi() {
    const { fetchApi } = useFetchApi();

    const fetchShoppingListData = useCallback(async (): Promise<ShoppingList[] | null> => {
        return await fetchApi<ShoppingList[]>(gatewayUrl!, "/shoppingList", {
            method: "GET",
        });
    }, [fetchApi]);

    const addNewShoppingList = useCallback(async (shoppingListData: { heading: string, SKUs: string[] }): Promise<ShoppingList | null> => {
        return await fetchApi<ShoppingList>(gatewayUrl!, "/shoppinglist", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(shoppingListData),
        });
    }, [fetchApi]);



    const deleteShoppingList = useCallback(async (shoppingListId: string): Promise<boolean> => {
        const result = await fetchApi<null>(gatewayUrl!, `/shoppinglist/${shoppingListId}`, {
            method: "DELETE",
        });
        return result === null;
    }, [fetchApi]);

    const updateShoppingList = useCallback(async (shoppingListData: { id: string, heading: string, salesTax: number, isArchived: boolean }): Promise<ShoppingList | null> => {
        return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListData.id}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(shoppingListData),
        });
    }, [fetchApi]);

    const updateShoppingListItem = useCallback(async (shoppingListId: string, itemId: string, itemData: Partial<ShoppingListItem>): Promise<ShoppingList | null> => {
        return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items/${itemId}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(itemData),
        });
    }, [fetchApi]);

    const searchShoppingListItems = useCallback(async (searchTerm: string): Promise<Item[] | null> => {
        return await fetchApi<Item[]>(gatewayUrl!, `/shoppinglist/catalogitems/search?query=${searchTerm}`, {
            method: "GET",
        });
    }, [fetchApi]);

    const addShoppingListItems = useCallback(async (shoppingListId: string, itemData: { skus: string[] }): Promise<ShoppingList | null> => {
        return await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify(itemData),
        });
    }, [fetchApi]);

    const deleteShoppingListItem = useCallback(async (shoppingListId: string, itemId: string): Promise<boolean> => {
        const result = await fetchApi<ShoppingList>(gatewayUrl!, `/shoppinglist/${shoppingListId}/items/${itemId}`, {
            method: "DELETE",
        });

        return result === null;
    }, [fetchApi]);

    const getFrequentyBoughtItems = useCallback(async (): Promise<CatalogItem[] | null> => {
        return await fetchApi<CatalogItem[]>(gatewayUrl!, "/shoppinglist/catalogitems/freq-bought", {
            method: "GET",
        });
    }, [fetchApi]);
    return {
        fetchShoppingListData,
        addNewShoppingList,
        getFrequentyBoughtItems,
        deleteShoppingListItem,
        addShoppingListItems,
        searchShoppingListItems,
        updateShoppingListItem,
        updateShoppingList,
        deleteShoppingList
    }
}