import { configureStore } from "@reduxjs/toolkit";
import catalogReducer from "./catalogSlice";
import shoppingListReducer from './shoppingListSlice';
import userReducer from "./userSlice";

export const store = configureStore({
  reducer: {
    user: userReducer,
    categories: catalogReducer,
    shoppinglists: shoppingListReducer
  }, 
  devTools: process.env.NODE_ENV !== "production",
});

export type RootState = ReturnType<typeof store.getState>;
export type AppDispatch = typeof store.dispatch;


