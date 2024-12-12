import toast from "react-hot-toast";
import { Category, Item } from "../redux/catalogSlice";
import fetchApi from "./fetchAPI";
import { ShoppingList } from "../redux/shoppingListSlice";
const shoppingListServiceUrl = process.env.NEXT_PUBLIC_SHOPPING_LIST_SERVICE_URL;

// Fetch all categories
export async function fetchShoppingListData(): Promise<ShoppingList[] | null> {
    return await fetchApi<ShoppingList[]>(shoppingListServiceUrl!, "/api/ShoppingLists", {
        method: "GET",
    });
}