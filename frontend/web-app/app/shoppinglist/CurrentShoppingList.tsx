import React from "react";

interface ShoppingListItem {
    id: string;
    name: string;
    sku: string;
    categoryName: string;
    categorySKU: string;
    shoppingListId: string;
    unit: string;
    quantity: number;
    pricePerUnit: number;
    price: number;
    status: string;
    isOrphaned: boolean;
    family: string;
    ownerId: string;
}

interface CurrentShoppingListProps {
    shoppingList: {
        id: string;
        heading: string;
        createdAt: string;
        items: ShoppingListItem[];
        salesTax: number;
        isArchived: boolean;
        isDeleted: boolean;
        ownerId: string;
        family: string;
    };
}

export default function CurrentShoppingList({ shoppingList }: CurrentShoppingListProps) {
    const groupedItems = shoppingList.items.reduce((acc, item) => {
        if (!acc[item.categoryName]) {
            acc[item.categoryName] = [];
        }
        acc[item.categoryName].push(item);
        return acc;
    }, {} as Record<string, ShoppingListItem[]>);

    return (
        <>
            <h2 className="text-lg font-semibold text-gray-800 mb-4">{shoppingList.heading}</h2>
            <div className="space-y-6">
                {Object.keys(groupedItems).map((category) => (
                    <div key={category}>
                        <h3 className="text-md font-bold text-purple-700 mb-2">{category}</h3>
                        <ul className="space-y-2">
                            {groupedItems[category].map((item) => (
                                <li key={item.id} className="flex justify-between items-center bg-purple-50 p-2 rounded-lg shadow-sm">
                                    <div>
                                        <span className="font-medium text-gray-700">{item.name}</span>
                                        <span className="text-sm text-gray-500 ml-2">
                                            ({item.quantity} {item.unit})
                                        </span>
                                    </div>
                                    <span className={`text-sm font-medium ${item.status === "Finished" ? "text-green-600" : "text-red-600"}`}>
                                        {item.status}
                                    </span>
                                </li>
                            ))}
                        </ul>
                    </div>
                ))}
            </div>
        </>
    );
}
