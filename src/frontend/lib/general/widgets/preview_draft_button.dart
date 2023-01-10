import 'package:flutter/material.dart';

import 'progress_report.dart';
import '../../thing/bloc/thing_result_vm.dart';
import '../../thing/bloc/thing_actions.dart';
import '../contexts/document_context.dart';
import '../../thing/bloc/thing_bloc.dart';
import '../../widget_extensions.dart';

class PreviewDraftButton extends StatefulWidget {
  const PreviewDraftButton({super.key});

  @override
  State<PreviewDraftButton> createState() => _PreviewDraftButtonState();
}

class _PreviewDraftButtonState extends StateX<PreviewDraftButton> {
  late final _documentContext = useScoped<DocumentContext>();
  late final _thingBloc = use<ThingBloc>();

  bool _enabled = true;

  @override
  Widget build(BuildContext context) {
    return ElevatedButton(
      style: ElevatedButton.styleFrom(
        backgroundColor: Colors.amber[900],
        elevation: 8,
        padding: const EdgeInsets.symmetric(
          horizontal: 36,
          vertical: 16,
        ),
      ),
      child: _enabled ? Text('Preview draft') : CircularProgressIndicator(),
      onPressed: _enabled
          ? () async {
              var action = CreateNewThingDraft(
                documentContext: _documentContext,
              );
              _thingBloc.dispatch(action);

              setState(() {
                _enabled = false;
              });

              var vm = await action.result;
              if (vm is CreateNewThingDraftFailureVm) {
                // ...
              }

              vm as CreateNewThingDraftSuccessVm;

              showDialog(
                context: context,
                barrierDismissible: false,
                builder: (_) => ProgressReport(progress$: vm.progress$),
              );
            }
          : null,
    );
  }
}
