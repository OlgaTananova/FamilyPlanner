import { Button } from 'flowbite-react'
import React, { Dispatch, SetStateAction } from 'react'

interface MobileViewTogglesProps {
    onSetActiveSection: Dispatch<SetStateAction<"lists" | "current" | "frequent">>
    activeSection: "lists" | "current" | "frequent"
}

export default function MobileViewToggles({ onSetActiveSection, activeSection }: MobileViewTogglesProps) {
    return (
        <div className="flex justify-around mb-4 md:hidden">
            <Button
                color="light"
                onClick={() => onSetActiveSection("lists")}
                className={`${activeSection === "lists" ? "bg-purple-100" : ""}  focus:ring-purple-500 focus:border-purple-500`}
            >
                Lists
            </Button>
            <Button
                color="light"
                onClick={() => onSetActiveSection("current")}
                className={`${activeSection === "current" ? "bg-purple-100" : ""} focus:ring-purple-500 focus:border-purple-500`}
            >
                Current
            </Button>
            <Button
                color="light"
                onClick={() => onSetActiveSection("frequent")}
                className={`${activeSection === "frequent" ? "bg-purple-100" : ""} focus:ring-purple-500 focus:border-purple-500`}
            >
                Frequent
            </Button>
        </div>
    )
}
