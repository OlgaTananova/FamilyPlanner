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
        clearCategories(state) {
            state = initialState;
        },
        addCategory(state, action: PayloadAction<Category>) {
            state.categories.push(action.payload);
        },
        addItem(state, action: PayloadAction<Item>) {
            const { categoryId } = action.payload;
            const category = state.categories.find((cat) => cat.id === categoryId);
            if (category) {
                category.items.push(action.payload);
            }
        },
        updateItemInStore(state, action: PayloadAction<{ item: Item; currentCategory: string }>) {
            const { id, name, categoryId } = action.payload.item;
            const { currentCategory } = action.payload;

            // Remove the item from the current category if the category has changed
            if (categoryId !== currentCategory) {
                state.categories = state.categories.map((cat) =>
                    cat.id === currentCategory
                        ? { ...cat, items: cat.items.filter((item) => item.id !== id) }
                        : cat
                );
            }

            // Add or update the item in the new category
            const category = state.categories.find((cat) => cat.id === categoryId);
            if (category) {
                const existingItem = category.items.find((item) => item.id === id);

                if (existingItem) {
                    existingItem.name = name; // Update existing item
                } else {
                    category.items.push(action.payload.item); // Add new item if not found
                }
            }
        },
        removeItemFromStore(state, action: PayloadAction<string>) {
            state.categories.forEach((category) => {
                category.items = category.items.filter((item) => item.id !== action.payload);
            });
        },
        updateCategoryInStore(state, action: PayloadAction<Category>) {
            const category = state.categories.find((cat) => cat.id === action.payload.id);
            if (category) {
                category.name = action.payload.name;
            }
        },
        removeCategoryFromStore(state, action: PayloadAction<string>) {
            state.categories = state.categories.filter((cat) => cat.id !== action.payload);
        },

    }
});

export const { setCategories, clearCategories, addCategory, addItem, updateItemInStore, removeItemFromStore, updateCategoryInStore, removeCategoryFromStore } = catalogSlice.actions;
export default catalogSlice.reducer;