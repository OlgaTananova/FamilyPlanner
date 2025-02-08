import { NextRequest, NextResponse } from "next/server";

const TELEGRAM_BOT_TOKEN = process.env.TELEGRAM_BOT_TOKEN;
const TELEGRAM_CHAT_ID = process.env.TELEGRAM_CHAT_ID;

export async function POST(req: NextRequest) {
    try {
        const { message } = await req.json();


        if (!TELEGRAM_BOT_TOKEN) {
            return NextResponse.json({ error: "Missing Telegram configuration" }, { status: 500 });
        }

        const telegramApiUrl = `https://api.telegram.org/bot${TELEGRAM_BOT_TOKEN}/sendMessage`;


        const response = await fetch(telegramApiUrl, {
            method: "POST",
            headers: { "Content-Type": "application/json" },
            body: JSON.stringify({
                chat_id: TELEGRAM_CHAT_ID,
                text: message,
                parse_mode: "MarkdownV2", // Enables advanced Markdown formatting
            }),
        });

        const data = await response.json();
        if (!response.ok) {
            throw new Error(data.description || "Failed to send message");
        }

        return NextResponse.json({ success: true, data });
    } catch (error: any) {
        return NextResponse.json({ error: error.message }, { status: 500 });
    }
}
