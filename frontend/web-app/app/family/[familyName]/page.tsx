import { getFamilyUsers } from "@/app/lib/getFamilyUsers";
import { RootState } from "@/app/redux/store";
import Link from "next/link";

export interface FamilyUser {
  id: string;
  family: string;
  givenName: string;
  displayName: string;
  role: string;
  isAdmin: boolean;
  email: string;
}


export default async function FamilyPage({ params }: { params: Promise<{ familyName: string }> }) {
  const { familyName } = await params;
  let familyUsers: FamilyUser[] = [];

  if (!familyName) {
    return (
      <div className="flex flex-col items-center justify-center h-full bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50 p-4">
        <h1 className="text-2xl font-bold text-purple-600 mb-4">Family Members</h1>
        <p className="text-gray-600">
          No family name provided. Please log in or set your family context.
        </p>
      </div>
    );
  }


  try {
    const users = await getFamilyUsers(familyName as string);
    familyUsers = users;
  } catch (error) {
    console.error("Error fetching family users:", error);
  }


  return (
    <div className="flex flex-col items-center p-6 bg-gradient-to-r from-purple-50 via-purple-100 to-fuchsia-50 min-h-screen relative">
      <Link
        href={"/"}
        className="absolute top-2 right-2 text-gray-600 hover:text-gray-900"
        aria-label="Close Profile"
      >
        ✖
      </Link>

      <h1 className="text-3xl md:text-2xl sm:text-xl font-bold text-purple-700 mb-6">
        Members of <span className="text-fuchsia-600">{familyName}</span> family
      </h1>

      <div className="grid grid-cols-1 sm:grid-cols-2 md:grid-cols-3 lg:grid-cols-4 gap-6 w-full max-w-6xl">
        {familyUsers.map((user: FamilyUser) => (
          <div
            key={user.id}
            className="p-4 bg-white shadow-md rounded-lg flex flex-col items-center text-center transition-transform transform hover:scale-105"
          >
            <div className="w-16 h-16 rounded-full bg-purple-200 flex items-center justify-center text-purple-600 font-bold text-xl mb-4">
              {user.givenName[0]}
            </div>
            <h2 className="text-lg font-semibold text-gray-800">{user.givenName}</h2>
            <p className="text-sm text-gray-600">{user.role}</p>
            {user.isAdmin && (
              <span className="mt-2 px-2 py-1 bg-purple-500 text-white text-xs rounded-full">
                Admin
              </span>
            )}
            <p className="text-sm text-gray-500 mt-2 w-full truncate">{user.email}</p>
          </div>
        ))}
      </div>
    </div>
  );
}