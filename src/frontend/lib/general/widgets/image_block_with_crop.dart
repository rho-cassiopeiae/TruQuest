import 'dart:typed_data';

import 'package:flutter/material.dart';
import 'package:tuple/tuple.dart';

import 'image_selection_dialog.dart';
import '../contexts/document_context.dart';
import '../../widget_extensions.dart';

class ImageBlockWithCrop extends StatefulWidget {
  const ImageBlockWithCrop({super.key});

  @override
  State<ImageBlockWithCrop> createState() => _ImageBlockWithCropState();
}

class _ImageBlockWithCropState extends StateX<ImageBlockWithCrop> {
  late final _documentContext = useScoped<DocumentContext>();

  @override
  Widget build(BuildContext context) {
    return Column(
      children: [
        OutlinedButton(
          style: OutlinedButton.styleFrom(
            backgroundColor: Colors.yellow[700],
            foregroundColor: Colors.white,
            elevation: 5,
          ),
          child: Row(
            children: [
              Spacer(),
              Text('Image'),
              Expanded(
                child: Align(
                  alignment: Alignment.centerRight,
                  child: Icon(Icons.add),
                ),
              ),
            ],
          ),
          onPressed: () async {
            var result =
                await showDialog<Tuple3<String, Uint8List, Uint8List>?>(
              context: context,
              barrierDismissible: true,
              builder: (_) => ImageSelectionDialog(),
            );
            if (result != null) {
              setState(() {
                _documentContext.imageExt = result.item1;
                _documentContext.imageBytes = result.item2;
                _documentContext.croppedImageBytes = result.item3;
              });
            }
          },
        ),
        if (_documentContext.croppedImageBytes != null)
          Padding(
            padding: const EdgeInsets.only(top: 6),
            child: CircleAvatar(
              radius: 30,
              foregroundImage: MemoryImage(_documentContext.croppedImageBytes!),
            ),
          ),
      ],
    );
  }
}
