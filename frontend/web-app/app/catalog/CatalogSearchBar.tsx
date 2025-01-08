import React, { useState } from "react";
import { HiSearch, HiX } from "react-icons/hi";
import { fetchCatalogSearchResults } from "../lib/fetchCatalog";
import { useAuth } from "../hooks/useAuth";
import { Item } from "../redux/catalogSlice";
import toast from "react-hot-toast";

interface SearchBarProps {
    onSearch: (results: Item[] | null) => void;
}

export default function CatalogSearchBar({ onSearch }: SearchBarProps) {
    const [query, setQuery] = useState<string>("");
    const [isLoading, setIsLoading] = useState(false);
    const { acquireToken } = useAuth();

    // Handle search input
    const handleSearch = async () => {
        if (!query.trim()) return;

        setIsLoading(true);
        try {
            await acquireToken();
            const results = await fetchCatalogSearchResults(query); // Request data from the server
            onSearch(results);
        } catch (error) {
            console.error("Error searching catalog:", error);
            toast.error("Error searching catalog.")
        } finally {
            setIsLoading(false);
        }
    };

    // Clear the search query
    const handleClear = () => {
        setQuery("");
    };

    return (
        <div className="mt-4 sm:mt-0">
            <div className="relative w-full sm:w-80">
                {/* Input Field */}
                <input
                    id="search"
                    type="text"
                    value={query}
                    onChange={(e) => setQuery(e.target.value)}
                    placeholder="Search catalog and items..."
                    className="w-full px-4 py-2 text-sm text-gray-900 bg-white border border-gray-300 rounded-lg shadow-sm focus:ring-2 focus:ring-purple-500 focus:border-purple-500 focus:outline-none transition-all duration-200 ease-in-out"

                />
                {/* Clear Button */}
                {query && (
                    <button
                        onClick={handleClear}
                        className="absolute inset-y-0 right-12 px-2 flex items-center text-gray-500 hover:text-purple-500 transition-all duration-200"
                    >
                        <HiX className="w-5 h-5" />
                    </button>
                )}
                {/* Search Button */}
                <button
                    onClick={handleSearch}
                    disabled={isLoading}
                    className={`absolute inset-y-0 right-0 px-4 flex items-center justify-center bg-purple-500 text-white rounded-r-lg hover:bg-purple-600 focus:ring-2 focus:ring-purple-400 transition-all duration-200 ${isLoading ? "opacity-50 cursor-not-allowed" : ""
                        }`}
                >
                    <HiSearch className="w-5 h-5" />
                </button>


            </div>
        </div>
    );
}
