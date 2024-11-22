import { useSelector } from "react-redux";
import { getFamilyUsers } from "../lib/getFamilyUsers";
import { RootState } from "../redux/store";

interface FamilyUser {
    id: string,
    givenName: string
    displayName: string;
    role: string;
    isAdmin: boolean;
    email: string;
}

interface FamilyProps {
    familyName: string
}

export default async function Family({ familyName }: FamilyProps) {
    let familyUsers: FamilyUser[] = [];

    try {
        const response = await getFamilyUsers(familyName || "");
    } catch (error) {
        console.error("Error fetching family users:", error);
    }

    return (
        <div>
            <h1>Family Members</h1>
            <ul>
                {familyUsers.map((user) => (
                    <li key={user.id}>
                        {user.displayName} ({user.role})
                    </li>
                ))}
            </ul>
        </div>
    );
}