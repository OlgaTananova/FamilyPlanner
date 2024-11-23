"use client";

import { useEffect, useState } from "react";
import { toast } from "react-hot-toast";

interface Item {
  id: string;
  name: string;
  ownerId: string;
  family: string;
  isDeleted: boolean;
  categoryId: string;
}

interface Category {
  id: string;
  name: string;
  ownerId: string;
  family: string;
  isDeleted: boolean;
  items: Item[];
}

export default function Catalog() {
  const [categories, setCategories] = useState<Category[]>([]);
  const [loading, setLoading] = useState(false);
  const [error, setError] = useState("");

  // Fetch Categories and Items
  useEffect(() => {
    const fetchCatalogData = async () => {
      setLoading(true);
      setError("");

      try {
        // Fetch categories
        const categoryResponse = await fetch("http://localhost:7001/api/Catalog/categories");
        if (!categoryResponse.ok) {
          throw new Error("Failed to fetch categories.");
        }
        const fetchedCategories: Category[] = await categoryResponse.json();

        // Fetch items
        const itemResponse = await fetch("http://localhost:7001/api/Catalog/items");
        if (!itemResponse.ok) {
          throw new Error("Failed to fetch items.");
        }
        const fetchedItems: Item[] = await itemResponse.json();

        // Associate items with their categories
        const updatedCategories = fetchedCategories.map((category) => ({
          ...category,
          items: fetchedItems.filter((item) => item.categoryId === category.id),
        }));

        setCategories(updatedCategories);
      } catch (error: any) {
        console.error("Error fetching catalog data:", error);
        setError(error.message || "An error occurred.");
        toast.error(error.message || "Failed to fetch catalog data.");
      } finally {
        setLoading(false);
      }
    };

    fetchCatalogData();
  }, []);

  // Render loading state
  if (loading) {
    return (
      <div className="flex justify-center items-center h-screen">
        <p className="text-xl font-semibold text-purple-700">Loading catalog...</p>
      </div>
    );
  }

  // Render error state
  if (error) {
    return (
      <div className="flex justify-center items-center h-screen">
        <p className="text-xl font-semibold text-red-600">{error}</p>
      </div>
    );
  }

  // Render catalog
  return (
    <div className="container mx-auto px-4 py-6">
      <h1 className="text-2xl font-bold text-purple-700 mb-6">Catalog</h1>
      {categories.map((category) => (
        <div
          key={category.id}
          className="mb-6 bg-white rounded-lg shadow-md p-4 border border-gray-200"
        >
          <h2 className="text-xl font-semibold text-purple-600">{category.name}</h2>
          <ul className="mt-4 space-y-2">
            {category.items.map((item) => (
              <li key={item.id} className="text-gray-700">
                - {item.name}
              </li>
            ))}
          </ul>
        </div>
      ))}
    </div>
  );
}
