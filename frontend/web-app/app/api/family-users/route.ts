// app/api/family-users/route.ts
import { NextResponse } from "next/server";
import { getFamilyUsers } from "../../lib/getFamilyUsers";

export async function POST(req: Request) {
    const { familyName } = await req.json();
    console.log(familyName);
    try {
        const familyUsers = await getFamilyUsers(familyName || "");
        return NextResponse.json(familyUsers);
    } catch (error) {
        console.error("Error fetching family users:", error);
        return NextResponse.json({ error: "Failed to fetch family users" }, { status: 500 });
    }
}
