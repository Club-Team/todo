import 'package:flutter/material.dart';
import '../models/todo_item.dart';
import '../services/todo_api.dart';

class TodoListPage extends StatefulWidget {
  const TodoListPage({super.key});

  @override
  State<TodoListPage> createState() => _TodoListPageState();
}

class _TodoListPageState extends State<TodoListPage> {
  late Future<List<TodoItem>> _todos;

  @override
  void initState() {
    super.initState();
    _todos = TodoApi.getTodos();
  }

  void _refresh() {
    setState(() {
      _todos = TodoApi.getTodos();
    });
  }

  @override
  Widget build(BuildContext context) {
    return Scaffold(
      appBar: AppBar(title: const Text('Todo List')),
      body: FutureBuilder<List<TodoItem>>(
        future: _todos,
        builder: (context, snapshot) {
          if (snapshot.connectionState == ConnectionState.waiting) {
            return const Center(child: CircularProgressIndicator());
          } else if (snapshot.hasError) {
            return Center(child: Text('Error: ${snapshot.error}'));
          } else if (snapshot.hasData) {
            final todos = snapshot.data!;
            if (todos.isEmpty) {
              return const Center(child: Text('No todos yet'));
            }
            return RefreshIndicator(
              onRefresh: () async => _refresh(),
              child: ListView.builder(
                itemCount: todos.length,
                itemBuilder: (context, index) {
                  final todo = todos[index];
                  return ListTile(
                    title: Text(todo.title),
                    subtitle: Text(todo.description ?? ''),
                    trailing: Icon(
                      todo.isCompleted ? Icons.check_circle : Icons.circle_outlined,
                      color: todo.isCompleted ? Colors.green : Colors.grey,
                    ),
                  );
                },
              ),
            );
          } else {
            return const Center(child: Text('No data'));
          }
        },
      ),
    );
  }
}
