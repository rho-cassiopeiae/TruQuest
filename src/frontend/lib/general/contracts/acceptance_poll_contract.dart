import 'package:flutter_dotenv/flutter_dotenv.dart';

import '../../ethereum/services/ethereum_rpc_provider.dart';
import '../../ethereum_js_interop.dart';
import '../../thing/models/im/decision_im.dart';
import '../extensions/uuid_extension.dart';

class AcceptancePollContract {
  static final String address = dotenv.env['AcceptancePollAddress']!;
  static const String _abi = '''[
    {
      "inputs": [],
      "name": "getPollDurationBlocks",
      "outputs": [
        {
          "internalType": "uint16",
          "name": "",
          "type": "uint16"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        }
      ],
      "name": "getPollInitBlock",
      "outputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        },
        {
          "internalType": "uint16",
          "name": "_thingVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum AcceptancePoll.Vote",
          "name": "_vote",
          "type": "uint8"
        }
      ],
      "name": "castVote",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        },
        {
          "internalType": "uint16",
          "name": "_thingVerifiersArrayIndex",
          "type": "uint16"
        },
        {
          "internalType": "enum AcceptancePoll.Vote",
          "name": "_vote",
          "type": "uint8"
        },
        {
          "internalType": "string",
          "name": "_reason",
          "type": "string"
        }
      ],
      "name": "castVoteWithReason",
      "outputs": [],
      "stateMutability": "nonpayable",
      "type": "function"
    },
    {
      "inputs": [
        {
          "internalType": "bytes16",
          "name": "_thingId",
          "type": "bytes16"
        },
        {
          "internalType": "address",
          "name": "_user",
          "type": "address"
        }
      ],
      "name": "getUserIndexAmongThingVerifiers",
      "outputs": [
        {
          "internalType": "int256",
          "name": "",
          "type": "int256"
        }
      ],
      "stateMutability": "view",
      "type": "function"
    }
  ]''';

  late final Abi _interface;
  late final Contract _contract;

  AcceptancePollContract(EthereumRpcProvider ethereumRpcProvider) {
    _interface = Abi(_abi);
    _contract = Contract(
      address,
      _abi,
      ethereumRpcProvider.provider,
    );
  }

  Future<int> getPollDurationBlocks() =>
      _contract.read<int>('getPollDurationBlocks');

  Future<int?> getPollInitBlock(String thingId) async {
    var initBlock = await _contract.read<BigInt>(
      'getPollInitBlock',
      args: [thingId.toSolInputFormat()],
    );
    return initBlock != BigInt.zero ? initBlock.toInt() : null;
  }

  String castVote(
    String thingId,
    int userIndexInThingVerifiersArray,
    DecisionIm decision,
    String reason,
  ) {
    var thingIdHex = thingId.toSolInputFormat();
    return reason.isEmpty
        ? _interface.encodeFunctionData(
            'castVote',
            [
              thingIdHex,
              userIndexInThingVerifiersArray,
              decision.index,
            ],
          )
        : _interface.encodeFunctionData(
            'castVoteWithReason',
            [
              thingIdHex,
              userIndexInThingVerifiersArray,
              decision.index,
              reason,
            ],
          );
  }

  Future<int> getUserIndexAmongThingVerifiers(
    String thingId,
    String walletAddress,
  ) async {
    var index = await _contract.read<BigInt>(
      'getUserIndexAmongThingVerifiers',
      args: [thingId.toSolInputFormat(), walletAddress],
    );
    return index.toInt();
  }
}
