import 'dart:math';

import 'package:flutter/material.dart';
import 'package:google_fonts/google_fonts.dart';
import 'package:sleek_circular_slider/sleek_circular_slider.dart';

import '../../general/widgets/votes_table.dart';
import '../models/rvm/acceptance_poll_info_vm.dart';
import '../../general/widgets/block_countdown.dart';
import '../../ethereum/bloc/ethereum_bloc.dart';
import '../../user/bloc/user_bloc.dart';
import '../bloc/thing_actions.dart';
import '../bloc/thing_bloc.dart';
import '../models/rvm/thing_vm.dart';
import '../../widget_extensions.dart';
import 'poll_stepper.dart';

class Poll extends StatefulWidget {
  final ThingVm thing;

  const Poll({super.key, required this.thing});

  @override
  State<Poll> createState() => _PollState();
}

class _PollState extends StateX<Poll> {
  late final _thingBloc = use<ThingBloc>();
  late final _ethereumBloc = use<EthereumBloc>();
  late final _userBloc = use<UserBloc>();

  String? _currentUserId;

  late Future<AcceptancePollInfoVm?> _acceptancePollInfoFuture;

  @override
  void initState() {
    super.initState();

    _currentUserId = _userBloc.latestCurrentUser?.id;

    _thingBloc.dispatch(GetVotes(thingId: widget.thing.id));
    _acceptancePollInfoFuture = _thingBloc.execute<AcceptancePollInfoVm>(
      GetAcceptancePollInfo(thingId: widget.thing.id),
    );
  }

  @override
  void didUpdateWidget(covariant Poll oldWidget) {
    super.didUpdateWidget(oldWidget);
    _currentUserId = _userBloc.latestCurrentUser?.id;
    _acceptancePollInfoFuture = _thingBloc.execute<AcceptancePollInfoVm>(
      GetAcceptancePollInfo(thingId: widget.thing.id),
    );
  }

  @override
  Widget buildX(BuildContext context) {
    return Row(
      children: [
        Expanded(
          child: StreamBuilder(
            stream: _thingBloc.votes$,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(child: CircularProgressIndicator());
              }

              return VotesTable(
                result: snapshot.data!,
                currentUserId: _currentUserId,
                onRefresh: () => _thingBloc.dispatch(GetVotes(thingId: widget.thing.id)),
              );
            },
          ),
        ),
        Expanded(
          // @@??: Should switch to StreamBuilder to avoid snapshot.data becoming null momentarily
          // when switching accounts?
          child: FutureBuilder(
            future: _acceptancePollInfoFuture,
            builder: (context, snapshot) {
              if (snapshot.data == null) {
                return const Center(child: CircularProgressIndicator());
              }

              var info = snapshot.data!;

              return StreamBuilder(
                stream: _ethereumBloc.latestL1BlockNumber$,
                initialData: _ethereumBloc.latestL1BlockNumber,
                builder: (context, snapshot) {
                  var latestBlockNumber = snapshot.data!.toDouble();
                  var startBlock = info.initBlock?.abs().toDouble() ?? 0;
                  var endBlock = startBlock + info.durationBlocks;
                  var currentBlock = 0.0;
                  if (info.initBlock != null) {
                    currentBlock = min(latestBlockNumber, endBlock).toDouble();
                  }

                  return Center(
                    child: Column(
                      children: [
                        Container(
                          margin: const EdgeInsets.only(top: 18),
                          decoration: const BoxDecoration(
                            border: Border(
                              bottom: BorderSide(color: Colors.white),
                            ),
                          ),
                          padding: const EdgeInsets.fromLTRB(8, 0, 8, 6),
                          child: Text(
                            'Poll',
                            style: GoogleFonts.philosopher(
                              color: Colors.white,
                              fontSize: 24,
                            ),
                          ),
                        ),
                        const SizedBox(height: 12),
                        Stack(
                          alignment: Alignment.center,
                          children: [
                            SleekCircularSlider(
                              min: startBlock,
                              max: endBlock,
                              initialValue: currentBlock,
                              appearance: CircularSliderAppearance(
                                size: 300,
                                customColors: CustomSliderColors(
                                  dotColor: Colors.transparent,
                                ),
                              ),
                              innerWidget: (_) => const SizedBox.shrink(),
                            ),
                            if (info.initBlock != null) BlockCountdown(blocksLeft: (endBlock - currentBlock).toInt()),
                            Positioned(
                              bottom: 20,
                              left: 0,
                              right: 0,
                              child: Row(
                                children: [
                                  Text(
                                    startBlock.toStringAsFixed(0),
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 26,
                                    ),
                                  ),
                                  const Spacer(),
                                  Text(
                                    endBlock.toStringAsFixed(0),
                                    style: GoogleFonts.righteous(
                                      color: Colors.white,
                                      fontSize: 26,
                                    ),
                                  ),
                                ],
                              ),
                            ),
                          ],
                        ),
                        const SizedBox(height: 12),
                        PollStepper(
                          thing: widget.thing,
                          info: info,
                          currentBlock: currentBlock.toInt(),
                          endBlock: endBlock.toInt(),
                        ),
                      ],
                    ),
                  );
                },
              );
            },
          ),
        ),
      ],
    );
  }
}
