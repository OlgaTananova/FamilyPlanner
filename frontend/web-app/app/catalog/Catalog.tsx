"use client";

import { AwaitedReactNode, JSXElementConstructor, Key, ReactElement, ReactNode, ReactPortal, useEffect, useState } from "react";
import { toast } from "react-hot-toast";
import { useDispatch, useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { fetchCatalogData } from "../lib/fetchCatalog";
import { Category, Item, setCategories } from "../redux/catalogSlice";
import { Button, Dropdown, TextInput } from "flowbite-react";
import { HiDotsVertical, HiSearch } from "react-icons/hi";
import { MdOutlineAddShoppingCart } from "react-icons/md";
import { FaRegEdit } from "react-icons/fa";
import ItemComponent from "./Item";
import CategoryCard from "./CategoryCard";
import DropdownMenu from "./DropdownMenu";



export default function Catalog() {
  const categories = useSelector((state: RootState) => state.categories || []);
  const [showOnlyItems, setShowOnlyItems] = useState(false);
  const [itemsWOCategories, setItemWOCategories] = useState<Item[]>([]);
  const dispatch = useDispatch();

  // Fetch Categories and Items
  useEffect(() => {
    async function fetchData() {
      try {
        // Fetch categories
        const fetchedCategories = await fetchCatalogData();

        if (fetchedCategories) {
          dispatch(setCategories(fetchedCategories?.categories));
          const allItems = fetchedCategories?.categories.flatMap((category) => category.items).sort((a, b) => a.name.localeCompare(b.name));
          setItemWOCategories(allItems);
        }

      } catch (error: any) {
        console.error("Error fetching catalog data:", error);
        toast.error(error.message || "Failed to fetch catalog data.")
      }
    }
    fetchData();

  }, []);

  const handleShowOnlyItems = () => {

    setShowOnlyItems(!showOnlyItems);
  };


  // Render catalog
  return (
    <div className="container mx-auto px-4 py-6">
      {/* Catalog Header */}
      <div className="flex flex-col sm:flex-row justify-between items-center mb-6">
        <div className="flex items-center space-x-4">
          {/* Heading */}
          <h1 className="text-2xl font-bold text-purple-700">Catalog</h1>
          {/* Dropdown Button */}
          <DropdownMenu showOnlyItems={showOnlyItems} handleShowOnlyItems={handleShowOnlyItems} />
        </div>

        {/* Search Bar */}
        <div className="mt-4 sm:mt-0">
          <TextInput
            id="search"
            type="text"
            placeholder="Search categories and items..."
            icon={HiSearch}
            className="w-full sm:w-80 focus:ring-2 focus:ring-purple-500 focus:border-purple-500"
          />
        </div>
      </div>

      {/* Render Content */}
      {!showOnlyItems ? (
        <div className="mt-4">
          {categories.categories.map((category) => (
            <CategoryCard
              key={category.id}
              id={category.id}
              name={category.name}
              items={category.items}
              onEditCategory={() => toast.success(`Editing ${category.name} category!`)}
            />
          ))}
        </div>
      ) : (
        <div className="mt-4 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          {itemsWOCategories.map((item) => (
            <ItemComponent
              key={item.id}
              name={item.name}
              onAddToCart={() => toast.success(`Added ${item.name} to shopping list!`)}
              onEdit={() => toast.success(`Editing ${item.name}!`)}
            />
          ))}
        </div>
      )}
    </div >
  );
}
