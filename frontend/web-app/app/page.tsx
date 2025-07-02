'use client'
import { useRouter } from "next/navigation";
import DashboardPage from "./dashboard/page";
import { useAuth } from "./hooks/useAuth";


export default function Home() {

  const { isAuthenticated } = useAuth();


  if (isAuthenticated) {
    return (
      <>
        <DashboardPage />
      </>
    );
  } 

  return (
    <div className="flex justify-center items-center h-screen bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50">
      <div className="p-8 bg-white rounded-lg shadow-lg max-w-md w-full text-center">
        <h1 className="text-3xl font-bold text-purple-700 mb-6">
          Welcome to Family Planner
        </h1>
        <p className="text-gray-700 mb-6">
          Organize your family life with ease. Sign in to get started!
        </p>
      </div>
    </div>
  );
}
