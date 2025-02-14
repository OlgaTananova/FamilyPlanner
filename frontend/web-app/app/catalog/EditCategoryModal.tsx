import React, { useState, useEffect } from "react";
import { Modal, Button, Tooltip } from "flowbite-react";
import { toast } from "react-hot-toast";
import { useDispatch } from "react-redux";

import { removeCategoryFromStore, updateCategoryInStore } from "../redux/catalogSlice";
import ConfirmationModal from "./ConfirmationModal";
import { updateCatalogCategory } from "../redux/shoppingListSlice";
import { useCatalogApi } from "../hooks/useCatalogApi";

interface EditCategoryModalProps {
    isOpen: boolean;
    onClose: () => void;
    category: {
        id: string;
        name: string;
        sku: string;
        items: Array<{ id: string; name: string }>;
    };

}

export default function EditCategoryModal({
    isOpen,
    onClose,
    category,
}: EditCategoryModalProps) {
    const dispatch = useDispatch();

    const [categoryState, setCategoryState] = useState<{ id: string, name: string, sku: string, items: Array<{ id: string; name: string }> }>(category);
    const [isSaving, setIsSaving] = useState(false);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    const { updateCategory, deleteCategory } = useCatalogApi();

    useEffect(() => {
        if (category) {
            setCategoryState(category);
        }
    }, [category]);

    const validateName = (name: string) => name.trim().length >= 3;

    const handleSave = async () => {
        if (!validateName(categoryState.name)) {
            toast.error("Category name must be at least 3 characters long.");
            return;
        }

        setIsSaving(true);

        try {
            const updatedCategory = await updateCategory(categoryState.sku, categoryState.name);
            if (updatedCategory) {
                dispatch(updateCategoryInStore(updatedCategory));
                dispatch(updateCatalogCategory(updatedCategory));
                toast.success(`Category "${categoryState.name}" updated successfully!`);
                onClose();
            }
        } finally {
            setIsSaving(false);
        }
    };

    const handleDeleteConfirm = async () => {
        try {
            const result = await deleteCategory(category.sku);
            if (result) {
                dispatch(removeCategoryFromStore(category.sku));
                toast.success(`Category "${category.name}" deleted successfully!`);
                setIsConfirmModalOpen(false);
                onClose();
            }
        } catch (error) {
            console.error("Error deleting category:", error);
            toast.error("Failed to delete the category. Please try again.");
        }
    };

    return (
        <>
            <Modal show={isOpen} onClose={onClose}>
                <Modal.Header>Edit Category</Modal.Header>
                <Modal.Body>
                    <div className="space-y-6">
                        {/* Category Name */}
                        <div>
                            <label htmlFor="category-name" className="block mb-2 text-sm font-medium text-gray-900">
                                Category Name
                            </label>
                            <input
                                id="category-name"
                                type="text"
                                placeholder="Enter category name"
                                value={categoryState.name}
                                onChange={(e) =>
                                    setCategoryState((prev) => ({
                                        ...prev, // Spread the previous state to preserve other properties
                                        name: e.target.value, // Update the `name` property with the new value
                                    }))}
                                className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                            />
                            {!validateName(categoryState.name) && (
                                <p className="text-sm text-red-600 mt-1">
                                    Category name must be at least 3 characters long.
                                </p>
                            )}
                        </div>
                    </div>
                </Modal.Body>

                <Modal.Footer>

                    <div className="flex justify-between w-full">
                        {/* Delete Button */}
                        <Button
                            color="red"
                            onClick={() => setIsConfirmModalOpen(true)}
                            disabled={categoryState.items.length > 0}>

                            <Tooltip content="Cannot delete a category with items." placement="top" trigger="hover" hidden={categoryState.items.length === 0}>
                                Delete
                            </Tooltip>
                        </Button>

                        {/* Save and Cancel Buttons */}
                        <div className="flex space-x-4">
                            <Button
                                color="purple"
                                onClick={handleSave}
                                disabled={isSaving || !validateName(categoryState.name)}
                            >
                                {isSaving ? "Saving..." : "Save"}
                            </Button>
                            <Button color="light" onClick={onClose}>
                                Cancel
                            </Button>
                        </div>
                    </div>
                </Modal.Footer>
            </Modal >

            {/* Confirmation Modal */}
            < ConfirmationModal isOpen={isConfirmModalOpen} onClose={() => setIsConfirmModalOpen(false)
            }
                onConfirm={handleDeleteConfirm} />
        </>
    );
}
