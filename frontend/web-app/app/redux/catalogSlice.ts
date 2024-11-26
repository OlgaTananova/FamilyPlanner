import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface Item {
    id: string;
    name: string;
    ownerId: string;
    family: string;
    isDeleted: boolean;
    categoryId: string;
}

export interface Category {
    id: string;
    name: string;
    ownerId: string;
    family: string;
    isDeleted: boolean;
    items: Item[];
}

interface CatalogState {
    categories: Category[];
}

const initialState: CatalogState = {
    categories: [],
};

export const catalogSlice = createSlice({
    name: "catalog",
    initialState,
    reducers: {
        setCategories(state, action: PayloadAction<Category[]>) {
            state.categories = action.payload;
        },
    },
});

export const { setCategories} = catalogSlice.actions;
export default catalogSlice.reducer;