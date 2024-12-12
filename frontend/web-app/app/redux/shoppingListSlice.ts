import { createSlice, PayloadAction } from "@reduxjs/toolkit";

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
    },
});

export const {
    setShoppingLists,
    clearShoppingLists,
    setCurrentShoppingList,
    clearCurrentShoppingList,
} = shoppingListSlice.actions;

export default shoppingListSlice.reducer;