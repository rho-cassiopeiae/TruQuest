import 'dart:async';

import '../models/im/decision_im.dart';
import '../../general/contexts/multi_stage_operation_context.dart';
import '../../general/services/toast_messenger.dart';
import '../models/rvm/get_settlement_proposals_list_rvm.dart';
import '../models/rvm/get_verifiers_rvm.dart';
import '../models/rvm/thing_state_vm.dart';
import '../models/rvm/get_thing_rvm.dart';
import 'thing_result_vm.dart';
import '../../general/bloc/bloc.dart';
import '../services/thing_service.dart';
import 'thing_actions.dart';

class ThingBloc extends Bloc<ThingAction> {
  final ToastMessenger _toastMessenger;
  final ThingService _thingService;

  final _thingChannel = StreamController<GetThingResultVm>.broadcast();
  Stream<GetThingResultVm> get thing$ => _thingChannel.stream;

  final _verifierLotteryInfoChannel =
      StreamController<GetVerifierLotteryInfoSuccessVm>.broadcast();
  Stream<GetVerifierLotteryInfoSuccessVm> get verifierLotteryInfo$ =>
      _verifierLotteryInfoChannel.stream;

  final _verifierLotteryParticipantsChannel =
      StreamController<GetVerifierLotteryParticipantsSuccessVm>.broadcast();
  Stream<GetVerifierLotteryParticipantsSuccessVm>
      get verifierLotteryParticipants$ =>
          _verifierLotteryParticipantsChannel.stream;

  final _verifiersChannel = StreamController<GetVerifiersRvm>.broadcast();
  Stream<GetVerifiersRvm> get verifiers$ => _verifiersChannel.stream;

  final _proposalsListChannel =
      StreamController<GetSettlementProposalsListRvm>.broadcast();
  Stream<GetSettlementProposalsListRvm> get proposalsList$ =>
      _proposalsListChannel.stream;

  ThingBloc(this._toastMessenger, this._thingService) {
    actionChannel.stream.listen((action) {
      List<String>? validationErrors;
      if (action.mustValidate) {
        validationErrors = action.validate();
        if (validationErrors != null) {
          _toastMessenger.add('• ' + validationErrors.join('\n• '));
        }
      }

      if (action is CreateNewThingDraft) {
        _createNewThingDraft(action, validationErrors);
      } else if (action is GetThing) {
        _getThing(action);
      } else if (action is SubmitNewThing) {
        _submitNewThing(action);
      } else if (action is GetVerifierLotteryInfo) {
        _getVerifierLotteryInfo(action);
      } else if (action is GetVerifierLotteryParticipants) {
        _getVerifierLotteryParticipants(action);
      } else if (action is GetAcceptancePollInfo) {
        _getAcceptancePollInfo(action);
      } else if (action is GetVerifiers) {
        _getVerifiers(action);
      } else if (action is GetSettlementProposalsList) {
        _getSettlementProposalsList(action);
      } else if (action is Watch) {
        _watch(action);
      }
    });
  }

  void _createNewThingDraft(
    CreateNewThingDraft action,
    List<String>? validationErrors,
  ) async {
    if (validationErrors != null) {
      action.complete(CreateNewThingDraftFailureVm());
      return;
    }

    await _thingService.createNewThingDraft(action.documentContext);
    action.complete(null);
  }

  void _getThing(GetThing action) async {
    var result = await _thingService.getThing(action.thingId);
    if (result.isLeft) {
      _thingChannel.add(GetThingFailureVm(message: result.left.toString()));
      return;
    }

    var getThingResult = result.right;
    if (getThingResult.thing.state == ThingStateVm.awaitingFunding) {
      bool thingFunded = await _thingService.checkThingAlreadyFunded(
        action.thingId,
      );
      getThingResult = GetThingRvm(
        thing: getThingResult.thing.copyWith(
          fundedAwaitingConfirmation: thingFunded,
        ),
        signature: getThingResult.signature,
      );
    }

    _thingChannel.add(GetThingSuccessVm(result: getThingResult));
  }

  void _submitNewThing(SubmitNewThing action) async {
    await _thingService.submitNewThing(action.thing.id);
    action.complete(null);
  }

  Stream<Object> fundThing(
    String thingId,
    String signature,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _thingService.fundThing(thingId, signature, ctx);
  }

  void _refreshVerifierLotteryInfo(String thingId) async {
    var info = await _thingService.getVerifierLotteryInfo(thingId);
    _verifierLotteryInfoChannel.add(
      GetVerifierLotteryInfoSuccessVm(
        userId: info.$1,
        initBlock: info.$2,
        durationBlocks: info.$3,
        alreadyJoined: info.$4,
      ),
    );
  }

  void _getVerifierLotteryInfo(GetVerifierLotteryInfo action) {
    _refreshVerifierLotteryInfo(action.thingId);
  }

  Stream<Object> joinLottery(
    String thingId,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _thingService.joinLottery(thingId, ctx);
  }

  void _getVerifierLotteryParticipants(
    GetVerifierLotteryParticipants action,
  ) async {
    var result = await _thingService.getVerifierLotteryParticipants(
      action.thingId,
    );
    _verifierLotteryParticipantsChannel.add(
      GetVerifierLotteryParticipantsSuccessVm(entries: result.entries),
    );
  }

  void _getAcceptancePollInfo(GetAcceptancePollInfo action) async {
    var info = await _thingService.getAcceptancePollInfo(action.thingId);
    action.complete(
      GetAcceptancePollInfoSuccessVm(
        userId: info.$1,
        initBlock: info.$2,
        durationBlocks: info.$3,
        userIndexInThingVerifiersArray: info.$4,
      ),
    );
  }

  Stream<Object> castVoteOffChain(
    String thingId,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _thingService.castVoteOffChain(
      thingId,
      decision,
      reason,
      ctx,
    );
  }

  Stream<Object> castVoteOnChain(
    String thingId,
    int userIndexInThingVerifiersArray,
    DecisionIm decision,
    String reason,
    MultiStageOperationContext ctx,
  ) async* {
    yield* _thingService.castVoteOnChain(
      thingId,
      userIndexInThingVerifiersArray,
      decision,
      reason,
      ctx,
    );
  }

  void _getVerifiers(GetVerifiers action) async {
    var result = await _thingService.getVerifiers(action.thingId);
    _verifiersChannel.add(result);
  }

  void _getSettlementProposalsList(GetSettlementProposalsList action) async {
    var result = await _thingService.getSettlementProposalsList(action.thingId);
    _proposalsListChannel.add(result);
  }

  void _watch(Watch action) async {
    await _thingService.watch(action.thingId, action.markedAsWatched);
  }
}
