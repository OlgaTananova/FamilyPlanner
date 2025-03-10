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

export interface CatalogItem {

    id: string;
    sku: string;
    name: string;
    categoryName: string;
    categorySKU: string;
    count: number;
    isDeleted: boolean;
    ownerId: string;
    family: string;
}

export interface ShoppingListState {
    lists: ShoppingList[],
    currentShoppingList: ShoppingList | null,
    frequentItems: CatalogItem[]
}

const initialState: ShoppingListState = {
    lists: [],
    currentShoppingList: null,
    frequentItems: []
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
        setCurrentShoppingList(state, action: PayloadAction<ShoppingList | null>) {
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
        },
        deleteShoppingListItemFromStore(state, action: PayloadAction<{ shoppingListId: string, itemId: string }>) {
            const { shoppingListId, itemId } = action.payload;
            state.lists = state.lists.map((sl) => {
                if (sl.id === shoppingListId) {
                    sl.items = sl.items.filter((item) => item.id !== itemId);
                }
                return sl;
            });
            if (state.currentShoppingList?.id === shoppingListId) {
                state.currentShoppingList.items = state.currentShoppingList.items.filter((item) => item.id !== itemId);
            }
        },
        deleteCatalogItemFromShoppingList(state, action: PayloadAction<{ sku: string }>) {
            const { sku } = action.payload;
            state.lists = state.lists.map((sl) => {
                sl.items = sl.items.map((i) => {
                    if (i.sku == sku) i.isOrphaned = true;
                    return i;
                });
                return sl;
            });

            if (state.currentShoppingList) {
                state.currentShoppingList.items = state.currentShoppingList.items.map((i) => {
                    if (i.sku == sku) i.isOrphaned = true;
                    return i;
                })
            }

        },
        getFrequentItems(state, action: PayloadAction<CatalogItem[]>) {
            state.frequentItems = action.payload;
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
    updateShoppingListInStore,
    deleteShoppingListItemFromStore,
    getFrequentItems,
    deleteCatalogItemFromShoppingList
} = shoppingListSlice.actions;

export default shoppingListSlice.reducer;