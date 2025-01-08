import React from "react";
import { Button } from "flowbite-react";
import { Item } from "../redux/catalogSlice";
import ItemComponent from "./Item";

interface SearchResultsProps {
    results: Item[] | null;
    onClearSearch: () => void;
}

export default function SearchResults({ results, onClearSearch }: SearchResultsProps) {
    return (
        <div>
            <div className="flex justify-between items-center mb-4 mt-4">
                <h2 className="text-lg font-semibold text-purple-700">Search Results</h2>
                <Button
                    size="xs"
                    color="purple"
                    onClick={onClearSearch}
                    className="rounded-lg"
                >
                    Clear Search
                </Button>
            </div>

            {results && results.length > 0 ? (
                <ul className="space-y-4">
                    {results.map((item) => (
                        <ItemComponent name={item.name} key={item.sku} id={item.id} categorySKU={item.categorySKU} sku={item.sku}
                            setEditedItem={function (item: { id: string; name: string; categorySKU: string; sku: string; }): void {
                                throw new Error("Function not implemented.");
                            }} showEditItemButton={false} />
                    ))}
                </ul>
            ) : (
                <p className="text-gray-500">No items found.</p>
            )}
        </div>
    );
}
