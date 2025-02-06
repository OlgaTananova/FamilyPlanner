import { Button } from "flowbite-react";
import { toast } from "react-hot-toast";
import { useDispatch, useSelector } from "react-redux";
import { useShoppingListApi } from "../hooks/useShoppingListApi";
import { CatalogItem, updateShoppingListInStore } from "../redux/shoppingListSlice";
import { RootState } from "../redux/store";

export default function FrequentItems() {
    const dispatch = useDispatch();
    const { addShoppingListItems } = useShoppingListApi();

    // Get frequently bought items from the Redux store
    const frequentItems = useSelector((state: RootState) => state.shoppinglists.frequentItems);

    const currentShoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);

    // Handle adding item to the shopping list
    const handleAddItem = async (item: CatalogItem) => {
        if (!currentShoppingList) {
            toast.error("No shopping list selected!");
            return;
        }
        try {
            const updatedShoppingList = await addShoppingListItems(currentShoppingList.id, { skus: [item.sku] });
            if (updatedShoppingList) {
                dispatch(updateShoppingListInStore(updatedShoppingList));
                toast.success(`${item.name} added to the shopping list!`);
                return;
            }
        } catch (error) {
            console.error("Failed to add item to the shopping list", error);
            toast.error("Failed to add item to the shopping list!");
        }
    };

    return (
        <div>
            <h2 className="text-lg font-semibold text-gray-800 mb-4">Frequently Bought Items</h2>
            {frequentItems.length > 0 ? (
                <ul className="space-y-2">
                    {frequentItems.map((item) => (
                        <li key={item.sku} className="flex justify-between items-center p-2 bg-gray-50 hover:bg-purple-100 rounded-lg border border-gray-300 shadow-sm transition duration-200">
                            <span className="font-medium text-gray-700">{item.name}</span>
                            <Button
                                size="xs"
                                color="purple"
                                onClick={() => handleAddItem(item)}
                                className="px-3 py-1 rounded-lg hover:bg-purple-600"
                            >
                                Add
                            </Button>
                        </li>
                    ))}
                </ul>
            ) : (
                <p className="text-gray-500">No frequently bought items available.</p>
            )}
        </div>
    );
}
