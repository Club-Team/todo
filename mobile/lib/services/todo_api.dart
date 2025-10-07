import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/todo_item.dart';

class TodoApi {
  static const String baseUrl = 'http://localhost:5239/api/Todo'; // change port if needed

  static Future<List<TodoItem>> getTodos() async {
    try {
      final response = await http.get(Uri.parse(baseUrl));

      if (response.statusCode == 200) {
        final List<dynamic> jsonList = json.decode(response.body);
        return jsonList.map((e) => TodoItem.fromJson(e)).toList();
      } else {
        print('Failed to load todos from API: ${response.statusCode}');
      }
    } catch (e) {
      print('Error fetching todos: $e');
    }

    // Return placeholder data if request fails
    return [
      TodoItem(
        id: '1',
        title: 'Sample Todo 1',
        description: 'This is placeholder todo',
        isCompleted: false,
        createdAt: DateTime.now(),
      ),
      TodoItem(
        id: '2',
        title: 'Sample Todo 2',
        description: 'Another placeholder',
        isCompleted: true,
        createdAt: DateTime.now(),
      ),
    ];
  }

  static Future<TodoItem> createTodo(TodoItem item) async {
    try {
      final response = await http.post(
        Uri.parse(baseUrl),
        headers: {'Content-Type': 'application/json'},
        body: json.encode(item.toJson()),
      );

      if (response.statusCode == 200) {
        return TodoItem.fromJson(json.decode(response.body));
      }
    } catch (e) {
      print('Error creating todo: $e');
    }

    // Return the item itself as a fallback
    return item;
  }

  static Future<void> deleteTodo(String id) async {
    try {
      final response = await http.delete(Uri.parse('$baseUrl/$id'));
      if (response.statusCode != 200) {
        print('Failed to delete todo: ${response.statusCode}');
      }
    } catch (e) {
      print('Error deleting todo: $e');
    }
  }

  static Future<void> updateTodo(String id, TodoItem item) async {
    try {
      final response = await http.put(
        Uri.parse('$baseUrl/$id'),
        headers: {'Content-Type': 'application/json'},
        body: json.encode(item.toJson()),
      );
      if (response.statusCode != 200) {
        print('Failed to update todo: ${response.statusCode}');
      }
    } catch (e) {
      print('Error updating todo: $e');
    }
  }
}
