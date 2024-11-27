import React, { useState } from "react";
import { Modal, Button, TextInput, Dropdown } from "flowbite-react";
import { FaCarrot, FaAppleAlt, FaFish, FaBreadSlice } from "react-icons/fa";
import { TbMeat } from "react-icons/tb";
import { GiMilkCarton } from "react-icons/gi";
import { MdOutlineEmojiFoodBeverage } from "react-icons/md";
import { createCategory } from "../lib/fetchCatalog";
import { useDispatch } from "react-redux";
import { useAuth } from "../hooks/useAuth";
import { addCategory } from "../redux/catalogSlice";
import toast from "react-hot-toast";

interface AddCategoryModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function AddCategoryModal({ isOpen, onClose }: AddCategoryModalProps) {
    const [categoryName, setCategoryName] = useState("");
    const { acquireToken } = useAuth();
    const dispatch = useDispatch();
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    // Validate input
    const validateName = (name: string) => {
        if (name.trim().length === 0) {
            setError("Category name cannot be empty.");
            return false;
        }
        if (name.trim().length < 3) {
            setError("Category name must be at least 3 characters long.");
            return false;
        }
        setError(null);
        return true;
    };

    const handleChangeName = (e: React.ChangeEvent<HTMLInputElement>) => {
        const name = e.target.value;
        setCategoryName(name);
        validateName(name);
    };

    const handleClose = () => {
        // Reset fields when closing the modal
        setCategoryName("");
        setError(null);
        onClose();
      };

    const handleSave = async () => {
        if (!validateName(categoryName)) {
            return;
        }

        setIsSaving(true);

        try {
            await acquireToken();
            const newCategory = await createCategory(categoryName);
            if (newCategory) {
                console.log(newCategory);
                dispatch(addCategory(newCategory));
                toast.success(`Category "${newCategory.name}" created successfully!`);
            }
            setCategoryName("");
            onClose();

            // Reset form
            setCategoryName("");
            setError(null);
            onClose();
        } catch (error) {
            console.error("Error saving category:", error);
            toast.error("Failed to save the category. Please try again.");
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <Modal show={isOpen} onClose={handleClose}>
            <Modal.Header>Add New Category</Modal.Header>
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
                            value={categoryName}
                            onChange={handleChangeName}
                            className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                        />
                        {error && <p className="text-sm text-red-600 mt-1">{error}</p>}
                    </div>

                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button
                    color="purple"
                    onClick={handleSave}
                    disabled={!categoryName || !!error || isSaving}
                >
                    {isSaving ? "Saving..." : "Save"}
                </Button>
                <Button color="light" onClick={handleClose}>
                    Cancel
                </Button>
            </Modal.Footer>
        </Modal>
    );
}
