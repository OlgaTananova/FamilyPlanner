import React, { useEffect, useState } from "react";
import { Modal, Button, TextInput, Checkbox } from "flowbite-react";
import { useDispatch } from "react-redux";
import { toast } from "react-hot-toast";
import ConfirmationModal from "../catalog/ConfirmationModal";
import { deleteShoppingList, updateShoppingList } from "../lib/fetchShoppingLists";
import { deleteShoppingListFromStore, updateShoppingListInStore } from "../redux/shoppingListSlice";
import { useAuth } from "../hooks/useAuth";


interface EditShoppingListModalProps {
    isOpen: boolean;
    onClose: () => void;
    shoppingList: {
        id: string;
        heading: string;
        salesTax: number;
        isArchived: boolean;
    };
}

export default function EditShoppingListModal({
    isOpen,
    onClose,
    shoppingList,
}: EditShoppingListModalProps) {
    const [heading, setHeading] = useState<string>(shoppingList.heading || "");
    const [salesTax, setSalesTax] = useState<number>(shoppingList.salesTax || 0);
    const [isArchived, setIsArchived] = useState<boolean>(shoppingList.isArchived || false);
    const [isDeleting, setIsDeleting] = useState<boolean>(false);
    const [isSaving, setIsSaving] = useState<boolean>(false);
    const [isConfirmModalOpen, setIsConfirmModalOpen] = useState(false);
    const dispatch = useDispatch();
    const { acquireToken } = useAuth();

    const validateHeading = (name: string) => name.trim().length >= 3;
    const validateSalseTax = (tax: number) => tax >= 0;

    useEffect(() => {
        if (shoppingList) {
            setHeading(shoppingList.heading);
            setSalesTax(Number(shoppingList.salesTax));
            setIsArchived(shoppingList.isArchived);
        }
    }, [shoppingList]);

    const handleSave = async () => {
        if (!validateHeading(heading) || !validateSalseTax(salesTax)) {
            return;
        }
        try {
            setIsSaving(true);
            await acquireToken();
            const updatedShoppingList = await updateShoppingList({ id: shoppingList.id, heading, salesTax, isArchived });
            if (updatedShoppingList) {
                dispatch(updateShoppingListInStore(updatedShoppingList));
                toast.success("Shopping list updated successfully!");
                onClose();
            }
        } catch (error) {
            console.error("Error updating shopping list:", error);
            toast.error("Failed to update the shopping list.");
        } finally {
            setIsSaving(false);
        }
    };

    const handleDelete = async () => {

        try {
            setIsDeleting(true);
            const result = await deleteShoppingList(shoppingList.id);
            if (result) {
                dispatch(deleteShoppingListFromStore(shoppingList.id));
                toast.success("Shopping list deleted successfully!");
                setIsConfirmModalOpen(false);
                onClose();
            }
        } catch (error) {
            console.error("Error deleting shopping list:", error);
            toast.error("Failed to delete the shopping list.");
        } finally {
            setIsDeleting(false);
        }
    };

    const handleCancel = () => {
        // Reset form values to the current shopping list
        setHeading(shoppingList.heading);
        setSalesTax(shoppingList.salesTax);
        setIsArchived(shoppingList.isArchived);
        onClose();
    };

    return (
        <>
            <Modal show={isOpen} onClose={handleCancel}>
                <Modal.Header>Edit Shopping List</Modal.Header>
                <Modal.Body>
                    <div className="space-y-6">
                        {/* Heading Input */}
                        <div>
                            <label htmlFor="shopping-list-heading" className="block mb-2 text-sm font-medium text-gray-900">
                                Shopping List Heading
                            </label>
                            <input
                                id="shopping-list-heading"
                                type="text"
                                placeholder="Enter shopping list heading"
                                value={heading}
                                onChange={(e) => setHeading(e.target.value)}
                                className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                            />
                            {!validateHeading(heading) && (
                                <p className="text-sm text-red-600 mt-1">Heading must be at least 3 characters long.</p>
                            )}
                        </div>

                        {/* Sales Tax Input */}
                        <div>
                            <label htmlFor="sales-tax" className="block mb-2 text-sm font-medium text-gray-900">
                                Sales Tax
                            </label>
                            <input
                                id="sales-tax"
                                type="number"
                                value={Number(salesTax) || ""}
                                onChange={(e) => {
                                    const value = e.target.value;
                                    setSalesTax(value === "" ? 0 : Number(value));
                                }}
                                className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                            />
                            {!validateSalseTax(salesTax) && (
                                <p className="text-sm text-red-600 mt-1">Sales tax must be equal or greater to 0.</p>
                            )}
                        </div>

                        {/* Archived Checkbox */}
                        <div className="flex items-center">
                            <Checkbox
                                id="is-archived"
                                checked={isArchived}
                                className="w-5 h-5 text-purple-600 bg-gray-100 border-gray-300 rounded focus:ring-purple-500 focus:ring-2 checked:bg-purple-600 checked:border-purple-600"
                                onChange={(e) => setIsArchived(e.target.checked)}
                            />
                            <label htmlFor="is-archived" className="ml-2 text-sm font-medium text-gray-900">
                                Mark as Archived
                            </label>
                        </div>
                    </div>
                </Modal.Body>
                <Modal.Footer>
                    <div className="flex justify-between w-full">
                        {/* Delete Button */}
                        <Button color="red" onClick={() => setIsConfirmModalOpen(true)} disabled={isDeleting}>
                            {isDeleting ? "Deleting..." : "Delete"}
                        </Button>

                        {/* Save and Cancel Buttons */}
                        <div className="flex space-x-4">
                            <Button color="purple" onClick={handleSave} className="focus:ring-purple-500 focus:ring-2 checked:bg-purple-600 checked:border-purple-600"
                                disabled={isSaving || !validateHeading(heading) || !validateSalseTax(salesTax)}
                            >
                                {isSaving ? "Saving..." : "Save"}
                            </Button>
                            <Button color="light" onClick={handleCancel} className="focus:ring-purple-500 focus:ring-2 checked:bg-purple-600 checked:border-purple-600">
                                Cancel
                            </Button>
                        </div>
                    </div>
                </Modal.Footer>
            </Modal>
            <ConfirmationModal isOpen={isConfirmModalOpen} onClose={() => setIsConfirmModalOpen(false)} onConfirm={handleDelete}
                message="Are you sure you want to delete this shopping list? This action cannot be undone." />
        </>
    );
}
