class SignInWithEthereumRvm {
  final String userId;
  final String signerAddress;
  final String walletAddress;
  final String token;

  SignInWithEthereumRvm.fromMap(Map<String, dynamic> map)
      : userId = map['userId'],
        signerAddress = map['signerAddress'],
        walletAddress = map['walletAddress'],
        token = map['token'];
}
