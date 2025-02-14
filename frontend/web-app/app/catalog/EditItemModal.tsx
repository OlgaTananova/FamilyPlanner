import { Button, Modal } from "flowbite-react";
import { useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import { useDispatch, useSelector } from "react-redux";
import { useCatalogApi } from "../hooks/useCatalogApi";
import { removeItemFromStore, updateItemInStore } from "../redux/catalogSlice";
import { deleteCatalogItemFromShoppingList, updateCatalogItem } from "../redux/shoppingListSlice";
import { RootState } from "../redux/store";
import ConfirmationModal from "./ConfirmationModal";

interface EditItemModalProps {
    isOpen: boolean;
    onClose: () => void;
    item: {
        id: string;
        sku: string;
        name: string;
        categorySKU: string;
    };
}

export default function EditItemModal({ isOpen, onClose, item }: EditItemModalProps) {
    const categories = useSelector((state: RootState) => state.categories.categories || []);
    const dispatch = useDispatch();
    const [itemName, setItemName] = useState(item.name);
    const [selectedCategory, setSelectedCategory] = useState(item.categorySKU);
    const [isSaving, setIsSaving] = useState(false);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    const [currentCategory, setCurrentCategory] = useState(item.categorySKU);
    const { updateItem, deleteItem } = useCatalogApi();

    const validateName = (name: string) => name.trim().length >= 3;

    useEffect(() => {
        if (item) {
            setItemName(item.name);
            setSelectedCategory(item.categorySKU);
            setCurrentCategory(item.categorySKU);
        }
    }, [item]);

    const handleSave = async () => {
        if (!validateName(itemName) || !selectedCategory) {
            toast.error("Please fill in all fields correctly.");
            return;
        }

        setIsSaving(true);

        try {
            const updatedItem = await updateItem(item.sku, itemName, selectedCategory);

            if (updatedItem) {
                dispatch(updateItemInStore(updatedItem));
                dispatch(updateCatalogItem(updatedItem.updatedItem))
                toast.success(`Item "${name}" updated successfully!`);
            }
            onClose();
        }
        finally {
            setIsSaving(false);
        }
    };

    const handleDelete = async () => {

        const result = await deleteItem(item.sku);
        if (result) {
            dispatch(removeItemFromStore(item.sku));
            dispatch(deleteCatalogItemFromShoppingList(item));
            toast.success("Item deleted successfully!");
            setIsConfirmModalOpen(false);
        }
        onClose();

    };

    return (
        <>
            <Modal show={isOpen} onClose={onClose}>
                <Modal.Header>Edit Item</Modal.Header>
                <Modal.Body>
                    <div className="space-y-6">
                        {/* Item Name */}
                        <div>
                            <label htmlFor="item-name" className="block mb-2 text-sm font-medium text-gray-900">
                                Item Name
                            </label>
                            <input
                                id="item-name"
                                type="text"
                                placeholder="Enter item name"
                                value={itemName}
                                onChange={(e) => setItemName(e.target.value)}
                                className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                            />
                            {!validateName(itemName) && (
                                <p className="text-sm text-red-600 mt-1">Item name must be at least 3 characters long.</p>
                            )}
                        </div>

                        {/* Select Category */}
                        <div>
                            <label htmlFor="category-select" className="block mb-2 text-sm font-medium text-gray-900">
                                Select Category
                            </label>
                            <select
                                id="category-select"
                                value={selectedCategory}
                                onChange={(e) => setSelectedCategory(e.target.value)}
                                className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                            >
                                {categories.map((category) => (
                                    <option key={category.sku} value={category.sku}>
                                        {category.name}
                                    </option>
                                ))}
                            </select>
                        </div>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <div className="flex justify-between w-full">
                        {/* Delete Button */}
                        <Button color="red" onClick={() => setIsConfirmModalOpen(true)}>
                            Delete
                        </Button>
                        {/* Save and Cancel Buttons */}
                        <div className="flex space-x-4">
                            <Button
                                color="purple"
                                onClick={handleSave}
                                disabled={isSaving || !validateName(itemName) || !selectedCategory}
                            >
                                {isSaving ? "Saving..." : "Save"}
                            </Button>
                            <Button color="light" onClick={onClose}>
                                Cancel
                            </Button>
                        </div>
                    </div>
                </Modal.Footer>
            </Modal>
            <ConfirmationModal isOpen={isConfirmModalOpen}
                onClose={() => setIsConfirmModalOpen(false)}
                onConfirm={handleDelete}
                message="Are you sure you want to delete this item? This action cannot be undone." />
        </>
    );
}
