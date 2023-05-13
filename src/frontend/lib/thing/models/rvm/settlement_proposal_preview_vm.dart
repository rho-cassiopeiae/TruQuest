import 'package:flutter/material.dart';
import 'package:intl/intl.dart';

import '../../../settlement/models/rvm/settlement_proposal_state_vm.dart';
import '../../../settlement/models/rvm/verdict_vm.dart';

class SettlementProposalPreviewVm {
  final String id;
  final SettlementProposalStateVm state;
  final String title;
  final VerdictVm verdict;
  final String? croppedImageIpfsCid;
  final String submitterId;
  final DateTime? displayedTimestamp;

  String get displayedTimestampFormatted => displayedTimestamp != null
      ? DateFormat('EEE, M/d/y').format(displayedTimestamp!)
      : 'Not submitted yet';

  Color get verdictColor {
    switch (verdict) {
      case VerdictVm.delivered:
        return Colors.green;
      case VerdictVm.guessItCounts:
        return Color.fromARGB(255, 136, 193, 93);
      case VerdictVm.aintGoodEnough:
        return Color.fromARGB(255, 238, 255, 83);
      case VerdictVm.motionNotAction:
        return Color.fromARGB(255, 255, 209, 84);
      case VerdictVm.noEffortWhatsoever:
        return Color.fromARGB(255, 255, 108, 108);
      case VerdictVm.asGoodAsMaliciousIntent:
        return Color.fromARGB(255, 145, 0, 0);
    }
  }

  IconData get stateIcon {
    switch (state) {
      case SettlementProposalStateVm.draft:
        return Icons.drive_file_rename_outline_outlined;
      case SettlementProposalStateVm.awaitingFunding:
        return Icons.attach_money;
      case SettlementProposalStateVm.fundedAndVerifierLotteryInitiated:
        return Icons.people;
      case SettlementProposalStateVm.verifiersSelectedAndPollInitiated:
        return Icons.poll_outlined;
      case SettlementProposalStateVm.softDeclined:
        return Icons.thumbs_up_down;
      case SettlementProposalStateVm.hardDeclined:
        return Icons.thumb_down;
      case SettlementProposalStateVm.accepted:
        return Icons.thumb_up;
    }
  }

  SettlementProposalPreviewVm.fromMap(Map<String, dynamic> map)
      : id = map['id'],
        state = SettlementProposalStateVm.values[map['state']],
        title = map['title'],
        verdict = VerdictVm.values[map['verdict']],
        croppedImageIpfsCid = map['croppedImageIpfsCid'],
        submitterId = map['submitterId'],
        displayedTimestamp = map['displayedTimestamp'] != null
            ? DateTime.fromMillisecondsSinceEpoch(map['displayedTimestamp'])
            : null;
}