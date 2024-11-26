"use client";

import { useState } from "react";
import { FaRegEdit } from "react-icons/fa";
import { toast } from "react-hot-toast";
import ItemComponent from "./Item"; // Assume this is the component you created for items
import { Item } from "../redux/catalogSlice";

interface CategoryCardProps {
    id: string;
    name: string;
    items: Item[];
    onEditCategory: () => void;
}

export default function CategoryCard({ id, name, items, onEditCategory }: CategoryCardProps) {
    const [visibleItemsCount, setVisibleItemsCount] = useState(6);

    const handleShowMore = () => {
        setVisibleItemsCount((prev) => Math.min(prev + 3, items.length));
    };

    const handleShowLess = () => {
        setVisibleItemsCount(6);
    };

    return (
        <div
            key={id}
            className="mb-6 p-4 rounded-lg shadow-md border bg-purple-50 border-purple-300"
        >
            {/* Category Header */}
            <div className="flex items-center justify-between">
                <div className="flex items-center space-x-2">
                    <h2 className="text-xl font-semibold text-purple-600">{name}</h2>
                    <button
                        onClick={onEditCategory}
                        className="p-1 rounded-full text-purple-600 hover:bg-purple-100"
                        aria-label="Edit category"
                    >
                        <FaRegEdit size={18} />
                    </button>
                </div>
            </div>

            {/* Items */}
            <div className="mt-4 flex flex-wrap gap-2">
                {items.slice(0, visibleItemsCount).map((item) => (
                    <ItemComponent
                        key={item.id}
                        name={item.name}
                        onAddToCart={() => toast.success(`Added ${item.name} to shopping list!`)}
                        onEdit={() => toast.success(`Editing ${item.name}!`)}
                    />
                ))}
            </div>

            {/* Show More/Show Less Buttons */}
            {items.length > 6 && (
                <div className="mt-4 flex justify-end space-x-2">
                    {visibleItemsCount < items.length && (
                        <button
                            onClick={handleShowMore}
                            className="px-3 py-1 text-sm font-medium text-purple-700 bg-purple-100 rounded-md hover:bg-purple-200"
                        >
                            Show More
                        </button>
                    )}
                    {visibleItemsCount > 6 && (
                        <button
                            onClick={handleShowLess}
                            className="px-3 py-1 text-sm font-medium text-purple-700 bg-purple-100 rounded-md hover:bg-purple-200"
                        >
                            Show Less
                        </button>
                    )}
                </div>
            )}
        </div>
    );
}