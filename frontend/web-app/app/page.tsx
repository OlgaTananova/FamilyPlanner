'use client'
import Image from "next/image";
import { useAuth } from "./hooks/useAuth";
import { useEffect, useState } from "react";
import ProfilePage from "./profile/page";

export default function Home() {
  
  return (
    <div>
      <ProfilePage />
    </div>
  );
}
