// ignore_for_file: prefer_const_constructors

import "package:flutter/material.dart";

import "injector.dart";
import "general/pages/home_page.dart";

void main() {
  setup();
  runApp(App());
}

class App extends StatelessWidget {
  const App({super.key});

  @override
  Widget build(BuildContext context) {
    return MaterialApp(
      debugShowCheckedModeBanner: false,
      title: "TruQuest",
      theme: ThemeData(
        primarySwatch: Colors.blue,
      ),
      home: HomePage(),
    );
  }
}