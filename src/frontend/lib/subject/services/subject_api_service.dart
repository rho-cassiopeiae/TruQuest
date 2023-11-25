import 'dart:typed_data';

import 'package:dio/dio.dart';

import '../../general/utils/utils.dart';
import '../models/vm/subject_preview_vm.dart';
import '../models/vm/get_things_list_rvm.dart';
import '../models/im/new_subject_im.dart';
import '../../general/models/im/tag_im.dart';
import '../models/im/subject_type_im.dart';
import '../../general/services/server_connector.dart';
import '../models/vm/subject_vm.dart';

class SubjectApiService {
  final ServerConnector _serverConnector;
  final Dio _dio;

  SubjectApiService(this._serverConnector) : _dio = _serverConnector.dio;

  Future<String> addNewSubject(
    SubjectTypeIm type,
    String name,
    String details,
    String imageExt,
    Uint8List imageBytes,
    Uint8List croppedImageBytes,
    List<int> tags,
  ) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var input = NewSubjectIm(
        type: type,
        name: name,
        details: details,
        imageExt: imageExt,
        imageBytes: imageBytes,
        croppedImageExt: 'png',
        croppedImageBytes: croppedImageBytes,
        tags: tags.map((tagId) => TagIm(id: tagId)).toList(),
      );

      var response = await _dio.post(
        '/subjects/add',
        options: Options(
          headers: {'Authorization': 'Bearer $accessToken'},
        ),
        data: input.toFormData(),
      );

      return response.data['data'] as String;
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<List<SubjectPreviewVm>> getSubjects() async {
    try {
      var response = await _dio.get('/subjects');
      return List.unmodifiable(
        (response.data['data'] as List<dynamic>).map((map) => SubjectPreviewVm.fromMap(map)),
      );
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<SubjectVm> getSubject(String subjectId) async {
    try {
      var response = await _dio.get('/subjects/$subjectId');
      return SubjectVm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }

  Future<GetThingsListRvm> getThingsList(String subjectId) async {
    var accessToken = (await _serverConnector.latestConnection).$2;
    try {
      var response = await _dio.get(
        '/subjects/$subjectId/things',
        options: accessToken != null
            ? Options(
                headers: {'Authorization': 'Bearer $accessToken'},
              )
            : null,
      );

      return GetThingsListRvm.fromMap(response.data['data']);
    } on DioError catch (error) {
      throw wrapError(error);
    }
  }
}
