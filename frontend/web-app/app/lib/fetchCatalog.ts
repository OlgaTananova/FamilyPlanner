import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import { getAccessToken } from "./getAccessToken";
import { AppDispatch } from "../redux/store";
const catalogServiceUrl = process.env.NEXT_PUBLIC_CATALOG_SERVICE_URL;

export async function fetchCatalogData(): Promise<{ categories: Category[] } | null> {
    const accessToken = getAccessToken();
    if (!accessToken) {
        console.error("No access token available.");
        toast.error("No access token available. Try to sign out and sign in one again.")
        return null;
    }
    const categoryResponse = await fetch(`${catalogServiceUrl}/api/Catalog/categories`, {
        method: 'GET',
        headers: {
            Authorization: `Bearer ${accessToken}`
        }
    });

    if (!categoryResponse.ok) {
        throw new Error("Failed to fetch catalog data.");
    }

    const categories = await categoryResponse.json();

    return { categories };
}

export async function createCategory(name: string): Promise<Category | null> {

    const accessToken = getAccessToken();
    if (!accessToken) {
        console.error("Access token is not available.");
        toast.error("Failed to create category. Authentication is required.");
        return null;
    }

    try {
        const response = await fetch(`${catalogServiceUrl}/api/Catalog/categories`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${accessToken}`,
            },
            body: JSON.stringify({ name }),
        });

        if (!response.ok) {
            throw new Error("Failed to create category.");
        }

        const newCategory: Category = await response.json();
        toast.success(`Category "${name}" created successfully!`);
        return newCategory;
    } catch (error: any) {
        console.error("Error creating category:", error);
        toast.error(error.message || "Failed to create category.");
        return null;
    }
}

export async function createItem(name: string, categoryId: string): Promise<Item | null> {

    if (!catalogServiceUrl) {
        console.error("API URL is not defined.");
        toast.error("Failed to create item. Server configuration is missing.");
        return null;
    }

    const accessToken = getAccessToken();
    if (!accessToken) {
        console.error("Access token is not available.");
        toast.error("Failed to create item. Authentication is required.");
        return null;
    }

    try {
        const response = await fetch(`${catalogServiceUrl}/api/Catalog/items`, {
            method: "POST",
            headers: {
                "Content-Type": "application/json",
                Authorization: `Bearer ${accessToken}`,
            },
            body: JSON.stringify({ name, categoryId }),
        });

        if (!response.ok) {
            throw new Error("Failed to create item.");
        }
        const newItem = await response.json();

        return newItem;
    } catch (error: any) {
        console.error("Error creating item:", error);
        toast.error(error.message || "Failed to create item.");
        return null;
    }
}
