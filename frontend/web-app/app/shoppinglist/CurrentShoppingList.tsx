import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";
import { ShoppingListItem } from "../redux/shoppingListSlice";


export default function CurrentShoppingList() {
    const shoppingList = useSelector((state: RootState) => state.shoppinglists.currentShoppingList);
    const [groupedItems, setGroupedItems] = useState<Record<string, ShoppingListItem[]>>({});

    useEffect(() => {
        if (shoppingList?.items?.length) {
            const grouped = shoppingList.items.reduce<Record<string, ShoppingListItem[]>>((acc, item) => {
                acc[item.categoryName] = acc[item.categoryName] || [];
                acc[item.categoryName].push(item);
                return acc;
            }, {});
            setGroupedItems(grouped);
        } else {
            setGroupedItems({});
        }
    }, [shoppingList]);

    if (!shoppingList) {
        return <p className="text-gray-500">No shopping list selected.</p>;
    }


    return (
        <div>
            <h2 className="text-lg font-semibold text-gray-800 mb-4">{shoppingList.heading}</h2>
            <div className="space-y-6">
                {Object.entries(groupedItems).map(([category, items]) => (
                    <div key={category}>
                        <h3 className="text-md font-bold text-purple-700 mb-2">{category}</h3>
                        <ul className="space-y-2">
                            {items.map((item) => (
                                <li
                                    key={item.id}
                                    className="flex justify-between items-center bg-purple-50 p-2 rounded-lg shadow-sm"
                                >
                                    <div>
                                        <span className="font-medium text-gray-700">{item.name}</span>
                                        <span className="text-sm text-gray-500 ml-2">
                                            ({item.quantity} {item.unit})
                                        </span>
                                    </div>
                                    <span
                                        className={`text-sm font-medium ${item.status === "Finished" ? "text-green-600" : "text-red-600"
                                            }`}
                                    >
                                        {item.status}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
        </div>
    );

}
