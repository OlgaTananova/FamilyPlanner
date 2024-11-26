import toast from "react-hot-toast";
import { Category } from "../redux/catalogSlice";
import { getAccessToken } from "./getAccessToken";
const catalogServiceUrl = process.env.NEXT_PUBLIC_CATALOG_SERVICE_URL;

export async function fetchCatalogData(): Promise<{ categories: Category[] } | null> {

    const accessToken = getAccessToken();
    if (!accessToken) {
        console.error("No access token available.");
        toast.error("No access token available. Try to sign out and sign in one again.")
        return null;
    }
    const categoryResponse = await fetch(`${catalogServiceUrl}/api/Catalog/categories`,{
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