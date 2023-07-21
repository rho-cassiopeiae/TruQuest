import 'dart:async';

import '../models/vm/user_vm.dart';
import 'user_actions.dart';
import 'user_result_vm.dart';
import '../services/user_service.dart';
import '../../general/bloc/bloc.dart';

class UserBloc extends Bloc<UserAction> {
  final UserService _userService;

  Stream<UserVm> get currentUser$ => _userService.currentUserChanged$;
  UserVm? get latestCurrentUser => _userService.latestCurrentUser;
  Stream<List<String>> get walletAddresses$ => _userService.walletAddresses$;

  UserBloc(this._userService) {
    actionChannel.stream.listen((action) {
      if (action is CreateSmartWallet) {
        _createSmartWallet(action);
      } else if (action is CreateSmartWalletFromMnemonic) {
        _createSmartWalletFromMnemonic(action);
      } else if (action is EncryptAndSaveSmartWallet) {
        _encryptAndSaveSmartWallet(action);
      } else if (action is SignInWithEthereum) {
        _signInWithEthereum(action);
      } else if (action is AddEmail) {
        _addEmail(action);
      } else if (action is ConfirmEmail) {
        _confirmEmail(action);
      } else if (action is UnlockWallet) {
        _unlockWallet(action);
      } else if (action is AddAccount) {
        _addAccount(action);
      } else if (action is SwitchAccount) {
        _switchAccount(action);
      } else if (action is ApproveFundsUsage) {
        _approveFundsUsage(action);
      } else if (action is DepositFunds) {
        _depositFunds(action);
      }
    });
  }

  void _createSmartWallet(CreateSmartWallet action) async {
    var wallet = await _userService.createSmartWallet();
    action.complete(CreateSmartWalletSuccessVm(wallet: wallet));
  }

  void _createSmartWalletFromMnemonic(
    CreateSmartWalletFromMnemonic action,
  ) async {
    var wallet = await _userService.createSmartWalletFromMnemonic(
      action.mnemonic,
    );
    action.complete(CreateSmartWalletFromMnemonicSuccessVm(wallet: wallet));
  }

  void _encryptAndSaveSmartWallet(EncryptAndSaveSmartWallet action) async {
    await _userService.encryptSmartWalletAndSaveToLocalStorage(
      action.wallet,
      action.password,
    );
    action.complete(EncryptAndSaveSmartWalletSuccessVm());
  }

  void _signInWithEthereum(SignInWithEthereum action) async {
    await _userService.signInWithEthereum(action.password);
    action.complete(SignInWithEthereumSuccessVm());
  }

  void _addEmail(AddEmail action) async {
    await _userService.addEmail(action.email);
    action.complete(AddEmailSuccessVm());
  }

  void _confirmEmail(ConfirmEmail action) async {
    await _userService.confirmEmail(action.confirmationToken);
    action.complete(ConfirmEmailSuccessVm());
  }

  void _unlockWallet(UnlockWallet action) async {
    await _userService.unlockWallet(action.password);
    action.complete(UnlockWalletSuccessVm());
  }

  void _addAccount(AddAccount action) async {
    var error = await _userService.addAccount();
    action.complete(error != null ? AddAccountFailureVm(error: error) : null);
  }

  void _switchAccount(SwitchAccount action) async {
    await _userService.switchAccount(action.walletAddress);
    action.complete(SwitchAccountSuccessVm());
  }

  void _approveFundsUsage(ApproveFundsUsage action) async {}

  void _depositFunds(DepositFunds action) async {
    var error = await _userService.depositFunds(action.amount);
    action.complete(error != null ? DepositFundsFailureVm(error: error) : null);
  }
}
