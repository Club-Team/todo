"use client";

import { useState } from "react";

type Todo = {
  id: number;
  text: string;
  completed: boolean;
};

export default function TodoPage() {
  const [todos, setTodos] = useState<Todo[]>([]);
  const [newTodo, setNewTodo] = useState("");

  const addTodo = () => {
    if (!newTodo.trim()) return;
    const todo: Todo = {
      id: Date.now(),
      text: newTodo.trim(),
      completed: false,
    };
    setTodos((prev) => [...prev, todo]);
    setNewTodo("");
  };

  const toggleTodo = (id: number) => {
    setTodos((prev) =>
      prev.map((t) =>
        t.id === id ? { ...t, completed: !t.completed } : t
      )
    );
  };

  const deleteTodo = (id: number) => {
    setTodos((prev) => prev.filter((t) => t.id !== id));
  };

  return (
    <main className="flex flex-col items-center p-6 max-w-md mx-auto">
      <h1 className="text-2xl font-bold mb-4">Todo List</h1>

      <div className="flex w-full mb-4">
        <input
          type="text"
          value={newTodo}
          onChange={(e) => setNewTodo(e.target.value)}
          placeholder="Add a new todo..."
          className="flex-grow border border-gray-300 rounded-l-md p-2 outline-none"
        />
        <button
          onClick={addTodo}
          className="bg-blue-600 text-white px-4 rounded-r-md hover:bg-blue-700"
        >
          Add
        </button>
      </div>

      <ul className="w-full">
        {todos.map((todo) => (
          <li
            key={todo.id}
            className="flex justify-between items-center bg-gray-100 p-2 mb-2 rounded-md"
          >
            <span
              onClick={() => toggleTodo(todo.id)}
              className={`flex-grow cursor-pointer text-black ${
                todo.completed ? "line-through text-gray-500" : ""
              }`}
            >
              {todo.text}
            </span>
            <button
              onClick={() => deleteTodo(todo.id)}
              className="text-red-500 hover:text-red-700"
            >
              ✕
            </button>
          </li>
        ))}
      </ul>
    </main>
  );
}
