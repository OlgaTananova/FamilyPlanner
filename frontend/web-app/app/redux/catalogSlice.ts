import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface Item {
    id: string;
    sku: string;
    name: string;
    ownerId: string;
    family: string;
    isDeleted: boolean;
    categoryId: string;
    categorySKU: string;
}

export interface Category {
    id: string;
    name: string;
    ownerId: string;
    family: string;
    isDeleted: boolean;
    sku: string;
    items: Item[];
}


interface CatalogState {
    categories: Category[];
    itemsWOCategories: Item[];
}

const initialState: CatalogState = {
    categories: [],
    itemsWOCategories: []
};


export const catalogSlice = createSlice({
    name: "catalog",
    initialState,
    reducers: {
        setCategories(state, action: PayloadAction<Category[]>) {
            state.categories = action.payload;
            const allItems = state.categories.flatMap((category) => category.items).sort((a, b) => a.name.localeCompare(b.name));
            state.itemsWOCategories = allItems;
        },
        clearCategories(state) {
            state.categories = initialState.categories;
            state.itemsWOCategories = initialState.itemsWOCategories;
        },
        addCategory(state, action: PayloadAction<Category>) {
            state.categories.push(action.payload);
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        addItem(state, action: PayloadAction<Item>) {
            const { categoryId } = action.payload;
            const category = state.categories.find((cat) => cat.id === categoryId);
            if (category) {
                category.items.push(action.payload);
            }
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
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
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        removeItemFromStore(state, action: PayloadAction<string>) {
            state.categories.forEach((category) => {
                category.items = category.items.filter((item) => item.id !== action.payload);
            });
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        updateCategoryInStore(state, action: PayloadAction<Category>) {
            const category = state.categories.find((cat) => cat.id === action.payload.id);
            if (category) {
                category.name = action.payload.name;
            }
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        removeCategoryFromStore(state, action: PayloadAction<string>) {
            state.categories = state.categories.filter((cat) => cat.id !== action.payload);
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },

    }
});

export const { setCategories, clearCategories, addCategory, addItem, updateItemInStore, removeItemFromStore, updateCategoryInStore, removeCategoryFromStore } = catalogSlice.actions;
export default catalogSlice.reducer;