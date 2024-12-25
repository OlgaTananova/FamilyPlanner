import React, { useState, useRef, useEffect } from "react";
import { ShoppingListItem, updateShoppingListInStore } from "../redux/shoppingListSlice";
import { useAuth } from "../hooks/useAuth";
import { updateShoppingListItem } from "../lib/fetchShoppingLists";
import { useDispatch } from "react-redux";
import toast from "react-hot-toast";
import { Button } from "flowbite-react";

interface ShoppingListItemProps {
    item: ShoppingListItem;
}

export default function ShoppingListItemComponent({ item }: ShoppingListItemProps) {
    const [isTooltipOpen, setIsTooltipOpen] = useState(false);
    const [quantity, setQuantity] = useState(item.quantity);
    const [pricePerUnit, setPricePerUnit] = useState(item.pricePerUnit);
    const [isFinished, setIsFinished] = useState<boolean>(item.status === "Finished");
    const [price, setPrice] = useState(item.price);
    const [unit, setUnit] = useState(item.unit || "pcs");
    const [isSaving, setIsSaving] = useState(false);
    const { acquireToken } = useAuth();
    const dispatch = useDispatch();

    const units = ["pcs", "gal", "lb", "oz", "carton", "fl_oz"];
    const tooltipRef = useRef<HTMLDivElement | null>(null);

    const validateQuantity = (qt: number) => qt >= 1;
    const validatePricePerUnit = (price: number) => price >= 0;
    const validatePrice = (price: number) => price >= 0;


    const onUpdateItem = async () => {
        try {
            setIsSaving(true);
            await acquireToken();
            const status = isFinished ? "Finished" : "Pending";
            const shoppingList = await updateShoppingListItem(item.shoppingListId, item.id, { unit, quantity, pricePerUnit, price, status });
            if (shoppingList) {
                dispatch(updateShoppingListInStore(shoppingList));
                toast.success("Item updated successfully.");
            }
        } catch (error) {
            console.error(error);
            toast.error("Failed to update item.");
        } finally {
            handleCloseTooltip();
            setIsSaving(false);
        }
    }

    // TODO: Implement handleCheckboxChange properly
    const handleCheckboxChange = () => {
        console.log(`before ${isFinished}`);
        setIsFinished(!isFinished);
        //onUpdateItem();
        console.log(`after ${isFinished}`);
    };

    const handleOpenTooltip = () => setIsTooltipOpen(true);

    const handleCloseTooltip = () => {
        setIsTooltipOpen(false);
        resetInputs();
    };

    const resetInputs = () => {
        setQuantity(Number(item.quantity));
        setPricePerUnit(Number(item.pricePerUnit));
        setPrice(Number(item.price));
        setUnit(item.unit);
    };

    const handleSave = () => {
        onUpdateItem();
    };

    // Close tooltip when clicking outside
    useEffect(() => {
        const handleClickOutside = (event: MouseEvent) => {
            if (tooltipRef.current && !tooltipRef.current.contains(event.target as Node)) {
                handleCloseTooltip();
            }
        };

        if (isTooltipOpen) {
            document.addEventListener("mousedown", handleClickOutside);
        }

        return () => document.removeEventListener("mousedown", handleClickOutside);
    }, [isTooltipOpen]);

    useEffect(() => {
        resetInputs();
    }, [item]);

    return (
        <li className="relative w-full flex justify-between items-center px-4 py-3 bg-gray-50 hover:bg-purple-100 rounded-lg border border-gray-300 shadow-sm transition duration-200">
            {/* Checkbox and Item Details */}
            <div className="flex items-center space-x-4">
                <input
                    type="checkbox"
                    checked={isFinished}
                    onChange={handleCheckboxChange}
                    className="w-5 h-5 text-purple-600 bg-gray-100 border-gray-300 rounded focus:ring-purple-500 focus:ring-2"
                />
                <div>
                    <span className={`font-medium ${item.isOrphaned ? "line-through text-gray-400" : "text-gray-700"}`}>
                        {item.name}
                    </span>
                    <span className="text-sm text-gray-500 ml-2">
                        ({item.quantity} {item.unit})
                    </span>
                </div>
            </div>
            {/* Open Tooltip */}
            <button
                onClick={handleOpenTooltip}
                className="text-sm font-medium text-gray-700 hover:text-purple-600"
            >
                Edit
            </button>

            {/* Tooltip */}
            {isTooltipOpen && (
                <div
                    ref={tooltipRef}
                    className="absolute z-10 top-full right-0 mt-2 w-72 p-4 bg-white border border-gray-300 shadow-lg rounded-lg"
                >
                    <h4 className="text-md font-semibold text-gray-800 mb-2">Edit Item</h4>
                    <div className="space-y-4">
                        <div>
                            <label className="block text-xs font-medium text-gray-700">Quantity</label>
                            <input
                                type="number"
                                value={Number(quantity) || ""}
                                onChange={(e) => {
                                    const value = parseFloat(e.target.value);
                                    setQuantity(value);
                                    if (pricePerUnit !== 0) {
                                        const price = value * pricePerUnit;
                                        setPrice(price);
                                    } else if (price !== 0) {
                                        const pricePerUnit = price / value;
                                        setPricePerUnit(pricePerUnit);
                                    }
                                }}
                                className="w-full text-xs border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                            />
                            {!validateQuantity(quantity) && (
                                <p className="text-xs text-red-600 mt-1">Quantity must be greater or equal to 1.</p>
                            )}
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-700">Price Per Unit</label>
                            <input
                                type="number"
                                value={Number(pricePerUnit) || ""}
                                onChange={(e) => {
                                    const value = parseFloat(e.target.value);
                                    setPricePerUnit(value);
                                    if (quantity !== 0) {
                                        const price = value * quantity;
                                        setPrice(price);
                                    }
                                }}
                                className="w-full text-xs border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                            />
                            {!validatePricePerUnit(pricePerUnit) && (
                                <p className="text-xs text-red-600 mt-1">Price per unit must be greater or equal to 0.</p>
                            )}
                        </div>
                        <div>
                            <label className="block text-xs font-medium text-gray-700">Price</label>
                            <input
                                type="number"
                                value={Number(price) || ""}
                                onChange={(e) => {
                                    let value = parseFloat(e.target.value);
                                    setPrice(value);
                                    if (value !== 0 && pricePerUnit !== 0) {
                                        const pricePerUnit = value / quantity;
                                        setPricePerUnit(pricePerUnit);
                                    }
                                }}
                                className="w-full text-xs border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                            />
                            {!validatePrice(price) && (
                                <p className="text-xs text-red-600 mt-1">Price must be greater or equal to 0.</p>
                            )}
                        </div>
                    </div>
                    {/* Unit Select Dropdown */}
                    <div>
                        <label className="block text-xs font-medium mt-2 text-gray-700">Unit</label>
                        <select
                            value={unit}
                            onChange={(e) => setUnit(e.target.value)}
                            className="w-full text-xs border-gray-300 rounded-lg focus:ring-purple-500 focus:border-purple-500"
                        >
                            {units.map((u) => (
                                <option key={u} value={u}>
                                    {u}
                                </option>
                            ))}
                        </select>
                    </div>
                    <div className="flex justify-between mt-4">
                        <Button
                            size="xs"
                            onClick={handleSave}
                            disabled={isSaving || !validateQuantity(quantity) || !validatePricePerUnit(pricePerUnit) || !validatePrice(price)}
                            className="px-4 py-2 bg-purple-500 text-white rounded-lg hover:bg-purple-600"
                        >
                            {isSaving ? "Saving..." : "Save"}
                        </Button>
                        <Button
                            size="xs"
                            onClick={() => { }}
                            className="px-4 py-2 bg-red-500 text-white rounded-lg hover:bg-red-600"
                        >
                            Delete
                        </Button>
                        <Button
                            size="xs"
                            onClick={handleCloseTooltip}
                            className="px-4 py-2 bg-gray-200 text-gray-700 rounded-lg hover:bg-gray-300"
                        >
                            Cancel
                        </Button>
                    </div>
                </div>
            )}
        </li>
    );
}
