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
export async function createItem(name: string, categoryId: string): Promise<Item | null> {
    const newItem = await fetchApi<Item>(catalogServiceUrl!, "/api/Catalog/items", {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name, categoryId }),
    });

    if (newItem) {
        toast.success(`Item "${name}" created successfully!`);
    }

    return newItem;
}


// Update an item
export async function updateItem(id: string, name: string, categoryId: string): Promise<Item | null> {
    const updatedItem = await fetchApi<Item>(catalogServiceUrl!, `/api/Catalog/items/${id}`, {
        method: "PUT",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify({ name, categoryId }),
    });

    if (updatedItem) {
        toast.success(`Item "${name}" updated successfully!`);
    }

    return updatedItem;
}

// Delete an item
export async function deleteItem(id: string): Promise<boolean> {
    const success = await fetchApi<null>(catalogServiceUrl!, `/api/Catalog/items/${id}`, {
        method: "DELETE",
    });

    if (success === null) {
        toast.success("Item deleted successfully!");
        return true;
    }

    return false;
}

export async function updateCategory(id: string, name: string): Promise<Category | null> {
    return await fetchApi(catalogServiceUrl!, `/api/Catalog/categories/${id}`, {
        method: "PUT",
        headers: { "Content-Type": "application/json" },
        body: JSON.stringify({ name }),
    });
}

export async function deleteCategory(id: string): Promise<boolean> {
    const response = await fetchApi(catalogServiceUrl!, `/api/Catalog/categories/${id}`, { method: "DELETE" });
    return response === null; // 204 No Content
}