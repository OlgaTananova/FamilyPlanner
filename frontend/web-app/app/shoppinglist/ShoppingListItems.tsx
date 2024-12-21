import React from 'react'
import { ShoppingList, ShoppingListItem } from '../redux/shoppingListSlice';
import ShoppingListItemComponent from './ShoppingListItem';

interface ShoppingListItemsProps {
    shoppingList: ShoppingList;
    groupedItems: Record<string, ShoppingListItem[]>
    isHiddenCategories: boolean;
}
export default function ShoppingListItems({ isHiddenCategories, groupedItems, shoppingList }: ShoppingListItemsProps) {
    

    return (
        <div className="space-y-6">
            {!isHiddenCategories ? Object.entries(groupedItems).map(([category, items]) => (
                <div key={category}>
                    <h3 className="text-md font-bold text-purple-700 mb-2">{category}</h3>
                    <ul className="space-y-2">
                        {items.map((item) => (
                            <ShoppingListItemComponent key={item.id} item={item} />
                        ))}
                    </ul>
                </div>
            ))
                : shoppingList.items.map((item) => {
                    return (<ShoppingListItemComponent key={item.id} item={item} />);
                })}
        </div>
    )
}
