'use client'
import Image from "next/image";
import { useAuth } from "./hooks/useAuth";
import { useEffect, useState } from "react";

export default function Home() {
  const auth = useAuth();
  const token = auth.isAuthenticated
  const [account, setAccount] = useState(auth.account?.idTokenClaims);
  
  useEffect(()=>{
    setAccount(account)
  }, [account])
  
  return (
    <div>
      <h3 className="text-3xl font-semibold">
        {account?.idTokenClaims?.given_name}
        {account?.idTokenClaims?.extension_Family}
      
      </h3>
    </div>
  );
}
