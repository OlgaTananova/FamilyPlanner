import { Button, Modal } from "flowbite-react";
import { useState } from "react";
import { toast } from "react-hot-toast";
import { useDispatch, useSelector } from "react-redux";
import { useCatalogApi } from "../hooks/useCatalogApi";
import { addItem } from "../redux/catalogSlice";
import { RootState } from "../redux/store";


interface AddNewItemModalProps {
    isOpen: boolean;
    onClose: () => void;
}

export default function AddNewItemModal({ isOpen, onClose }: AddNewItemModalProps) {
    const categories = useSelector((state: RootState) => state.categories || []);
    const dispatch = useDispatch();
    const { createItem } = useCatalogApi();
    const [itemName, setItemName] = useState("");
    const [selectedCategory, setSelectedCategory] = useState<string | null>(null);
    const [error, setError] = useState<string | null>(null);
    const [isSaving, setIsSaving] = useState(false);

    const validateName = (name: string) => {
        if (name.trim().length < 3) {
            setError("Item name must be at least 3 characters long.");
            return false;
        }
        setError(null);
        return true;
    };

    const handleClose = () => {
        // Reset fields when closing the modal
        setItemName("");
        setSelectedCategory(null);
        setError(null);
        onClose();
    };

    const handleSave = async () => {
        if (!validateName(itemName) || !selectedCategory) {
            toast.error("Please fill in all fields correctly.");
            return;
        }

        setIsSaving(true);

        try {
            const newItem = await createItem(itemName, selectedCategory);
            if (newItem) {
                dispatch(addItem(newItem));
                setItemName("");
                setSelectedCategory(null);
                handleClose();
            }
        } finally {
            setIsSaving(false);
        }
    };

    return (
        <Modal show={isOpen} onClose={handleClose}>
            <Modal.Header>Add New Item</Modal.Header>
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
                            onBlur={() => validateName(itemName)}
                            className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                        />
                        {error && <p className="text-sm text-red-600 mt-1">{error}</p>}
                    </div>

                    {/* Select Category */}
                    <div>
                        <label htmlFor="category-select" className="block mb-2 text-sm font-medium text-gray-900">
                            Select Category
                        </label>
                        <select
                            id="category-select"
                            value={selectedCategory || ""}
                            onChange={(e) => setSelectedCategory(e.target.value)}
                            className="w-full border-gray-300 rounded-lg focus:ring-2 focus:ring-purple-500 focus:border-purple-500 transition-all duration-300"
                        >
                            <option value="" disabled>
                                Choose a category
                            </option>
                            {categories.categories.map((category) => (
                                <option key={category.sku} value={category.sku}>
                                    {category.name}
                                </option>
                            ))}
                        </select>
                    </div>
                </div>
            </Modal.Body>
            <Modal.Footer>
                <Button
                    color="purple"
                    onClick={handleSave}
                    disabled={isSaving || !itemName || !selectedCategory || !!error}
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
