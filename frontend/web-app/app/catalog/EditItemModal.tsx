import React, { useState } from "react";
import { Modal, Button, TextInput, Select } from "flowbite-react";
import { toast } from "react-hot-toast";
import { useSelector, useDispatch } from "react-redux";
import { RootState } from "../redux/store";
//import { updateItem, deleteItem } from "../lib/itemService";
//import { updateItemInStore, removeItemFromStore } from "../redux/catalogSlice";

interface EditItemModalProps {
    isOpen: boolean;
    onClose: () => void;
    item: {
        id: string;
        name: string;
        categoryId: string;
    };
}

export default function EditItemModal({ isOpen, onClose, item }: EditItemModalProps) {
    const categories = useSelector((state: RootState) => state.categories.categories || []);
    const dispatch = useDispatch();

    const [itemName, setItemName] = useState(item.name);
    const [selectedCategory, setSelectedCategory] = useState(item.categoryId);
    const [isSaving, setIsSaving] = useState(false);

    const validateName = (name: string) => name.trim().length >= 3;

    const handleSave = async () => {
        if (!validateName(itemName) || !selectedCategory) {
            toast.error("Please fill in all fields correctly.");
            return;
        }

        setIsSaving(true);

        try {
            //const updatedItem = await updateItem(item.id, itemName, selectedCategory);
            //   if (updatedItem) {
            //     dispatch(updateItemInStore(updatedItem));
            //     toast.success(`Item "${itemName}" updated successfully!`);
            //     onClose();
            //}
        } catch (error) {
            console.error("Error updating item:", error);
            toast.error("Failed to update item. Please try again.");
        } finally {
            setIsSaving(false);
        }
    };

    const handleDelete = async () => {
        if (!window.confirm("Are you sure you want to delete this item?")) {
            return;
        }

        try {
            //   await deleteItem(item.id);
            //   dispatch(removeItemFromStore(item.id));
            //   toast.success("Item deleted successfully!");
            //   onClose();
        } catch (error) {
            console.error("Error deleting item:", error);
            toast.error("Failed to delete item. Please try again.");
        }
    };

    return (
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
                                <option key={category.id} value={category.id}>
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
                    <Button color="red" onClick={handleDelete}>
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
    );
}
