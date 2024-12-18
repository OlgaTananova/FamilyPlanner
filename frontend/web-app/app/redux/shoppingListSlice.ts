import { createSlice, PayloadAction } from "@reduxjs/toolkit";
import { Category } from "./catalogSlice";

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
        }
    },
});

export const {
    setShoppingLists,
    clearShoppingLists,
    setCurrentShoppingList,
    clearCurrentShoppingList,
    updateCatalogItem,
    updateCatalogCategory
} = shoppingListSlice.actions;

export default shoppingListSlice.reducer;