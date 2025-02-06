"use client";
import Link from "next/link";
import { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { Item } from "../redux/catalogSlice";
import { RootState } from "../redux/store";
import AddCategoryModal from "./AddCategoryModal";
import AddNewItemModal from "./AddItemModal";
import CatalogSearchBar from "./CatalogSearchBar";
import CategoryCard from "./CategoryCard";
import DropdownMenu from "./DropdownMenu";
import EditCategoryModal from "./EditCategoryModal";
import EditItemModal from "./EditItemModal";
import ItemComponent from "./Item";
import SearchResults from "./SearchResults";



export default function Catalog() {
  const categories = useSelector((state: RootState) => state.categories.categories);
  const itemsWOCategories = useSelector((state: RootState) => state.categories.itemsWOCategories);
  const [showOnlyItems, setShowOnlyItems] = useState(false);
  const [isAddCategoryModalOpen, setIsAddCategoryModalOpen] = useState(false);
  const [isAddItemModalOpen, setIsAddItemModalOpen] = useState(false);
  const [isEditItemModalOpen, setIsEditItemModalOpen] = useState(false);
  const [isEditCategoryModalOpen, setIsEditCategoryModalOpen] = useState(false);
  const [editedItem, setEditedItem] = useState<{ id: string, name: string, categorySKU: string, sku: string }>({ id: "", name: "", categorySKU: "", sku: "" });
  const [editedCategory, setEditedCategory] = useState<{ id: string, name: string, sku: string, items: Item[] }>({ id: "", name: "", sku: "", items: [] });
  const [showTooltip, setShowTooltip] = useState(true);
  const [searchResults, setSearchResults] = useState<Item[] | null>();
  const [isSearching, setIsSearching] = useState(false);


  useEffect(() => {
    const timer = setTimeout(() => setShowTooltip(false), 5000);
    return () => clearTimeout(timer);
  }, [])


  const handleShowOnlyItems = () => {

    setShowOnlyItems(!showOnlyItems);
  };

  // Handle search results from the server
  const handleSearchResults = (results: Item[] | null) => {
    setSearchResults(results);
    setIsSearching(true);
  };

  // Clear the search and return to the catalog
  const handleClearSearch = () => {
    setSearchResults([]);
    setIsSearching(false);
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
    <div className="container mx-auto px-4 py-6 relative">
      {/* Close Button */}
      <Link
        href={"/"}
        className="absolute top-0 right-0 text-gray-600 hover:text-gray-900"
        aria-label="Close Profile"
      >
        ✖
      </Link>

      {/* Catalog Header */}
      <div className="flex flex-col sm:flex-row justify-between items-center mb-6">
        <div className="flex items-center space-x-4">
          <h1 className="text-2xl font-bold text-purple-700">Catalog</h1>
          <div className="relative">
            <DropdownMenu
              showOnlyItems={showOnlyItems}
              handleShowOnlyItems={handleShowOnlyItems}
              setIsAddCategoryModalOpen={setIsAddCategoryModalOpen}
              setIsAddItemModalOpen={setIsAddItemModalOpen}
            />
            {showTooltip && (
              <div className="absolute top-5 -left-1/2 w-40 transform -translate-x-1/2 px-3 py-1 text-xs text-white bg-purple-500 rounded-lg shadow-lg animate-fade-in-out">
                Add a new catalog item
              </div>
            )}
          </div>
        </div>
      </div>

      {/* Search Bar */}
      <CatalogSearchBar onSearch={handleSearchResults} />

      {/* Modals */}
      <AddCategoryModal
        isOpen={isAddCategoryModalOpen}
        onClose={() => setIsAddCategoryModalOpen(false)}
      />
      <AddNewItemModal
        isOpen={isAddItemModalOpen}
        onClose={() => setIsAddItemModalOpen(false)}
      />
      <EditItemModal
        isOpen={isEditItemModalOpen}
        onClose={() => setIsEditItemModalOpen(false)}
        item={editedItem}
      />
      <EditCategoryModal
        isOpen={isEditCategoryModalOpen}
        onClose={() => setIsEditCategoryModalOpen(false)}
        category={{
          id: editedCategory.id,
          name: editedCategory.name,
          items: editedCategory.items,
          sku: editedCategory.sku,
        }}
      />

      {/* Conditional Rendering: Search Results or Catalog */}
      {isSearching ? (
        // Search Results
        <SearchResults results={searchResults || null} onClearSearch={handleClearSearch} />
      ) : showOnlyItems ? (
        // Items Without Categories View
        <div className="mt-4 grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 gap-4">
          {itemsWOCategories.map((item) => (
            <ItemComponent
              key={item.sku}
              id={item.sku}
              sku={item.sku}
              name={item.name}
              categorySKU={item.categorySKU}
              setEditedItem={setEditedItem}
              setIsEditItemModalOpen={setIsEditItemModalOpen}
              showEditItemButton={true}
            />
          ))}
        </div>
      ) : (
        // Categories View
        <div className="mt-4">
          {categories.map((category) => (
            <CategoryCard
              key={category.sku}
              id={category.sku}
              sku={category.sku}
              name={category.name}
              items={category.items}
              setEditedCategory={setEditedCategory}
              setEditedItem={setEditedItem}
              setIsEditItemModalOpen={setIsEditItemModalOpen}
              setIsEditCategoryModalOpen={setIsEditCategoryModalOpen}
            />
          ))}
        </div>
      )}
    </div>
  );
}
