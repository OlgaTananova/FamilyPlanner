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
import { useAuth } from "../hooks/useAuth";
import Link from "next/link";
import AddCategoryModal from "./AddCategoryModal";
import AddNewItemModal from "./AddItemModal";
import EditItemModal from "./EditItemModal";
import EditCategoryModal from "./EditCategoryModal";
import CatalogNotificationHandler from "./CatalogNotificationHandler";



export default function Catalog() {
  const categories = useSelector((state: RootState) => state.categories.categories);
  const itemsWOCategories = useSelector((state: RootState) => state.categories.itemsWOCategories);
  const [showOnlyItems, setShowOnlyItems] = useState(false);
  const [isAddCategoryModalOpen, setIsAddCategoryModalOpen] = useState(false);
  const [isAddItemModalOpen, setIsAddItemModalOpen] = useState(false);
  const [isEditItemModalOpen, setIsEditItemModalOpen] = useState(false);
  const [isEditCategoryModalOpen, setIsEditCategoryModalOpen] = useState(false);
  const [editedItem, setEditedItem] = useState<{ id: string, name: string, categoryId: string }>({ id: "", name: "", categoryId: "" });
  const [editedCategory, setEditedCategory] = useState<{ id: string, name: string, items: Item[] }>({ id: "", name: "", items: [] });
  const { acquireToken } = useAuth();
  const dispatch = useDispatch();


  const handleShowOnlyItems = () => {

    setShowOnlyItems(!showOnlyItems);
  };

  if (categories.length == 0) {
    return (
      <div className="container mx-auto px-4 py-6 relative">
        <Link href={("/")}
          className="absolute top-0 right-0 text-gray-600 hover:text-gray-900"
          aria-label="Close Profile"
        >
          ✖
        </Link>
        <div className="flex flex-col">
          {/* Heading */}
          <h1 className="text-2xl font-bold text-purple-700">Catalog</h1>
          <p className="text-xl mt-3">No catalog data is avaliable.</p>

        </div>
      </div>

    )
  }


  // Render catalog
  return (
    <>
      <div className="container mx-auto px-4 py-6 relative">
        <Link href={("/")}
          className="absolute top-0 right-0 text-gray-600 hover:text-gray-900"
          aria-label="Close Profile"
        >
          ✖
        </Link>
        {/* Catalog Header */}
        <div className="flex flex-col sm:flex-row justify-between items-center mb-6">
          <div className="flex items-center space-x-4">
            {/* Heading */}
            <h1 className="text-2xl font-bold text-purple-700">Catalog</h1>
            {/* Dropdown Button */}
            <DropdownMenu showOnlyItems={showOnlyItems}
              handleShowOnlyItems={handleShowOnlyItems}
              setIsAddCategoryModalOpen={setIsAddCategoryModalOpen}
              setIsAddItemModalOpen={setIsAddItemModalOpen} />
          </div>

          {/* Search Bar */}
          <div className="mt-4 sm:mt-0">
            <div className="relative w-full sm:w-80">
              <input
                id="search"
                type="text"
                placeholder="Search categories and items..."
                className="w-full px-4 py-2 text-sm text-gray-900 bg-white border border-gray-300 rounded-lg shadow-sm focus:ring-2 focus:ring-purple-500 focus:border-purple-500 focus:outline-none"
              />

              <div className="absolute inset-y-0 right-3 flex items-center pointer-events-none text-gray-400">
                <HiSearch />
              </div>
            </div>
          </div>
        </div>
        {/*Modals*/}
        <AddCategoryModal isOpen={isAddCategoryModalOpen} onClose={() => setIsAddCategoryModalOpen(false)} />
        <AddNewItemModal isOpen={isAddItemModalOpen} onClose={() => setIsAddItemModalOpen(false)} />
        <EditItemModal isOpen={isEditItemModalOpen} onClose={() => setIsEditItemModalOpen(false)} item={editedItem} />
        <EditCategoryModal isOpen={isEditCategoryModalOpen} onClose={() => setIsEditCategoryModalOpen(false)} category={{
          id: editedCategory.id,
          name: editedCategory.name,
          items: editedCategory.items
        }} />
        {/* Render Content */}
        {!showOnlyItems ? (
          <div className="mt-4">
            {categories.map((category) => (
              <CategoryCard
                key={category.id}
                id={category.id}
                name={category.name}
                items={category.items}
                setEditedCategory={setEditedCategory}
                setEditedItem={setEditedItem}
                setIsEditItemModalOpen={setIsEditItemModalOpen}
                setIsEditCategoryModalOpen={setIsEditCategoryModalOpen}
              />
            ))}
          </div>
        ) : (
          <div className="mt-4 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
            {itemsWOCategories.map((item) => (
              <ItemComponent
                key={item.id}
                id={item.id}
                name={item.name}
                categoryId={item.categoryId}
                setEditedItem={setEditedItem}
                setIsEditItemModalOpen={setIsEditItemModalOpen}
              />
            ))}
          </div>
        )}
      </div >
      <CatalogNotificationHandler />
    </>
  );
}
