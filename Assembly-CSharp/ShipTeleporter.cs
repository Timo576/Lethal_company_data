// Decompiled with JetBrains decompiler
// Type: ShipTeleporter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

#nullable disable
public class ShipTeleporter : NetworkBehaviour
{
  public bool isInverseTeleporter;
  public Transform teleportOutPosition;
  [Space(5f)]
  public Transform teleporterPosition;
  public Animator teleporterAnimator;
  public Animator buttonAnimator;
  public AudioSource buttonAudio;
  public AudioSource shipTeleporterAudio;
  public AudioClip buttonPressSFX;
  public AudioClip teleporterSpinSFX;
  public AudioClip teleporterBeamUpSFX;
  public AudioClip beamUpPlayerBodySFX;
  private Coroutine beamUpPlayerCoroutine;
  public int teleporterId = 1;
  private int[] playersBeingTeleported;
  private float cooldownTime;
  public float cooldownAmount;
  public InteractTrigger buttonTrigger;
  public static bool hasBeenSpawnedThisSession;
  public static bool hasBeenSpawnedThisSessionInverse;
  private System.Random shipTeleporterSeed;

  public void SetRandomSeed()
  {
    if (!this.isInverseTeleporter)
      return;
    this.shipTeleporterSeed = new System.Random(StartOfRound.Instance.randomMapSeed + 17 + (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  private void Awake()
  {
    this.playersBeingTeleported = new int[4]
    {
      -1,
      -1,
      -1,
      -1
    };
    if (this.isInverseTeleporter && ShipTeleporter.hasBeenSpawnedThisSessionInverse || !this.isInverseTeleporter && ShipTeleporter.hasBeenSpawnedThisSession)
    {
      this.buttonTrigger.interactable = false;
      this.cooldownTime = this.cooldownAmount;
    }
    else if (this.isInverseTeleporter && !StartOfRound.Instance.inShipPhase)
      this.SetRandomSeed();
    if (this.isInverseTeleporter)
      ShipTeleporter.hasBeenSpawnedThisSessionInverse = true;
    else
      ShipTeleporter.hasBeenSpawnedThisSession = true;
  }

  private void Update()
  {
    if (this.buttonTrigger.interactable)
      return;
    if ((double) this.cooldownTime <= 0.0)
    {
      this.buttonTrigger.interactable = true;
    }
    else
    {
      this.buttonTrigger.disabledHoverTip = string.Format("[Cooldown: {0} sec.]", (object) (int) this.cooldownTime);
      this.cooldownTime -= Time.deltaTime;
    }
  }

  private void OnDisable()
  {
    for (int index = 0; index < this.playersBeingTeleported.Length; ++index)
    {
      if (this.playersBeingTeleported[index] == this.teleporterId)
        StartOfRound.Instance.allPlayerScripts[this.playersBeingTeleported[index]].shipTeleporterId = -1;
    }
    StartOfRound.Instance.StartNewRoundEvent.RemoveListener(new UnityAction(this.SetRandomSeed));
  }

  private void OnEnable()
  {
    StartOfRound.Instance.StartNewRoundEvent.AddListener(new UnityAction(this.SetRandomSeed));
  }

  public void PressTeleportButtonOnLocalClient()
  {
    if (this.isInverseTeleporter && (StartOfRound.Instance.inShipPhase || SceneManager.sceneCount <= 1))
      return;
    this.PressTeleportButtonServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void PressTeleportButtonServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(389447712U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 389447712U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PressTeleportButtonClientRpc();
  }

  [ClientRpc]
  public void PressTeleportButtonClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2773756087U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2773756087U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.PressButtonEffects();
    if (this.beamUpPlayerCoroutine != null)
      this.StopCoroutine(this.beamUpPlayerCoroutine);
    this.cooldownTime = this.cooldownAmount;
    this.buttonTrigger.interactable = false;
    if (this.isInverseTeleporter)
    {
      if (!this.CanUseInverseTeleporter())
        return;
      this.beamUpPlayerCoroutine = this.StartCoroutine(this.beamOutPlayer());
    }
    else
      this.beamUpPlayerCoroutine = this.StartCoroutine(this.beamUpPlayer());
  }

  private void PressButtonEffects()
  {
    this.buttonAnimator.SetTrigger("press");
    this.buttonAnimator.SetBool("GlassOpen", false);
    this.buttonAnimator.GetComponentInChildren<AnimatedObjectTrigger>().boolValue = false;
    if (this.isInverseTeleporter)
    {
      if (this.CanUseInverseTeleporter())
        this.teleporterAnimator.SetTrigger("useInverseTeleporter");
      else
        Debug.Log((object) string.Format("Using inverse teleporter was not allowed; {0}; {1}", (object) StartOfRound.Instance.inShipPhase, (object) StartOfRound.Instance.currentLevel.PlanetName));
    }
    else
      this.teleporterAnimator.SetTrigger("useTeleporter");
    this.buttonAudio.PlayOneShot(this.buttonPressSFX);
    WalkieTalkie.TransmitOneShotAudio(this.buttonAudio, this.buttonPressSFX);
  }

  private bool CanUseInverseTeleporter()
  {
    return !StartOfRound.Instance.inShipPhase && StartOfRound.Instance.currentLevel.spawnEnemiesAndScrap;
  }

  private IEnumerator beamOutPlayer()
  {
    if (!((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) null))
    {
      if (StartOfRound.Instance.inShipPhase)
      {
        Debug.Log((object) "Attempted using teleporter while in ship phase");
      }
      else
      {
        this.shipTeleporterAudio.PlayOneShot(this.teleporterSpinSFX);
        for (int b = 0; b < 5; ++b)
        {
          for (int index = 0; index < StartOfRound.Instance.allPlayerObjects.Length; ++index)
          {
            PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[index];
            Vector3 position = allPlayerScript.transform.position;
            if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
              position = allPlayerScript.deadBody.bodyParts[5].transform.position;
            if ((double) Vector3.Distance(position, this.teleportOutPosition.position) > 2.0)
            {
              if (allPlayerScript.shipTeleporterId != 1)
              {
                if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
                {
                  allPlayerScript.deadBody.beamOutParticle.Stop();
                  allPlayerScript.deadBody.bodyAudio.Stop();
                }
                else
                {
                  allPlayerScript.beamOutBuildupParticle.Stop();
                  allPlayerScript.movementAudio.Stop();
                }
              }
            }
            else if (allPlayerScript.shipTeleporterId == 1)
            {
              Debug.Log((object) string.Format("Cancelled teleporting #{0} with inverse teleporter; {1}", (object) allPlayerScript.playerClientId, (object) allPlayerScript.shipTeleporterId));
            }
            else
            {
              this.SetPlayerTeleporterId(allPlayerScript, 2);
              if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
              {
                if ((UnityEngine.Object) allPlayerScript.deadBody.beamUpParticle == (UnityEngine.Object) null)
                  yield break;
                else if (!allPlayerScript.deadBody.beamOutParticle.isPlaying)
                {
                  allPlayerScript.deadBody.beamOutParticle.Play();
                  allPlayerScript.deadBody.bodyAudio.PlayOneShot(this.beamUpPlayerBodySFX);
                }
              }
              else if (!allPlayerScript.beamOutBuildupParticle.isPlaying)
              {
                allPlayerScript.beamOutBuildupParticle.Play();
                allPlayerScript.movementAudio.PlayOneShot(this.beamUpPlayerBodySFX);
              }
            }
          }
          yield return (object) new WaitForSeconds(1f);
        }
        for (int index = 0; index < StartOfRound.Instance.allPlayerObjects.Length; ++index)
        {
          PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[index];
          if (allPlayerScript.shipTeleporterId == 1)
          {
            Debug.Log((object) string.Format("Player #{0} is in teleport 1, skipping", (object) allPlayerScript.playerClientId));
          }
          else
          {
            this.SetPlayerTeleporterId(allPlayerScript, -1);
            if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
            {
              allPlayerScript.deadBody.beamOutParticle.Stop();
              allPlayerScript.deadBody.bodyAudio.Stop();
            }
            else
            {
              allPlayerScript.beamOutBuildupParticle.Stop();
              allPlayerScript.movementAudio.Stop();
            }
            if (!((UnityEngine.Object) allPlayerScript != (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController) && !StartOfRound.Instance.inShipPhase)
            {
              Vector3 position1 = allPlayerScript.transform.position;
              if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
                position1 = allPlayerScript.deadBody.bodyParts[5].transform.position;
              if ((double) Vector3.Distance(position1, this.teleportOutPosition.position) < 2.0)
              {
                if (RoundManager.Instance.insideAINodes.Length != 0)
                {
                  Vector3 position2 = RoundManager.Instance.insideAINodes[this.shipTeleporterSeed.Next(0, RoundManager.Instance.insideAINodes.Length)].transform.position;
                  Debug.DrawRay(position2, Vector3.up * 1f, Color.red);
                  Vector3 inBoxPredictable = RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(position2, randomSeed: this.shipTeleporterSeed);
                  Debug.DrawRay(inBoxPredictable + Vector3.right * 0.01f, Vector3.up * 3f, Color.green);
                  this.SetPlayerTeleporterId(allPlayerScript, 2);
                  if ((UnityEngine.Object) allPlayerScript.deadBody != (UnityEngine.Object) null)
                  {
                    this.TeleportPlayerBodyOutServerRpc((int) allPlayerScript.playerClientId, inBoxPredictable);
                  }
                  else
                  {
                    this.TeleportPlayerOutWithInverseTeleporter((int) allPlayerScript.playerClientId, inBoxPredictable);
                    this.TeleportPlayerOutServerRpc((int) allPlayerScript.playerClientId, inBoxPredictable);
                  }
                }
              }
              else
                Debug.Log((object) string.Format("Player #{0} is not close enough to teleporter to beam out", (object) allPlayerScript.playerClientId));
            }
          }
        }
      }
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void TeleportPlayerOutServerRpc(int playerObj, Vector3 teleportPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3033548568U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      bufferWriter.WriteValueSafe(in teleportPos);
      this.__endSendServerRpc(ref bufferWriter, 3033548568U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.TeleportPlayerOutClientRpc(playerObj, teleportPos);
  }

  [ClientRpc]
  public void TeleportPlayerOutClientRpc(int playerObj, Vector3 teleportPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3527914562U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      bufferWriter.WriteValueSafe(in teleportPos);
      this.__endSendClientRpc(ref bufferWriter, 3527914562U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || StartOfRound.Instance.inShipPhase || StartOfRound.Instance.allPlayerScripts[playerObj].IsOwner)
      return;
    this.TeleportPlayerOutWithInverseTeleporter(playerObj, teleportPos);
  }

  private void TeleportPlayerOutWithInverseTeleporter(int playerObj, Vector3 teleportPos)
  {
    if (StartOfRound.Instance.allPlayerScripts[playerObj].isPlayerDead)
    {
      this.StartCoroutine(this.teleportBodyOut(playerObj, teleportPos));
    }
    else
    {
      PlayerControllerB allPlayerScript = StartOfRound.Instance.allPlayerScripts[playerObj];
      this.SetPlayerTeleporterId(allPlayerScript, -1);
      allPlayerScript.DropAllHeldItems();
      if ((bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
        UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[2].ChangeAudioReverbForPlayer(allPlayerScript);
      allPlayerScript.isInElevator = false;
      allPlayerScript.isInHangarShipRoom = false;
      allPlayerScript.isInsideFactory = true;
      allPlayerScript.averageVelocity = 0.0f;
      allPlayerScript.velocityLastFrame = Vector3.zero;
      StartOfRound.Instance.allPlayerScripts[playerObj].TeleportPlayer(teleportPos);
      StartOfRound.Instance.allPlayerScripts[playerObj].beamOutParticle.Play();
      this.shipTeleporterAudio.PlayOneShot(this.teleporterBeamUpSFX);
      StartOfRound.Instance.allPlayerScripts[playerObj].movementAudio.PlayOneShot(this.teleporterBeamUpSFX);
      if (!((UnityEngine.Object) allPlayerScript == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
        return;
      Debug.Log((object) "Teleporter shaking camera");
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void TeleportPlayerBodyOutServerRpc(int playerObj, Vector3 teleportPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(660932683U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      bufferWriter.WriteValueSafe(in teleportPos);
      this.__endSendServerRpc(ref bufferWriter, 660932683U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.TeleportPlayerBodyOutClientRpc(playerObj, teleportPos);
  }

  [ClientRpc]
  public void TeleportPlayerBodyOutClientRpc(int playerObj, Vector3 teleportPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1544539621U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObj);
      bufferWriter.WriteValueSafe(in teleportPos);
      this.__endSendClientRpc(ref bufferWriter, 1544539621U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.StartCoroutine(this.teleportBodyOut(playerObj, teleportPos));
  }

  private IEnumerator teleportBodyOut(int playerObj, Vector3 teleportPosition)
  {
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[playerObj].deadBody != (UnityEngine.Object) null || (double) Time.realtimeSinceStartup - (double) startTime > 2.0));
    if (!StartOfRound.Instance.inShipPhase && SceneManager.sceneCount > 1)
    {
      DeadBodyInfo deadBody = StartOfRound.Instance.allPlayerScripts[playerObj].deadBody;
      this.SetPlayerTeleporterId(StartOfRound.Instance.allPlayerScripts[playerObj], -1);
      if ((UnityEngine.Object) deadBody != (UnityEngine.Object) null)
      {
        deadBody.attachedTo = (Transform) null;
        deadBody.attachedLimb = (Rigidbody) null;
        deadBody.secondaryAttachedLimb = (Rigidbody) null;
        deadBody.secondaryAttachedTo = (Transform) null;
        if ((UnityEngine.Object) deadBody.grabBodyObject != (UnityEngine.Object) null && deadBody.grabBodyObject.isHeld && (UnityEngine.Object) deadBody.grabBodyObject.playerHeldBy != (UnityEngine.Object) null)
          deadBody.grabBodyObject.playerHeldBy.DropAllHeldItems();
        deadBody.isInShip = false;
        deadBody.parentedToShip = false;
        deadBody.transform.SetParent((Transform) null, true);
        deadBody.SetRagdollPositionSafely(teleportPosition, true);
      }
    }
  }

  private IEnumerator beamUpPlayer()
  {
    this.shipTeleporterAudio.PlayOneShot(this.teleporterSpinSFX);
    PlayerControllerB playerToBeamUp = StartOfRound.Instance.mapScreen.targetedPlayer;
    if ((UnityEngine.Object) playerToBeamUp == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Targeted player is null");
    }
    else
    {
      if ((UnityEngine.Object) playerToBeamUp.redirectToEnemy != (UnityEngine.Object) null)
      {
        Debug.Log((object) string.Format("Attemping to teleport enemy '{0}' (tied to player #{1}) to ship.", (object) playerToBeamUp.redirectToEnemy.gameObject.name, (object) playerToBeamUp.playerClientId));
        if (StartOfRound.Instance.shipIsLeaving)
          Debug.Log((object) string.Format("Ship could not teleport enemy '{0}' (tied to player #{1}) because the ship is leaving the nav mesh.", (object) playerToBeamUp.redirectToEnemy.gameObject.name, (object) playerToBeamUp.playerClientId));
        playerToBeamUp.redirectToEnemy.ShipTeleportEnemy();
        yield return (object) new WaitForSeconds(3f);
        this.shipTeleporterAudio.PlayOneShot(this.teleporterBeamUpSFX);
        if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
          HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
      }
      this.SetPlayerTeleporterId(playerToBeamUp, 1);
      if ((UnityEngine.Object) playerToBeamUp.deadBody != (UnityEngine.Object) null)
      {
        if ((UnityEngine.Object) playerToBeamUp.deadBody.beamUpParticle == (UnityEngine.Object) null)
        {
          yield break;
        }
        else
        {
          playerToBeamUp.deadBody.beamUpParticle.Play();
          playerToBeamUp.deadBody.bodyAudio.PlayOneShot(this.beamUpPlayerBodySFX);
        }
      }
      else
      {
        playerToBeamUp.beamUpParticle.Play();
        playerToBeamUp.movementAudio.PlayOneShot(this.beamUpPlayerBodySFX);
      }
      Debug.Log((object) "Teleport A");
      yield return (object) new WaitForSeconds(3f);
      bool flag = false;
      if ((UnityEngine.Object) playerToBeamUp.deadBody != (UnityEngine.Object) null)
      {
        if ((UnityEngine.Object) playerToBeamUp.deadBody.grabBodyObject == (UnityEngine.Object) null || !playerToBeamUp.deadBody.grabBodyObject.isHeldByEnemy)
        {
          flag = true;
          playerToBeamUp.deadBody.attachedTo = (Transform) null;
          playerToBeamUp.deadBody.attachedLimb = (Rigidbody) null;
          playerToBeamUp.deadBody.secondaryAttachedLimb = (Rigidbody) null;
          playerToBeamUp.deadBody.secondaryAttachedTo = (Transform) null;
          playerToBeamUp.deadBody.SetRagdollPositionSafely(this.teleporterPosition.position, true);
          playerToBeamUp.deadBody.transform.SetParent(StartOfRound.Instance.elevatorTransform, true);
          if ((UnityEngine.Object) playerToBeamUp.deadBody.grabBodyObject != (UnityEngine.Object) null && playerToBeamUp.deadBody.grabBodyObject.isHeld && (UnityEngine.Object) playerToBeamUp.deadBody.grabBodyObject.playerHeldBy != (UnityEngine.Object) null)
            playerToBeamUp.deadBody.grabBodyObject.playerHeldBy.DropAllHeldItems();
        }
      }
      else
      {
        flag = true;
        playerToBeamUp.DropAllHeldItems();
        if ((bool) (UnityEngine.Object) UnityEngine.Object.FindObjectOfType<AudioReverbPresets>())
          UnityEngine.Object.FindObjectOfType<AudioReverbPresets>().audioPresets[3].ChangeAudioReverbForPlayer(playerToBeamUp);
        playerToBeamUp.isInElevator = true;
        playerToBeamUp.isInHangarShipRoom = true;
        playerToBeamUp.isInsideFactory = false;
        playerToBeamUp.averageVelocity = 0.0f;
        playerToBeamUp.velocityLastFrame = Vector3.zero;
        playerToBeamUp.TeleportPlayer(this.teleporterPosition.position, true, 160f);
      }
      Debug.Log((object) "Teleport B");
      this.SetPlayerTeleporterId(playerToBeamUp, -1);
      if (flag)
      {
        this.shipTeleporterAudio.PlayOneShot(this.teleporterBeamUpSFX);
        if (GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom)
          HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
      }
      Debug.Log((object) "Teleport C");
    }
  }

  private void SetPlayerTeleporterId(PlayerControllerB playerScript, int teleporterId)
  {
    playerScript.shipTeleporterId = teleporterId;
    this.playersBeingTeleported[playerScript.playerClientId] = (int) playerScript.playerClientId;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_ShipTeleporter()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(389447712U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_389447712)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2773756087U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_2773756087)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3033548568U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_3033548568)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3527914562U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_3527914562)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(660932683U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_660932683)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1544539621U, new NetworkManager.RpcReceiveHandler(ShipTeleporter.__rpc_handler_1544539621)));
  }

  private static void __rpc_handler_389447712(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipTeleporter) target).PressTeleportButtonServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2773756087(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipTeleporter) target).PressTeleportButtonClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3033548568(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    Vector3 teleportPos;
    reader.ReadValueSafe(out teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipTeleporter) target).TeleportPlayerOutServerRpc(playerObj, teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3527914562(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    Vector3 teleportPos;
    reader.ReadValueSafe(out teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipTeleporter) target).TeleportPlayerOutClientRpc(playerObj, teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_660932683(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    Vector3 teleportPos;
    reader.ReadValueSafe(out teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((ShipTeleporter) target).TeleportPlayerBodyOutServerRpc(playerObj, teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1544539621(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObj;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObj);
    Vector3 teleportPos;
    reader.ReadValueSafe(out teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((ShipTeleporter) target).TeleportPlayerBodyOutClientRpc(playerObj, teleportPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (ShipTeleporter);
}
