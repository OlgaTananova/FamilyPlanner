import React from "react";
import { Button } from "flowbite-react";
import { FaListUl, FaPlus, FaUpload, FaCalendarAlt, FaDollarSign } from "react-icons/fa";
import { useSelector } from "react-redux";
import { RootState } from "../redux/store";

export default function Dashboard() {
    const user = useSelector((state: RootState) => state.user);

    return (
        <div className="container mx-auto px-4 py-6">
            {/* Greeting Section */}
            <div className="mb-6">
                <h1 className="text-2xl sm:text-3xl font-bold text-purple-700">
                    Welcome Back, {user?.givenName || "User"}!
                </h1>
                <p className="text-gray-600 mt-2 text-sm sm:text-base">
                    What would you like to do today? Here are some quick actions to get started:
                </p>
            </div>

            {/* Quick Actions Bar */}
            <div className="grid grid-cols-2 sm:grid-cols-3 lg:grid-cols-5 gap-4">
                {/* Create a New Shopping List */}
                <Button
                    color="purple"
                    className="flex flex-col items-center justify-center p-4 space-y-2 bg-purple-50 hover:bg-purple-200 border border-purple-300 rounded-lg shadow-md transition duration-300 ease-in-out transform hover:-translate-y-1"
                    onClick={() => alert("Quick Action!")}
                >
                    <FaListUl size={24} className="text-purple-600 mr-2" />
                    <span className="text-sm font-medium text-purple-700">New Shopping List</span>
                </Button>

                {/* Add a New Item */}
                <Button
                    color="purple"
                    className="flex flex-col items-center justify-center p-4 space-y-2 bg-purple-50 hover:border-fuchsia-950 border-purple-300 rounded-lg shadow-md"
                    onClick={() => alert("Add a new item to the catalog!")}
                >
                    <FaPlus size={24} className="text-purple-600 mr-2" />
                    <span className="text-sm font-medium text-purple-700">Add New Item</span>
                </Button>

                {/* Upload Grocery Bill */}
                <Button
                    color="purple"
                    className="flex flex-col items-center justify-center p-4 space-y-2 bg-purple-50 hover:bg-purple-100 border border-purple-300 rounded-lg shadow-md"
                    onClick={() => alert("Upload a grocery bill!")}
                >
                    <FaUpload size={24} className="text-purple-600 mr-2" />
                    <span className="text-sm font-medium text-purple-700">Upload Bill</span>
                </Button>

                {/* Create a Weekly Menu */}
                <Button
                    color="purple"
                    className="flex flex-col items-center justify-center p-4 space-y-2 bg-purple-50 hover:bg-purple-100 border border-purple-300 rounded-lg shadow-md"
                    onClick={() => alert("Create a weekly menu!")}
                >
                    <FaCalendarAlt size={24} className="text-purple-600 mr-2" />
                    <span className="text-sm font-medium text-purple-700">Weekly Menu</span>
                </Button>

                {/* Add Expenses to Budget */}
                <Button
                    color="purple"
                    className="flex flex-col items-center justify-center p-4 space-y-2 bg-purple-50 hover:bg-purple-100 border border-purple-300 rounded-lg shadow-md"
                    onClick={() => alert("Add expenses to your budget!")}
                >
                    <FaDollarSign size={24} className="text-purple-600 mr-2" />
                    <span className="text-sm font-medium text-purple-700">Add Expenses</span>
                </Button>
            </div>
        </div>
    );
};

