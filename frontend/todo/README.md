# Todo MVP

A minimal, production-ready todo application built with Next.js, NextAuth.js, Prisma, and SQLite.

## Features

- User authentication (register, login, logout)
- Create, read, update, and delete todos
- User-specific todo isolation
- Form validation with Zod
- Clean Tailwind CSS design

## Setup

1. Install dependencies:
```bash
npm install
```

2. Set up environment variables:
Create a `.env.local` file with:
```
NEXTAUTH_URL=http://localhost:3000
NEXTAUTH_SECRET=your-secret-key-here
DATABASE_URL="file:./dev.db"
```

3. Set up the database:
```bash
npx prisma generate
npx prisma db push
```

4. Run the development server:
```bash
npm run dev
```

## Project Structure

```
├── app/
│   ├── (auth)/
│   │   ├── login/
│   │   └── register/
│   ├── api/
│   │   ├── auth/
│   │   └── todos/
│   ├── dashboard/
│   └── globals.css
├── components/
│   ├── AddTodo.tsx
│   ├── LoginForm.tsx
│   ├── LogoutButton.tsx
│   ├── RegisterForm.tsx
│   └── TodoList.tsx
├── lib/
│   ├── auth.ts
│   ├── prisma.ts
│   └── validations.ts
├── prisma/
│   └── schema.prisma
└── middleware.ts
```

## Tech Stack

- **Frontend**: Next.js 15, TypeScript, Tailwind CSS
- **Backend**: Next.js API routes
- **Database**: SQLite with Prisma ORM
- **Authentication**: NextAuth.js
- **Validation**: Zod