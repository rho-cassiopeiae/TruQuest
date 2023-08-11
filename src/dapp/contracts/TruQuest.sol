// SPDX-License-Identifier: AGPL-3.0-only
pragma solidity >=0.8.0 <0.9.0;

import "@ganache/console.log/console.sol";

import "./Truthserum.sol";

error TruQuest__ThingAlreadyFunded(bytes16 thingId);
error TruQuest__NotEnoughFunds(uint256 requiredAmount, uint256 availableAmount);
error TruQuest__Unauthorized();
error TruQuest__InvalidSignature();
error TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
    bytes16 thingId
);
error TruQuest__RequestedWithdrawAmountExceedsAvailable(
    uint256 requestedAmount,
    uint256 availableAmount
);

contract TruQuest {
    struct ThingTd {
        bytes16 id;
    }

    struct SettlementProposalTd {
        bytes16 thingId;
        bytes16 id;
    }

    struct SettlementProposal {
        bytes16 id;
        address submitter;
    }

    bytes private constant THING_TD = "ThingTd(bytes16 id)";
    bytes32 private immutable i_thingTdHash;
    bytes private constant SETTLEMENT_PROPOSAL_TD =
        "SettlementProposalTd(bytes16 thingId,bytes16 id)";
    bytes32 private immutable i_settlementProposalTdHash;
    bytes32 private constant SALT =
        0xf2d857f4a3edcb9b78b4d503bfe733db1e3f6cdc2b7971ee739626c97e86a558;
    bytes private constant DOMAIN_TD =
        "EIP712Domain(string name,string version,uint256 chainId,address verifyingContract,bytes32 salt)";
    bytes32 private immutable i_domainSeparator;

    Truthserum private immutable i_truthserum;
    address private s_thingSubmissionVerifierLotteryAddress;
    address private s_acceptancePollAddress;
    address private s_thingAssessmentVerifierLotteryAddress;
    address private s_assessmentPollAddress;

    address private s_orchestrator;

    uint256 public s_thingSubmissionStake;
    uint256 public s_verifierStake;
    uint256 public s_thingSettlementProposalStake;
    uint256 public s_thingSubmissionAcceptedReward;
    uint256 public s_thingSubmissionRejectedPenalty;
    uint256 public s_verifierReward;
    uint256 public s_verifierPenalty;
    uint256 public s_thingSettlementProposalAcceptedReward;
    uint256 public s_thingSettlementProposalRejectedPenalty;

    mapping(address => uint256) public s_balanceOf;
    mapping(address => uint256) public s_stakedBalanceOf;

    mapping(bytes16 => address) public s_thingSubmitter;
    mapping(bytes16 => SettlementProposal) public s_thingIdToSettlementProposal;

    event FundsDeposited(address indexed user, uint256 amount);

    event FundsWithdrawn(address indexed user, uint256 amount);

    event ThingFunded(
        bytes16 indexed thingId,
        address indexed user,
        uint256 thingStake
    );

    event ThingSettlementProposalFunded(
        bytes16 indexed thingId,
        bytes16 indexed settlementProposalId,
        address indexed user,
        uint256 thingSettlementProposalStake
    );

    modifier whenHasAtLeast(uint256 _requiredFunds) {
        uint256 availableFunds = getAvailableFunds(msg.sender);
        if (availableFunds < _requiredFunds) {
            revert TruQuest__NotEnoughFunds(_requiredFunds, availableFunds);
        }
        _;
    }

    modifier onlyOrchestrator() {
        if (msg.sender != s_orchestrator) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyVerifierLottery() {
        if (
            msg.sender != s_thingSubmissionVerifierLotteryAddress &&
            msg.sender != s_thingAssessmentVerifierLotteryAddress
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyPoll() {
        if (
            msg.sender != s_acceptancePollAddress &&
            msg.sender != s_assessmentPollAddress
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyAcceptancePoll() {
        if (msg.sender != s_acceptancePollAddress) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyAssessmentPoll() {
        if (msg.sender != s_assessmentPollAddress) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyLotteryOrPoll() {
        if (
            !(msg.sender == s_thingSubmissionVerifierLotteryAddress ||
                msg.sender == s_thingAssessmentVerifierLotteryAddress ||
                msg.sender == s_acceptancePollAddress ||
                msg.sender == s_assessmentPollAddress)
        ) {
            revert TruQuest__Unauthorized();
        }
        _;
    }

    modifier onlyWhenNotFunded(bytes16 _thingId) {
        if (s_thingSubmitter[_thingId] != address(0)) {
            revert TruQuest__ThingAlreadyFunded(_thingId);
        }
        _;
    }

    modifier onlyWhenNoProposalUnderAssessmentFor(bytes16 _thingId) {
        if (s_thingIdToSettlementProposal[_thingId].submitter != address(0)) {
            revert TruQuest__ThingAlreadyHasSettlementProposalUnderAssessment(
                _thingId
            );
        }
        _;
    }

    constructor(
        address _truthserumAddress,
        uint256 _verifierStake,
        uint256 _verifierReward,
        uint256 _verifierPenalty,
        uint256 _thingSubmissionStake,
        uint256 _thingSubmissionAcceptedReward,
        uint256 _thingSubmissionRejectedPenalty,
        uint256 _thingSettlementProposalStake,
        uint256 _thingSettlementProposalAcceptedReward,
        uint256 _thingSettlementProposalRejectedPenalty
    ) {
        i_truthserum = Truthserum(_truthserumAddress);
        s_orchestrator = msg.sender;
        s_verifierStake = _verifierStake;
        s_verifierReward = _verifierReward;
        s_verifierPenalty = _verifierPenalty;
        s_thingSubmissionStake = _thingSubmissionStake;
        s_thingSubmissionAcceptedReward = _thingSubmissionAcceptedReward;
        s_thingSubmissionRejectedPenalty = _thingSubmissionRejectedPenalty;
        s_thingSettlementProposalStake = _thingSettlementProposalStake;
        s_thingSettlementProposalAcceptedReward = _thingSettlementProposalAcceptedReward;
        s_thingSettlementProposalRejectedPenalty = _thingSettlementProposalRejectedPenalty;

        i_domainSeparator = keccak256(
            abi.encode(
                keccak256(DOMAIN_TD),
                keccak256("TruQuest"),
                keccak256("0.0.1"),
                block.chainid,
                address(this),
                SALT
            )
        );
        i_thingTdHash = keccak256(THING_TD);
        i_settlementProposalTdHash = keccak256(SETTLEMENT_PROPOSAL_TD);
    }

    function setLotteryAndPollAddresses(
        address _thingSubmissionVerifierLotteryAddress,
        address _acceptancePollAddress,
        address _thingAssessmentVerifierLotteryAddress,
        address _assessmentPollAddress
    ) external onlyOrchestrator {
        s_thingSubmissionVerifierLotteryAddress = _thingSubmissionVerifierLotteryAddress;
        s_acceptancePollAddress = _acceptancePollAddress;
        s_thingAssessmentVerifierLotteryAddress = _thingAssessmentVerifierLotteryAddress;
        s_assessmentPollAddress = _assessmentPollAddress;
    }

    function deposit(uint256 _amount) external {
        i_truthserum.transferFrom(msg.sender, address(this), _amount);
        s_balanceOf[msg.sender] += _amount;
        emit FundsDeposited(msg.sender, _amount);
    }

    function withdraw(uint256 _amount) external {
        uint256 availableAmount = getAvailableFunds(msg.sender);
        if (availableAmount < _amount) {
            revert TruQuest__RequestedWithdrawAmountExceedsAvailable(
                _amount,
                availableAmount
            );
        }

        // @@!!: With current dummy implementation it's possible for transfer
        // to fail due to insufficient balance. Ignore it for now since the entire
        // token design and emittance strategy will be rewritten before going live on mainnet.
        i_truthserum.transfer(msg.sender, _amount);
        s_balanceOf[msg.sender] -= _amount;

        emit FundsWithdrawn(msg.sender, _amount);
    }

    function _stake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] += _amount;
    }

    function _unstake(address _user, uint256 _amount) private {
        s_stakedBalanceOf[_user] -= _amount;
    }

    function stakeAsVerifier(address _user) external onlyVerifierLottery {
        s_stakedBalanceOf[_user] += s_verifierStake;
    }

    function unstakeAsVerifier(address _user) external onlyLotteryOrPoll {
        s_stakedBalanceOf[_user] -= s_verifierStake;
    }

    function unstakeThingSubmitter(address _user) external onlyLotteryOrPoll {
        _unstake(_user, s_thingSubmissionStake);
    }

    function unstakeProposalSubmitter(
        address _user
    ) external onlyLotteryOrPoll {
        _unstake(_user, s_thingSettlementProposalStake);
    }

    function unstakeAndRewardThingSubmitter(
        address _user
    ) external onlyAcceptancePoll {
        _unstake(_user, s_thingSubmissionStake);
        s_balanceOf[_user] += s_thingSubmissionAcceptedReward;
    }

    function unstakeAndSlashThingSubmitter(
        address _user
    ) external onlyAcceptancePoll {
        _unstake(_user, s_thingSubmissionStake);
        s_balanceOf[_user] -= s_thingSubmissionRejectedPenalty;
    }

    function unstakeAndRewardVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        s_balanceOf[_user] += s_verifierReward;
    }

    function unstakeAndSlashVerifier(address _user) external onlyPoll {
        _unstake(_user, s_verifierStake);
        s_balanceOf[_user] -= s_verifierPenalty;
    }

    function unstakeAndRewardProposalSubmitter(
        address _user
    ) external onlyAssessmentPoll {
        _unstake(_user, s_thingSettlementProposalStake);
        s_balanceOf[_user] += s_thingSettlementProposalAcceptedReward;
    }

    function unstakeAndSlashProposalSubmitter(
        address _user
    ) external onlyAssessmentPoll {
        _unstake(_user, s_thingSettlementProposalStake);
        s_balanceOf[_user] -= s_thingSettlementProposalRejectedPenalty;
    }

    function getAvailableFunds(address _user) public view returns (uint256) {
        return s_balanceOf[_user] - s_stakedBalanceOf[_user];
    }

    function checkHasEnoughFundsToStakeAsVerifier(
        address _user
    ) external view returns (bool) {
        return getAvailableFunds(_user) >= s_verifierStake;
    }

    function _hashThing(ThingTd memory _thing) private view returns (bytes32) {
        return
            keccak256(
                abi.encodePacked(
                    "\x19\x01",
                    i_domainSeparator,
                    keccak256(abi.encode(i_thingTdHash, _thing.id))
                )
            );
    }

    function _verifyOrchestratorSignatureForThing(
        ThingTd memory _thing,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    ) private view returns (bool) {
        return s_orchestrator == ecrecover(_hashThing(_thing), _v, _r, _s);
    }

    function checkThingAlreadyFunded(
        bytes16 _thingId
    ) external view returns (bool) {
        return s_thingSubmitter[_thingId] != address(0);
    }

    function fundThing(
        bytes16 _thingId,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        external
        onlyWhenNotFunded(_thingId)
        whenHasAtLeast(s_thingSubmissionStake)
    {
        ThingTd memory thing = ThingTd(_thingId);
        if (!_verifyOrchestratorSignatureForThing(thing, _v, _r, _s)) {
            revert TruQuest__InvalidSignature();
        }
        _stake(msg.sender, s_thingSubmissionStake);
        s_thingSubmitter[_thingId] = msg.sender;
        emit ThingFunded(_thingId, msg.sender, s_thingSubmissionStake);
    }

    function _hashSettlementProposal(
        SettlementProposalTd memory _settlementProposal
    ) private view returns (bytes32) {
        return
            keccak256(
                abi.encodePacked(
                    "\x19\x01",
                    i_domainSeparator,
                    keccak256(
                        abi.encode(
                            i_settlementProposalTdHash,
                            _settlementProposal.thingId,
                            _settlementProposal.id
                        )
                    )
                )
            );
    }

    function _verifyOrchestratorSignatureForSettlementProposal(
        SettlementProposalTd memory _settlementProposal,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    ) private view returns (bool) {
        return
            s_orchestrator ==
            ecrecover(_hashSettlementProposal(_settlementProposal), _v, _r, _s);
    }

    function checkThingAlreadyHasSettlementProposalUnderAssessment(
        bytes16 _thingId
    ) external view returns (bool) {
        return s_thingIdToSettlementProposal[_thingId].submitter != address(0);
    }

    function fundThingSettlementProposal(
        bytes16 _thingId,
        bytes16 _proposalId,
        uint8 _v,
        bytes32 _r,
        bytes32 _s
    )
        external
        onlyWhenNoProposalUnderAssessmentFor(_thingId)
        whenHasAtLeast(s_thingSettlementProposalStake)
    {
        SettlementProposalTd memory proposal = SettlementProposalTd(
            _thingId,
            _proposalId
        );
        if (
            !_verifyOrchestratorSignatureForSettlementProposal(
                proposal,
                _v,
                _r,
                _s
            )
        ) {
            revert TruQuest__InvalidSignature();
        }

        _stake(msg.sender, s_thingSettlementProposalStake);
        s_thingIdToSettlementProposal[_thingId] = SettlementProposal(
            _proposalId,
            msg.sender
        );

        emit ThingSettlementProposalFunded(
            _thingId,
            _proposalId,
            msg.sender,
            s_thingSettlementProposalStake
        );
    }

    function getSettlementProposalId(
        bytes16 _thingId
    ) external view returns (bytes16) {
        return s_thingIdToSettlementProposal[_thingId].id;
    }

    function getSettlementProposalSubmitter(
        bytes16 _thingId
    ) external view returns (address) {
        return s_thingIdToSettlementProposal[_thingId].submitter;
    }
}
