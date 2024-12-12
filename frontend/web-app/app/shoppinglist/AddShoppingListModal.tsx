import React, { useState } from "react";
import { Modal, Button, TextInput, Select } from "flowbite-react";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { Item } from "../redux/catalogSlice";
import toast from "react-hot-toast";

interface AddShoppingListModalProps {
    isOpen: boolean;
    onClose: () => void;
    onSave: (shoppingList: { heading: string; items: string[] }) => void;
}

export default function AddShoppingListModal({
    isOpen,
    onClose,
    onSave,
}: AddShoppingListModalProps) {
    const items = useSelector((state: RootState) => state.categories.itemsWOCategories);
    const [heading, setHeading] = useState<string>("");
    const [selectedItems, setSelectedItems] = useState<string[]>([]);
    const [isSaving, setIsSaving] = useState<boolean>(false);

    const handleSave = async () => {
        setIsSaving(true);

        try {
            // Save logic here
            const newShoppingList = { heading: heading.trim(), items: selectedItems };
            onSave(newShoppingList);

            toast.success("Shopping list created successfully!");
            // Reset form
            setHeading("");
            setSelectedItems([]);
            onClose();
        } catch (error) {
            toast.error("Failed to create the shopping list.");
            console.error(error);
        } finally {
            setIsSaving(false);
        }
    };

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
                        <TextInput
                            id="shopping-list-heading"
                            type="text"
                            placeholder="Enter shopping list heading"
                            value={heading}
                            onChange={(e) => setHeading(e.target.value)}
                            className="w-full"
                        />
                    </div>

                    {/* Items MultiSelect */}
                    <div>
                        <label
                            htmlFor="items-select"
                            className="block mb-2 text-sm font-medium text-gray-900"
                        >
                            Add Items (Optional)
                        </label>
                        <Select multiple>
                            <option>Choose an item</option>
                            {items && items.map((item) => (
                                <option key={item.id} value={item.sku}>{item.name}</option>
                            ))}
                        </Select>
                    </div>
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
                    <Button color="light" onClick={onClose}>
                        Cancel
                    </Button>
                </div>
            </Modal.Footer>
        </Modal>
    );
}
