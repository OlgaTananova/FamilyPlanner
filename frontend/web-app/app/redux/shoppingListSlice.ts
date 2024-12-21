import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { Category } from "./catalogSlice";
import { updateShoppingList } from "../lib/fetchShoppingLists";

export interface ShoppingListItem {
    id: string;
    name: string;
    sku: string;
    categoryName: string;
    categorySKU: string;
    shoppingListId: string;
    unit: string;
    quantity: number;
    pricePerUnit: number;
    price: number;
    status: string;
    isOrphaned: boolean;
    family: string;
    ownerId: string;
}

export interface ShoppingList {
    id: string,
    heading: string,
    createdAt: string, //????
    items: ShoppingListItem[],
    salesTax: number,
    isArchived: boolean,
    isDeleted: boolean
    ownerId: string,
    family: string
}

export interface ShoppingListState {
    lists: ShoppingList[],
    currentShoppingList: ShoppingList | null
}

const initialState: ShoppingListState = {
    lists: [],
    currentShoppingList: null
}

const shoppingListSlice = createSlice({
    name: "shoppingList",
    initialState,
    reducers: {
        setShoppingLists(state, action: PayloadAction<ShoppingList[]>) {
            state.lists = action.payload;
        },
        clearShoppingLists(state) {
            state.lists = [];
        },
        setCurrentShoppingList(state, action: PayloadAction<ShoppingList>) {
            state.currentShoppingList = action.payload;
        },
        clearCurrentShoppingList(state) {
            state.currentShoppingList = null;
        },
        updateCatalogItem(
            state,
            action: PayloadAction<{
                sku: string;
                name: string;
                categoryName: string;
                categorySKU: string;
            }>
        ) {
            const { sku, name, categoryName, categorySKU } = action.payload;

            // Update the items in all shopping lists
            state.lists.forEach((list) => {
                list.items.forEach((item) => {
                    if (item.sku === sku) {
                        item.name = name;
                        item.categoryName = categoryName;
                        item.categorySKU = categorySKU;
                    }
                });
            });

            // Update the items in the current shopping list
            if (state.currentShoppingList) {
                state.currentShoppingList.items.forEach((item) => {
                    if (item.sku === sku) {
                        item.name = name;
                        item.categoryName = categoryName;
                        item.categorySKU = categorySKU;
                    }
                });
            }
        },
        updateCatalogCategory(state, action: PayloadAction<Category>) {
            const { sku, name } = action.payload;
            // Update the items in all shopping lists
            state.lists.forEach((list) => {
                list.items.forEach((item) => {
                    if (item.categorySKU === sku) {
                        item.categoryName = name;
                    }
                });
            });
            // Update the items in the current shopping list
            if (state.currentShoppingList) {
                state.currentShoppingList.items.forEach((item) => {
                    if (item.categorySKU === sku) {
                        item.categoryName = name;
                    }
                });
            }
        },
        addShoppingList(state, action: PayloadAction<ShoppingList>) {
            const existingShoppingList = state.lists.find(sl => sl.id == action.payload.id);
            if (existingShoppingList) return;
            state.lists.unshift(action.payload);
        },
        deleteShoppingListFromStore(state, action: PayloadAction<string>) {
            state.lists = state.lists.filter((sl) => sl.id !== action.payload);
            if (action.payload === state.currentShoppingList?.id) {
                state.currentShoppingList = null;
            }
        },
        updateShoppingListInStore(state, action: PayloadAction<ShoppingList>) {
            state.lists = state.lists.map((sl) => {
                if (sl.id === action.payload.id) {
                    return action.payload;
                }
                return sl;
            });
            if (state.currentShoppingList?.id === action.payload.id) {
                state.currentShoppingList = action.payload;
            }
        },
        updateShoppingListItemInStore(state, action: PayloadAction<ShoppingList>) {
            state.lists = state.lists.map((sl) => {
                if (sl.id === action.payload.id) {
                    return action.payload;
                }
                return sl;
            });
            if (state.currentShoppingList?.id === action.payload.id) {
                state.currentShoppingList = action.payload;
            }
        }
    }
});

export const {
    setShoppingLists,
    clearShoppingLists,
    setCurrentShoppingList,
    clearCurrentShoppingList,
    updateCatalogItem,
    updateCatalogCategory,
    addShoppingList,
    deleteShoppingListFromStore,
    updateShoppingListInStore
} = shoppingListSlice.actions;

export default shoppingListSlice.reducer;