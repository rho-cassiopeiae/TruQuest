// GENERATED CODE - DO NOT MODIFY BY HAND

part of "injector.dart";

// **************************************************************************
// KiwiInjectorGenerator
// **************************************************************************

class _$Injector extends Injector {
  @override
  void configure() {
    final KiwiContainer container = KiwiContainer();
    container
      ..registerSingleton((c) => UserBloc(c<UserService>()))
      ..registerSingleton((c) => UserService(c<EthereumService>()))
      ..registerSingleton((c) => EthereumBloc(c<EthereumService>()))
      ..registerSingleton((c) => EthereumService());
  }
}
