import React, { useState } from "react";
import { useDispatch, useSelector } from "react-redux";
import { Modal, Button, TextInput } from "flowbite-react";
import { RootState } from "../redux/store";
import { Item } from "../redux/catalogSlice";
import { HiChevronDown } from "react-icons/hi";
import toast from "react-hot-toast";
import { addNewShoppingList } from "../lib/fetchShoppingLists";
import { addShoppingList } from "../redux/shoppingListSlice";

interface AddShoppingListModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function AddShoppingListModal({
    isOpen,
    onClose,
}: AddShoppingListModalProps) {
    const items = useSelector((state: RootState) => state.categories.itemsWOCategories);
    const [heading, setHeading] = useState<string>("");
    const [selectedItems, setSelectedItems] = useState<string[]>([]);
    const [dropdownOpen, setDropdownOpen] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const dispatch = useDispatch();

    const handleSave = async () => {
        setIsSaving(true);

        try {
            const newShoppingList = { heading: heading.trim(), SKUs: selectedItems };
            const addedShoppingList = await addNewShoppingList(newShoppingList);
            if (addedShoppingList) {
                dispatch(addShoppingList(addedShoppingList));
                toast.success("Shopping list created successfully!");
            }
            // Reset form
            handleModalClose();
        } catch (error) {
            toast.error("Failed to create the shopping list.");
            console.error(error);
        } finally {
            setIsSaving(false);
        }
    };

    const toggleItemSelection = (itemSKU: string) => {
        setSelectedItems((prevSelected) =>
            prevSelected.includes(itemSKU)
                ? prevSelected.filter((sku) => sku !== itemSKU) // Remove if already selected
                : [...prevSelected, itemSKU] // Add if not selected
        );
    };

    const handleModalClose = () => {
        onClose();
        setHeading("");
        setSelectedItems([]);
        setDropdownOpen(false);
    }

    return (
        <Modal show={isOpen} onClose={onClose}>
            <Modal.Header>Add New Shopping List</Modal.Header>
            <Modal.Body>
                <div className="space-y-6">
                    {/* Heading Input */}
                    <div>
                        <label
                            htmlFor="shopping-list-heading"
                            className="block mb-2 text-sm font-medium text-gray-900"
                        >
                            Shopping List Heading (Optional)
                        </label>
                        <input
                            id="shopping-list-heading"
                            type="text"
                            placeholder="Enter shopping list heading"
                            value={heading}
                            onChange={(e) => setHeading(e.target.value)}
                            className="w-full text-gray-700 border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
                        />
                    </div>

                    {/* Items Dropdown */}
                    <div className="relative">
                        <button
                            onClick={() => setDropdownOpen((prev) => !prev)}
                            className="w-full flex justify-between items-center border border-gray-300 rounded-lg p-2 text-gray-700 bg-white hover:bg-gray-100 focus:ring-2 focus:ring-purple-500"
                        >
                            Add Items (Optional)
                            <HiChevronDown className="w-5 h-5" />
                        </button>
                        {dropdownOpen && (
                            <div className="absolute z-10 mt-2 w-full max-h-40 overflow-y-auto bg-white border border-gray-300 rounded-lg shadow-lg">
                                <ul className="divide-y divide-gray-200">
                                    {items.map((item: Item) => (
                                        <li key={item.id} className="flex items-center p-2 hover:bg-gray-100">
                                            <input
                                                type="checkbox"
                                                id={`item-${item.sku}`}
                                                checked={selectedItems.includes(item.sku)}
                                                onChange={() => toggleItemSelection(item.sku)}
                                                className="mr-2"
                                            />
                                            <label
                                                htmlFor={`item-${item.sku}`}
                                                className="text-gray-700 text-sm"
                                            >
                                                {item.name}
                                            </label>
                                        </li>
                                    ))}
                                </ul>
                            </div>
                        )}
                    </div>

                    {/* Selected Items */}
                    {selectedItems.length > 0 && (
                        <div className="mt-2 text-sm text-gray-600">
                            Selected Items:{" "}
                            {selectedItems
                                .map((sku) => items.find((item: { sku: string; }) => item.sku === sku)?.name || "")
                                .join(", ")}
                        </div>
                    )}
                </div>
            </Modal.Body>
            <Modal.Footer>
                <div className="flex justify-between w-full">
                    <Button
                        color="purple"
                        onClick={handleSave}
                        disabled={isSaving}
                    >
                        {isSaving ? "Saving..." : "Save"}
                    </Button>
                    <Button color="light" onClick={handleModalClose}>
                        Cancel
                    </Button>
                </div>
            </Modal.Footer>
        </Modal>
    );
}
