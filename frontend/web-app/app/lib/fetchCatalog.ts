import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import fetchApi from "./fetchAPI";
const catalogServiceUrl = process.env.NEXT_PUBLIC_CATALOG_SERVICE_URL;


// Fetch all categories
export async function fetchCatalogData(): Promise<Category[] | null> {
    return await fetchApi<Category[]>(catalogServiceUrl!, "/api/Catalog/categories", {
        method: "GET",
    });
}

// Create a new category
export async function createCategory(name: string): Promise<Category | null> {
    const newCategory = await fetchApi<Category>(catalogServiceUrl!, "/api/Catalog/categories", {
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
}

// Create a new item
export async function createItem(name: string, categorySKU: string): Promise<Item | null> {
    const newItem = await fetchApi<Item>(catalogServiceUrl!, "/api/Catalog/items", {
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
}


// Update an item
export async function updateItem(sku: string, name: string, categorySKU: string): Promise<{ updatedItem: Item, previousCategorySKU: string } | null> {
    const updatedItem = await fetchApi<{ updatedItem: Item, previousCategorySKU: string }>(catalogServiceUrl!, `/api/Catalog/items/${sku}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name, categorySKU }),
    });

    return updatedItem;
}

// Delete an item
export async function deleteItem(sku: string): Promise<boolean> {
    const success = await fetchApi<null>(catalogServiceUrl!, `/api/Catalog/items/${sku}`, {
        method: "DELETE",
    });

    if (success === null) {
        toast.success("Item deleted successfully!");
        return true;
    }

    return false;
}

export async function updateCategory(sku: string, name: string): Promise<Category | null> {
    return await fetchApi(catalogServiceUrl!, `/api/Catalog/categories/${sku}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name }),
    });
}

export async function deleteCategory(sku: string): Promise<boolean> {
    const response = await fetchApi(catalogServiceUrl!, `/api/Catalog/categories/${sku}`, { method: "DELETE" });
    return response === null; // 204 No Content
}

export async function fetchCatalogSearchResults(query: string): Promise<Item[] | null> {
    return await fetchApi(catalogServiceUrl!, `/api/Catalog/items/search?query=${query}`, { method: "GET" });
};