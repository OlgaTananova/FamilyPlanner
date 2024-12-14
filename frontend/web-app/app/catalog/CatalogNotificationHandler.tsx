import { useEffect } from "react";
import { useSignalR } from "../providers/SignalRProvider";
import { useDispatch } from "react-redux";
import { updateCatalogItem } from "../redux/shoppingListSlice";

export default function CatalogNotificationHandler() {
    const { connection, isConnected } = useSignalR();
    const dispatch = useDispatch();

    useEffect(() => {
        if (connection && isConnected) {
            connection.on("CatalogItemUpdated", (updatedItem) => {
                console.log(updatedItem);
                dispatch(updateCatalogItem(updatedItem))
            });

            return () => {
                connection.off("CatalogItemUpdated");
            };
        }
    }, [connection, isConnected]);

    return null; // This component doesn't render anything
}
