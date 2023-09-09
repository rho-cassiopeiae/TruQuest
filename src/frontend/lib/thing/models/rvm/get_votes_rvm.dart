import '../../../general/models/rvm/vote_vm.dart';

class GetVotesRvm {
  final String thingId;
  final String? voteAggIpfsCid;
  final List<Vote2Vm> votes;

  GetVotesRvm.fromMap(Map<String, dynamic> map)
      : thingId = map['thingId'],
        voteAggIpfsCid = map['voteAggIpfsCid'],
        votes = List.unmodifiable(
          (map['votes'] as List<dynamic>).map((submap) => Vote2Vm.fromMap(submap)),
        );
}