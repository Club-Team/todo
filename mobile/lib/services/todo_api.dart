import 'dart:convert';
import 'package:http/http.dart' as http;
import '../models/todo_item.dart';

class TodoApi {
  static const String baseUrl = 'http://localhost:5239/api/Todo'; // change port if needed

  static Future<List<TodoItem>> getTodos() async {
    final response = await http.get(Uri.parse(baseUrl));

    if (response.statusCode == 200) {
      final List<dynamic> jsonList = json.decode(response.body);
      return jsonList.map((e) => TodoItem.fromJson(e)).toList();
    } else {
      throw Exception('Failed to load todos: ${response.statusCode}');
    }
  }

  static Future<TodoItem> createTodo(TodoItem item) async {
    final response = await http.post(
      Uri.parse(baseUrl),
      headers: {'Content-Type': 'application/json'},
      body: json.encode(item.toJson()),
    );

    if (response.statusCode == 200) {
      return TodoItem.fromJson(json.decode(response.body));
    } else {
      throw Exception('Failed to create todo');
    }
  }

  static Future<void> deleteTodo(String id) async {
    final response = await http.delete(Uri.parse('$baseUrl/$id'));
    if (response.statusCode != 200) {
      throw Exception('Failed to delete todo');
    }
  }

  static Future<void> updateTodo(String id, TodoItem item) async {
    final response = await http.put(
      Uri.parse('$baseUrl/$id'),
      headers: {'Content-Type': 'application/json'},
      body: json.encode(item.toJson()),
    );
    if (response.statusCode != 200) {
      throw Exception('Failed to update todo');
    }
  }
}
