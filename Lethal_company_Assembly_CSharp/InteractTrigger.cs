// Decompiled with JetBrains decompiler
// Type: InteractTrigger
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class InteractTrigger : NetworkBehaviour
{
  [Header("Aesthetics")]
  public Sprite hoverIcon;
  public string hoverTip;
  [Space(5f)]
  public Sprite disabledHoverIcon;
  public string disabledHoverTip;
  [Header("Interaction")]
  public bool interactable = true;
  public bool oneHandedItemAllowed = true;
  public bool twoHandedItemAllowed;
  [Space(5f)]
  public bool holdInteraction;
  public float timeToHold = 0.5f;
  public float timeToHoldSpeedMultiplier = 1f;
  public string holdTip;
  public bool isBeingHeldByPlayer;
  public InteractEventFloat holdingInteractEvent;
  private float timeHeld;
  private bool isHoldingThisFrame;
  [Space(5f)]
  public bool touchTrigger;
  public bool triggerOnce;
  private bool hasTriggered;
  [Header("Misc")]
  public bool interactCooldown = true;
  public float cooldownTime = 1f;
  [HideInInspector]
  public float currentCooldownValue;
  public bool disableTriggerMesh = true;
  [Space(5f)]
  public bool RandomChanceTrigger;
  public int randomChancePercentage;
  [Header("Events")]
  public InteractEvent onInteract;
  public InteractEvent onInteractEarly;
  public InteractEvent onStopInteract;
  public InteractEvent onCancelAnimation;
  [Header("Special Animation")]
  public bool specialCharacterAnimation;
  public bool stopAnimationManually;
  public string stopAnimationString = "SA_stopAnimation";
  public bool hidePlayerItem;
  public bool isPlayingSpecialAnimation;
  public float animationWaitTime = 2f;
  public string animationString;
  [Space(5f)]
  public bool lockPlayerPosition;
  public Transform playerPositionNode;
  private Transform lockedPlayer;
  private bool usedByOtherClient;
  private StartOfRound playersManager;
  private float updateInterval = 1f;
  [Header("Ladders")]
  public bool isLadder;
  public Transform topOfLadderPosition;
  public bool useRaycastToGetTopPosition;
  public Transform bottomOfLadderPosition;
  public Transform ladderHorizontalPosition;
  [Space(5f)]
  public Transform ladderPlayerPositionNode;
  public bool usingLadder;
  private bool atBottomOfLadder;
  private Vector3 moveVelocity;
  private PlayerControllerB playerScriptInSpecialAnimation;
  private Coroutine useLadderCoroutine;
  private int playerUsingId;

  public void StopInteraction()
  {
    if (!this.isBeingHeldByPlayer)
      return;
    this.isBeingHeldByPlayer = false;
    this.onStopInteract.Invoke((PlayerControllerB) null);
  }

  public void HoldInteractNotFilled()
  {
    this.holdingInteractEvent.Invoke(HUDManager.Instance.holdFillAmount / this.timeToHold);
    if (this.specialCharacterAnimation || this.isLadder)
      return;
    if (!this.isBeingHeldByPlayer)
      this.onInteractEarly.Invoke((PlayerControllerB) null);
    this.isBeingHeldByPlayer = true;
  }

  public void Interact(Transform playerTransform)
  {
    if (this.triggerOnce && this.hasTriggered || StartOfRound.Instance.firingPlayersCutsceneRunning)
      return;
    this.hasTriggered = true;
    if (this.RandomChanceTrigger && Random.Range(0, 101) > this.randomChancePercentage)
      return;
    if (!this.interactable || this.isPlayingSpecialAnimation || this.usingLadder)
    {
      if (!this.usingLadder)
        return;
      this.CancelLadderAnimation();
    }
    else
    {
      PlayerControllerB component = playerTransform.GetComponent<PlayerControllerB>();
      if (component.inSpecialInteractAnimation && !component.isClimbingLadder)
        return;
      if (this.interactCooldown)
      {
        if ((double) this.currentCooldownValue >= 0.0)
          return;
        this.currentCooldownValue = this.cooldownTime;
      }
      if (!this.specialCharacterAnimation && !this.isLadder)
      {
        this.onInteract.Invoke(component);
      }
      else
      {
        component.ResetFallGravity();
        if (this.isLadder)
        {
          if (component.isInHangarShipRoom)
            return;
          this.ladderPlayerPositionNode.position = new Vector3(this.ladderHorizontalPosition.position.x, Mathf.Clamp(component.thisPlayerBody.position.y, this.bottomOfLadderPosition.position.y + 0.3f, this.topOfLadderPosition.position.y - 2.2f), this.ladderHorizontalPosition.position.z);
          if (this.LadderPositionObstructed(component))
            return;
          if (this.useLadderCoroutine != null)
            this.StopCoroutine(this.useLadderCoroutine);
          this.useLadderCoroutine = this.StartCoroutine(this.ladderClimbAnimation(component));
        }
        else
          this.StartCoroutine(this.specialInteractAnimation(component));
      }
    }
  }

  private bool LadderPositionObstructed(PlayerControllerB playerController)
  {
    if ((double) playerController.transform.position.y >= (double) this.topOfLadderPosition.position.y - 0.5)
    {
      if (Physics.Linecast(playerController.gameplayCamera.transform.position, this.ladderPlayerPositionNode.position + Vector3.up * 2.8f, out RaycastHit _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        return true;
    }
    else if (Physics.Linecast(playerController.gameplayCamera.transform.position, this.ladderPlayerPositionNode.position, out RaycastHit _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      return true;
    return false;
  }

  private IEnumerator ladderClimbAnimation(PlayerControllerB playerController)
  {
    InteractTrigger interactTrigger = this;
    interactTrigger.onInteractEarly.Invoke((PlayerControllerB) null);
    interactTrigger.lockedPlayer = playerController.thisPlayerBody;
    interactTrigger.playerScriptInSpecialAnimation = playerController;
    if (interactTrigger.hidePlayerItem && (Object) interactTrigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer != (Object) null)
      interactTrigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer.EnableItemMeshes(false);
    interactTrigger.SetUsingLadderOnLocalClient(true);
    interactTrigger.hoverTip = "Let go : [LMB]";
    if (!playerController.isTestingPlayer)
      playerController.UpdateSpecialAnimationValue(true, (short) interactTrigger.ladderPlayerPositionNode.eulerAngles.y, climbingLadder: true);
    playerController.enteringSpecialAnimation = true;
    playerController.inSpecialInteractAnimation = true;
    playerController.currentTriggerInAnimationWith = interactTrigger;
    playerController.isCrouching = false;
    playerController.playerBodyAnimator.SetBool("crouching", false);
    playerController.playerBodyAnimator.SetTrigger("EnterLadder");
    playerController.thisController.enabled = false;
    float timer = 0.0f;
    while ((double) timer <= (double) interactTrigger.animationWaitTime)
    {
      yield return (object) null;
      timer += Time.deltaTime;
      playerController.thisPlayerBody.position = Vector3.Lerp(playerController.thisPlayerBody.position, interactTrigger.ladderPlayerPositionNode.position, Mathf.SmoothStep(0.0f, 1f, timer / interactTrigger.animationWaitTime));
      interactTrigger.lockedPlayer.rotation = Quaternion.Lerp(interactTrigger.lockedPlayer.rotation, interactTrigger.ladderPlayerPositionNode.rotation, Mathf.SmoothStep(0.0f, 1f, timer / interactTrigger.animationWaitTime));
    }
    playerController.TeleportPlayer(interactTrigger.ladderPlayerPositionNode.position, allowInteractTrigger: true);
    Debug.Log((object) "Finished snapping to ladder");
    playerController.playerBodyAnimator.SetBool("ClimbingLadder", true);
    playerController.isClimbingLadder = true;
    playerController.enteringSpecialAnimation = false;
    playerController.ladderCameraHorizontal = 0.0f;
    playerController.clampCameraRotation = interactTrigger.bottomOfLadderPosition.eulerAngles;
    int finishClimbingLadder = 0;
    while (finishClimbingLadder == 0)
    {
      yield return (object) null;
      if ((double) playerController.thisPlayerBody.position.y < (double) interactTrigger.bottomOfLadderPosition.position.y)
        finishClimbingLadder = 1;
      else if ((double) playerController.thisPlayerBody.position.y + 2.0 > (double) interactTrigger.topOfLadderPosition.position.y)
        finishClimbingLadder = 2;
    }
    playerController.isClimbingLadder = false;
    playerController.playerBodyAnimator.SetBool("ClimbingLadder", false);
    if (finishClimbingLadder == 1)
      interactTrigger.ladderPlayerPositionNode.position = interactTrigger.bottomOfLadderPosition.position;
    else if (!interactTrigger.useRaycastToGetTopPosition)
    {
      interactTrigger.ladderPlayerPositionNode.position = interactTrigger.topOfLadderPosition.position;
    }
    else
    {
      Ray ray = new Ray(playerController.transform.position + Vector3.up, interactTrigger.topOfLadderPosition.position + Vector3.up - playerController.transform.position + Vector3.up);
      RaycastHit hitInfo;
      if (Physics.Linecast(playerController.transform.position + Vector3.up, interactTrigger.topOfLadderPosition.position + Vector3.up, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        Debug.DrawLine(playerController.transform.position + Vector3.up, interactTrigger.topOfLadderPosition.position + Vector3.up, Color.red, 10f);
        interactTrigger.ladderPlayerPositionNode.position = ray.GetPoint(Mathf.Max(hitInfo.distance - 1.2f, 0.0f));
        Debug.DrawRay(interactTrigger.ladderPlayerPositionNode.position, Vector3.up * 0.5f, Color.yellow, 10f);
      }
      else
      {
        Debug.DrawLine(playerController.transform.position + Vector3.up, interactTrigger.topOfLadderPosition.position + Vector3.up, Color.green, 10f);
        interactTrigger.ladderPlayerPositionNode.position = interactTrigger.topOfLadderPosition.position;
      }
    }
    timer = 0.0f;
    float shorterWaitTime = interactTrigger.animationWaitTime / 2f;
    while ((double) timer <= (double) shorterWaitTime)
    {
      yield return (object) null;
      timer += Time.deltaTime;
      playerController.thisPlayerBody.position = Vector3.Lerp(playerController.thisPlayerBody.position, interactTrigger.ladderPlayerPositionNode.position, Mathf.SmoothStep(0.0f, 1f, timer / shorterWaitTime));
      playerController.thisPlayerBody.rotation = Quaternion.Lerp(playerController.thisPlayerBody.rotation, interactTrigger.ladderPlayerPositionNode.rotation, Mathf.SmoothStep(0.0f, 1f, timer / shorterWaitTime));
      playerController.gameplayCamera.transform.rotation = Quaternion.Slerp(playerController.gameplayCamera.transform.rotation, playerController.gameplayCamera.transform.parent.rotation, Mathf.SmoothStep(0.0f, 1f, timer / shorterWaitTime));
    }
    playerController.gameplayCamera.transform.localEulerAngles = Vector3.zero;
    Debug.Log((object) "Finished ladder sequence");
    playerController.UpdateSpecialAnimationValue(false);
    playerController.inSpecialInteractAnimation = false;
    playerController.thisController.enabled = true;
    interactTrigger.SetUsingLadderOnLocalClient(false);
    interactTrigger.hoverTip = "Use ladder : [LMB]";
    interactTrigger.lockedPlayer = (Transform) null;
    interactTrigger.currentCooldownValue = interactTrigger.cooldownTime;
    interactTrigger.onInteract.Invoke((PlayerControllerB) null);
  }

  public void CancelAnimationExternally()
  {
    if (this.isLadder)
      this.CancelLadderAnimation();
    else
      this.StopSpecialAnimation();
  }

  public void CancelLadderAnimation()
  {
    if (this.useLadderCoroutine != null)
      this.StopCoroutine(this.useLadderCoroutine);
    this.onCancelAnimation.Invoke(this.playerScriptInSpecialAnimation);
    this.playerScriptInSpecialAnimation.currentTriggerInAnimationWith = (InteractTrigger) null;
    this.playerScriptInSpecialAnimation.isClimbingLadder = false;
    this.playerScriptInSpecialAnimation.thisController.enabled = true;
    this.playerScriptInSpecialAnimation.playerBodyAnimator.SetBool("ClimbingLadder", false);
    this.playerScriptInSpecialAnimation.gameplayCamera.transform.localEulerAngles = Vector3.zero;
    this.playerScriptInSpecialAnimation.UpdateSpecialAnimationValue(false);
    this.playerScriptInSpecialAnimation.inSpecialInteractAnimation = false;
    this.SetUsingLadderOnLocalClient(false);
    this.lockedPlayer = (Transform) null;
    this.currentCooldownValue = this.cooldownTime;
    if (this.hidePlayerItem && (Object) this.playerScriptInSpecialAnimation.currentlyHeldObjectServer != (Object) null)
      this.playerScriptInSpecialAnimation.currentlyHeldObjectServer.EnableItemMeshes(true);
    this.onInteract.Invoke((PlayerControllerB) null);
  }

  private void SetUsingLadderOnLocalClient(bool isUsing)
  {
    this.usingLadder = isUsing;
    if (isUsing)
      this.hoverTip = "Let go : [LMB]";
    else
      this.hoverTip = "Climb : [LMB]";
  }

  private IEnumerator specialInteractAnimation(PlayerControllerB playerController)
  {
    InteractTrigger interactTrigger = this;
    interactTrigger.UpdateUsedByPlayerServerRpc((int) playerController.playerClientId);
    interactTrigger.onInteractEarly.Invoke((PlayerControllerB) null);
    interactTrigger.isPlayingSpecialAnimation = true;
    interactTrigger.lockedPlayer = playerController.thisPlayerBody;
    interactTrigger.playerScriptInSpecialAnimation = playerController;
    if (interactTrigger.hidePlayerItem && (Object) interactTrigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer != (Object) null)
      interactTrigger.playerScriptInSpecialAnimation.currentlyHeldObjectServer.EnableItemMeshes(false);
    playerController.Crouch(false);
    playerController.UpdateSpecialAnimationValue(true, (short) interactTrigger.playerPositionNode.eulerAngles.y);
    playerController.inSpecialInteractAnimation = true;
    playerController.currentTriggerInAnimationWith = interactTrigger;
    playerController.playerBodyAnimator.ResetTrigger(interactTrigger.animationString);
    playerController.playerBodyAnimator.SetTrigger(interactTrigger.animationString);
    HUDManager.Instance.ClearControlTips();
    if (!interactTrigger.stopAnimationManually)
    {
      yield return (object) new WaitForSeconds(interactTrigger.animationWaitTime);
      interactTrigger.StopSpecialAnimation();
    }
  }

  public void StopSpecialAnimation()
  {
    if (this.isPlayingSpecialAnimation && this.stopAnimationManually && (Object) this.lockedPlayer != (Object) null)
    {
      Debug.Log((object) string.Format("Calling stop animation function StopUsing server rpc for player: {0}", (object) GameNetworkManager.Instance.localPlayerController.playerClientId));
      this.StopUsingServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    }
    if (!((Object) this.lockedPlayer != (Object) null))
      return;
    PlayerControllerB component = this.lockedPlayer.GetComponent<PlayerControllerB>();
    Debug.Log((object) ("STOPPING SPECIAL ANIMATION ON LOCAL CLIENT; player who was using: " + component.playerUsername));
    this.onCancelAnimation.Invoke(component);
    if (this.hidePlayerItem && (Object) component.currentlyHeldObjectServer != (Object) null)
      component.currentlyHeldObjectServer.EnableItemMeshes(true);
    this.isPlayingSpecialAnimation = false;
    component.inSpecialInteractAnimation = false;
    component.currentTriggerInAnimationWith = (InteractTrigger) null;
    if (component.isClimbingLadder)
    {
      this.CancelLadderAnimation();
      component.isClimbingLadder = false;
    }
    if (this.stopAnimationManually)
      component.playerBodyAnimator.SetTrigger(this.stopAnimationString);
    component.UpdateSpecialAnimationValue(false);
    this.lockedPlayer = (Transform) null;
    this.currentCooldownValue = this.cooldownTime;
    this.onInteract.Invoke((PlayerControllerB) null);
    if (!component.isHoldingObject || !((Object) component.currentlyHeldObjectServer != (Object) null))
      return;
    component.currentlyHeldObjectServer.SetControlTipsForItem();
  }

  [ServerRpc(RequireOwnership = false)]
  private void UpdateUsedByPlayerServerRpc(int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1430497838U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendServerRpc(ref bufferWriter, 1430497838U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UpdateUsedByPlayerClientRpc(playerNum);
  }

  [ClientRpc]
  private void UpdateUsedByPlayerClientRpc(int playerNum)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3458599252U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerNum);
      this.__endSendClientRpc(ref bufferWriter, 3458599252U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (this.hidePlayerItem && (Object) StartOfRound.Instance.allPlayerScripts[playerNum].currentlyHeldObjectServer != (Object) null)
    {
      StartOfRound.Instance.allPlayerScripts[playerNum].currentlyHeldObjectServer.EnableItemMeshes(false);
      this.playerUsingId = playerNum;
    }
    if (this.stopAnimationManually)
    {
      this.isPlayingSpecialAnimation = true;
      StartOfRound.Instance.allPlayerScripts[playerNum].currentTriggerInAnimationWith = this;
    }
    else
      this.StartCoroutine(this.isSpecialAnimationPlayingTimer(playerNum));
  }

  private IEnumerator isSpecialAnimationPlayingTimer(int playerNum)
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    InteractTrigger interactTrigger = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      StartOfRound.Instance.allPlayerScripts[playerNum].currentTriggerInAnimationWith = (InteractTrigger) null;
      interactTrigger.isPlayingSpecialAnimation = false;
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    StartOfRound.Instance.allPlayerScripts[playerNum].currentTriggerInAnimationWith = interactTrigger;
    interactTrigger.isPlayingSpecialAnimation = true;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = (object) new WaitForSeconds(interactTrigger.animationWaitTime);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  [ServerRpc(RequireOwnership = false)]
  private void StopUsingServerRpc(int playerUsing)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(880620475U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerUsing);
      this.__endSendServerRpc(ref bufferWriter, 880620475U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopUsingClientRpc(playerUsing);
  }

  [ClientRpc]
  private void StopUsingClientRpc(int playerUsing)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(953330655U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerUsing);
      this.__endSendClientRpc(ref bufferWriter, 953330655U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SetInteractTriggerNotInAnimation(playerUsing);
  }

  public void SetInteractTriggerNotInAnimation(int playerUsing = -1)
  {
    if (playerUsing == -1)
      playerUsing = this.playerUsingId;
    this.isPlayingSpecialAnimation = false;
    if (playerUsing == -1)
      return;
    if ((Object) StartOfRound.Instance.allPlayerScripts[playerUsing].currentlyHeldObjectServer != (Object) null)
      StartOfRound.Instance.allPlayerScripts[playerUsing].currentlyHeldObjectServer.EnableItemMeshes(true);
    StartOfRound.Instance.allPlayerScripts[playerUsing].currentTriggerInAnimationWith = (InteractTrigger) null;
    this.playerUsingId = -1;
  }

  private void LateUpdate()
  {
    if (!this.isPlayingSpecialAnimation || !((Object) this.lockedPlayer != (Object) null) || this.playerScriptInSpecialAnimation.isPlayerDead || !this.lockPlayerPosition)
      return;
    this.lockedPlayer.position = Vector3.Lerp(this.lockedPlayer.position, this.playerPositionNode.position, Time.deltaTime * 20f);
    this.lockedPlayer.rotation = Quaternion.Lerp(this.lockedPlayer.rotation, this.playerPositionNode.rotation, Time.deltaTime * 20f);
  }

  private void Update()
  {
    if ((double) this.currentCooldownValue >= 0.0)
      this.currentCooldownValue -= Time.deltaTime;
    if (this.isPlayingSpecialAnimation)
    {
      if (!((Object) this.lockedPlayer != (Object) null) || !this.playerScriptInSpecialAnimation.isPlayerDead)
        return;
      this.StopSpecialAnimation();
    }
    else
    {
      if (!this.usingLadder || !((Object) this.playerScriptInSpecialAnimation != (Object) null) || !this.playerScriptInSpecialAnimation.isPlayerDead)
        return;
      this.CancelLadderAnimation();
    }
  }

  private void OnTriggerEnter(Collider other)
  {
    if (!this.touchTrigger || !other.gameObject.CompareTag("Player") || !(bool) (Object) other.gameObject.GetComponent<PlayerControllerB>() || !other.gameObject.GetComponent<PlayerControllerB>().IsOwner)
      return;
    this.Interact(other.gameObject.GetComponent<PlayerControllerB>().thisPlayerBody);
  }

  private void Start()
  {
    if (this.disableTriggerMesh && (bool) (Object) this.gameObject.GetComponent<MeshRenderer>())
      this.gameObject.GetComponent<MeshRenderer>().enabled = false;
    this.playersManager = Object.FindObjectOfType<StartOfRound>();
  }

  public void SetInteractionToHold(bool mustHold) => this.holdInteraction = mustHold;

  public void SetInteractionToHoldOpposite(bool mustHold) => this.holdInteraction = !mustHold;

  public void SetRandomTimeToHold(float min, float max) => this.timeToHold = Random.Range(min, max);

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_InteractTrigger()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1430497838U, new NetworkManager.RpcReceiveHandler(InteractTrigger.__rpc_handler_1430497838)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3458599252U, new NetworkManager.RpcReceiveHandler(InteractTrigger.__rpc_handler_3458599252)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(880620475U, new NetworkManager.RpcReceiveHandler(InteractTrigger.__rpc_handler_880620475)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(953330655U, new NetworkManager.RpcReceiveHandler(InteractTrigger.__rpc_handler_953330655)));
  }

  private static void __rpc_handler_1430497838(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((InteractTrigger) target).UpdateUsedByPlayerServerRpc(playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3458599252(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerNum;
    ByteUnpacker.ReadValueBitPacked(reader, out playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((InteractTrigger) target).UpdateUsedByPlayerClientRpc(playerNum);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_880620475(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerUsing;
    ByteUnpacker.ReadValueBitPacked(reader, out playerUsing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((InteractTrigger) target).StopUsingServerRpc(playerUsing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_953330655(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerUsing;
    ByteUnpacker.ReadValueBitPacked(reader, out playerUsing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((InteractTrigger) target).StopUsingClientRpc(playerUsing);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (InteractTrigger);
}
