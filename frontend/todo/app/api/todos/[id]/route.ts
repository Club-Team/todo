import { NextRequest, NextResponse } from 'next/server'
import { getServerSession } from 'next-auth'
import { authOptions } from '@/lib/auth'
import { prisma } from '@/lib/prisma'

export async function PATCH(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  try {
    const session = await getServerSession(authOptions)
    
    if (!session?.user?.id) {
      return NextResponse.json({ message: 'Unauthorized' }, { status: 401 })
    }

    const body = await request.json()
    const { completed } = body

    // Verify todo belongs to user
    const todo = await prisma.todo.findFirst({
      where: {
        id: params.id,
        userId: session.user.id
      }
    })

    if (!todo) {
      return NextResponse.json({ message: 'Todo not found' }, { status: 404 })
    }

    const updatedTodo = await prisma.todo.update({
      where: {
        id: params.id
      },
      data: {
        completed: completed
      }
    })

    return NextResponse.json(updatedTodo)
  } catch (error) {
    console.error('Failed to update todo:', error)
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    )
  }
}

export async function DELETE(
  request: NextRequest,
  { params }: { params: { id: string } }
) {
  try {
    const session = await getServerSession(authOptions)
    
    if (!session?.user?.id) {
      return NextResponse.json({ message: 'Unauthorized' }, { status: 401 })
    }

    // Verify todo belongs to user
    const todo = await prisma.todo.findFirst({
      where: {
        id: params.id,
        userId: session.user.id
      }
    })

    if (!todo) {
      return NextResponse.json({ message: 'Todo not found' }, { status: 404 })
    }

    await prisma.todo.delete({
      where: {
        id: params.id
      }
    })

    return NextResponse.json({ message: 'Todo deleted successfully' })
  } catch (error) {
    console.error('Failed to delete todo:', error)
    return NextResponse.json(
      { message: 'Internal server error' },
      { status: 500 }
    )
  }
}
