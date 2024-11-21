import { createSlice, PayloadAction } from "@reduxjs/toolkit";

export interface UserState {
  givenName: string | null;
  family: string | null;
  role: string | null;
  email: string | null;
}

const initialState: UserState = {
  givenName: null,
  family: null,
  role: null,
  email: null,
};

const userSlice = createSlice({
  name: "user",
  initialState,
  reducers: {
    setUser: (state, action: PayloadAction<UserState>) => {
      state.givenName = action.payload.givenName;
      state.family = action.payload.family;
      state.role = action.payload.role;
      state.email = action.payload.email;
    },
    clearUser: (state) => {
      state.givenName = null;
      state.family = null;
      state.role = null;
      state.email = null;
    },
  },
});

export const { setUser, clearUser } = userSlice.actions;
export default userSlice.reducer;
