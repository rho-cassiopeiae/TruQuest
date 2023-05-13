import 'dart:async';

import 'package:rxdart/rxdart.dart';

import '../services/notifications_cache.dart';
import 'notification_actions.dart';
import '../../settlement/services/settlement_service.dart';
import '../../thing/services/thing_service.dart';
import 'bloc.dart';

class NotificationBloc extends Bloc<NotificationAction> {
  final NotificationsCache _notificationsCache;
  final ThingService _thingService;
  final SettlementService _settlementService;

  final BehaviorSubject<Stream<int>?> _progress$Channel =
      BehaviorSubject<Stream<int>?>();
  Stream<Stream<int>?> get progress$$ => _progress$Channel.stream;

  NotificationBloc(
    this._notificationsCache,
    this._thingService,
    this._settlementService,
  ) {
    actionChannel.stream.listen((action) {
      if (action is Dismiss) {
        _dismiss(action);
      }
    });

    _thingService.progress$$.listen((progress$) {
      _progress$Channel.add(progress$);
      progress$.listen(null, onDone: () {
        Future.delayed(Duration(seconds: 2)).then(
          (_) {
            if (_progress$Channel.value == progress$) {
              _progress$Channel.add(null);
            }
          },
        );
      });
    });

    _settlementService.progress$$.listen((progress$) {
      _progress$Channel.add(progress$);
      progress$.listen(null, onDone: () {
        Future.delayed(Duration(seconds: 2)).then(
          (_) {
            if (_progress$Channel.value == progress$) {
              _progress$Channel.add(null);
            }
          },
        );
      });
    });
  }

  void _dismiss(Dismiss action) async {
    await _notificationsCache.remove(action.notifications);
  }
}