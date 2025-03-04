import { useCallback } from "react";
import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import { useFetchApi } from "./useFetchApi";
import { FamilyUser } from "../family/[familyName]/page";
const gatewayUrl = process.env.NEXT_PUBLIC_GATEWAY_URL;


export function useCatalogApi() {
    const { fetchApi } = useFetchApi();

    // Fetch all categories
    const fetchCatalogData = useCallback(async (): Promise<Category[] | null> => {
        return await fetchApi<Category[]>(gatewayUrl!, "/catalog/categories", {
            method: "GET",
        });
    }, [fetchApi]);

    // Create a new category
    const createCategory = useCallback(async (name: string): Promise<Category | null> => {
        const newCategory = await fetchApi<Category>(gatewayUrl!, "/catalog/categories", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ name }),
        });

        if (newCategory) {
            toast.success(`Category "${name}" created successfully!`);
        }

        return newCategory;
    }, [fetchApi]);

    // Create a new item
    const createItem = useCallback(async (name: string, categorySKU: string): Promise<Item | null> => {
        const newItem = await fetchApi<Item>(gatewayUrl!, "/catalog/items", {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ name, categorySKU }),
        });

        if (newItem) {
            toast.success(`Item "${name}" created successfully!`);
        }

        return newItem;
    }, [fetchApi]);


    // Update an item
    const updateItem = useCallback(async (sku: string, name: string, categorySKU: string): Promise<{ updatedItem: Item, previousCategorySKU: string } | null> => {
        const updatedItem = await fetchApi<{ updatedItem: Item, previousCategorySKU: string }>(gatewayUrl!, `/catalog/items/${sku}`, {
            method: "PUT",
            headers: {
                "Content-Type": "application/json",
            },
            body: JSON.stringify({ name, categorySKU }),
        });

        return updatedItem;
    }, [fetchApi]);

    // Delete an item
    const deleteItem = useCallback(async (sku: string): Promise<boolean> => {
        const success = await fetchApi<null>(gatewayUrl!, `/catalog/items/${sku}`, {
            method: "DELETE",
        });

        if (success === null) {
            toast.success("Item deleted successfully!");
            return true;
        }

        return false;
    }, [fetchApi]);

    const updateCategory = useCallback(async (sku: string, name: string): Promise<Category | null> => {
        return await fetchApi(gatewayUrl!, `/catalog/categories/${sku}`, {
            method: "PUT",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({ name }),
        });
    }, [fetchApi]);

    const deleteCategory = useCallback(async (sku: string): Promise<boolean> => {
        const response = await fetchApi(gatewayUrl!, `/catalog/categories/${sku}`, { method: "DELETE" });
        return response === null; // 204 No Content
    }, [fetchApi]);

    const fetchCatalogSearchResults = useCallback(async (query: string): Promise<Item[] | null> => {
        return await fetchApi(gatewayUrl!, `/catalog/items/search?query=${query}`, { method: "GET" });
    }, [fetchApi]);

    const fetchFamilyUsers = useCallback(async (familyName: string): Promise<FamilyUser[] | null> => {
        const res = await fetchApi<FamilyUser[]>(gatewayUrl!, `/api/family/${familyName}/users`, {
            method: "GET",
            headers: {
                "Content-Type": "application/json",
            }
        });

        return res;
    }, [fetchApi]);

    return {
        fetchCatalogSearchResults,
        deleteCategory,
        updateCategory,
        deleteItem,
        updateItem,
        createItem,
        createCategory,
        fetchCatalogData,
        fetchFamilyUsers
    }
}