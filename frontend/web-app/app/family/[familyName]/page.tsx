import { getFamilyUsers } from "@/app/lib/getFamilyUsers";

interface FamilyUser {
  id: string;
  givenName: string;
  displayName: string;
  role: string;
  isAdmin: boolean;
  email: string;
}

interface Props {
  params: { familyName: string };
}


export default async function FamilyPage({ params }: Props) {
  const { familyName } = params;

  if (!familyName) {
    return (
      <div>
        <h1>Family Members</h1>
        <p>No family name provided. Please log in or set your family context.</p>
      </div>
    );
  }
  let familyUsers;
  try {
    familyUsers = await getFamilyUsers(familyName);
  } catch (error) {
    console.error("Error fetching family users:", error);
  }

  return (
    <div>
      <h1>Family Members for {familyName}</h1>
      <ul>
        {familyUsers.map((user: FamilyUser) => (
          <li key={user.id}>
            {user.displayName} ({user.role})
          </li>
        ))}
      </ul>
    </div>
  );
}