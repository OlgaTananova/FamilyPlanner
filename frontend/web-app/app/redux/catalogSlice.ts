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
    categoryName: string;
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
            const existingCategory = state.categories.find(
                (cat) => cat.sku === action.payload.sku
            );
            if (existingCategory) return;

            const newCategory = { ...action.payload, items: action.payload.items || [] };
            state.categories.push(newCategory);
            // Update itemsWOCategories by flattening all category items
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        addItem(state, action: PayloadAction<Item>) {
            const existingCategory = state.categories.find(
                (cat) => cat.sku === action.payload.categorySKU
            );
            const existingItem = existingCategory?.items.find((i) => i.sku == action.payload.sku);
            if (existingItem) return;

            existingCategory?.items.push(action.payload);

            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        updateItemInStore(
            state,
            action: PayloadAction<{ updatedItem: Item; previousCategorySKU: string }>
        ) {
            const { sku, name, categorySKU } = action.payload.updatedItem;
            const { previousCategorySKU } = action.payload;
        
            // Step 1: Remove the item from the previous category if it has changed
            if (categorySKU !== previousCategorySKU) {
                const previousCategory = state.categories.find(
                    (cat) => cat.sku === previousCategorySKU
                );
                if (previousCategory) {
                    previousCategory.items = previousCategory.items.filter(
                        (item) => item.sku !== sku
                    );
                }
            }
        
            // Step 2: Add or update the item in the new category
            const targetCategory = state.categories.find((cat) => cat.sku === categorySKU);
            if (targetCategory) {
                const existingItem = targetCategory.items.find((item) => item.sku === sku);
        
                if (existingItem) {
                    // Update existing item
                    existingItem.name = name;
                } else {
                    // Add the new item
                    targetCategory.items.push(action.payload.updatedItem);
                }
            }
        
            // Step 3: Recalculate the flattened list of items without categories
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        removeItemFromStore(state, action: PayloadAction<string>) {
            state.categories.forEach((category) => {
                category.items = category.items.filter((item) => item.sku !== action.payload);
            });
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        updateCategoryInStore(state, action: PayloadAction<Category>) {
            const category = state.categories.find((cat) => cat.sku === action.payload.sku);
            if (category) {
                category.name = action.payload.name;
            }
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },
        removeCategoryFromStore(state, action: PayloadAction<string>) {
            state.categories = state.categories.filter((cat) => cat.sku !== action.payload);
            state.itemsWOCategories = state.categories
                .flatMap((category) => category.items)
                .sort((a, b) => a.name.localeCompare(b.name));
        },

    }
});

export const { setCategories, clearCategories, addCategory, addItem, updateItemInStore, removeItemFromStore, updateCategoryInStore, removeCategoryFromStore } = catalogSlice.actions;
export default catalogSlice.reducer;