// Decompiled with JetBrains decompiler
// Type: DressGirlAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
public class DressGirlAI : EnemyAI
{
  public PlayerControllerB hauntingPlayer;
  public bool hauntingLocalPlayer;
  public float timer;
  public float hauntInterval;
  private bool couldNotStareLastAttempt;
  public float staringTimer;
  public bool staringInHaunt;
  private int timesSeenByPlayer;
  private int timesStared;
  private bool seenByPlayerThisTime;
  private bool playerApproachedThisTime;
  public bool disappearingFromStare;
  private bool disappearByVanishing;
  private bool choseDisappearingPosition;
  private int timesChased;
  private float chaseTimer;
  public GameObject[] outsideNodes;
  public NavMeshHit navHit;
  private Coroutine disappearOnDelayCoroutine;
  public Transform turnCompass;
  public AudioClip[] appearStaringSFX;
  public AudioClip skipWalkSFX;
  public AudioClip breathingSFX;
  public float SFXVolumeLerpTo = 1f;
  public AudioSource heartbeatMusic;
  private bool enemyMeshEnabled;
  private System.Random ghostGirlRandom;
  private bool initializedRandomSeed;
  private bool switchedHauntingPlayer;
  private Coroutine switchHauntedPlayerCoroutine;
  private int timesChoosingAPlayer;

  public override void Start()
  {
    base.Start();
    if (!RoundManager.Instance.hasInitializedLevelRandomSeed)
      RoundManager.Instance.InitializeRandomNumberGenerators();
    this.outsideNodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    this.ChoosePlayerToHaunt();
    this.EnableEnemyMesh(false, true);
    this.enemyMeshEnabled = false;
    Debug.Log((object) "DISABLING ENEMY MESH!!!!!!!!!!!");
    this.navHit = new NavMeshHit();
  }

  private void ChoosePlayerToHaunt()
  {
    ++this.timesChoosingAPlayer;
    if (this.timesChoosingAPlayer > 1)
      this.timer = this.hauntInterval - 1f;
    this.SFXVolumeLerpTo = 0.0f;
    this.creatureVoice.Stop();
    this.heartbeatMusic.volume = 0.0f;
    if (!this.initializedRandomSeed)
      this.ghostGirlRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 158);
    float num1 = 0.0f;
    float num2 = 0.0f;
    int num3 = 0;
    int num4 = 0;
    for (int index = 0; index < 4; ++index)
    {
      if (StartOfRound.Instance.gameStats.allPlayerStats[index].turnAmount > num3)
      {
        num3 = StartOfRound.Instance.gameStats.allPlayerStats[index].turnAmount;
        num4 = index;
      }
      if ((double) StartOfRound.Instance.allPlayerScripts[index].insanityLevel > (double) num1)
      {
        num1 = StartOfRound.Instance.allPlayerScripts[index].insanityLevel;
        num2 = (float) index;
      }
    }
    int[] weights = new int[4];
    for (int index = 0; index < 4; ++index)
    {
      if (!StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled)
      {
        weights[index] = 0;
      }
      else
      {
        weights[index] += 80;
        if ((double) num2 == (double) index && (double) num1 > 1.0)
          weights[index] += 50;
        if (num4 == index)
          weights[index] += 30;
        if (!StartOfRound.Instance.allPlayerScripts[index].hasBeenCriticallyInjured)
          weights[index] += 10;
        if ((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[index].currentlyHeldObjectServer != (UnityEngine.Object) null && StartOfRound.Instance.allPlayerScripts[index].currentlyHeldObjectServer.scrapValue > 150)
          weights[index] += 30;
      }
    }
    this.hauntingPlayer = StartOfRound.Instance.allPlayerScripts[RoundManager.Instance.GetRandomWeightedIndex(weights, this.ghostGirlRandom)];
    if (this.hauntingPlayer.isPlayerDead)
    {
      for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
      {
        if (!StartOfRound.Instance.allPlayerScripts[index].isPlayerDead)
        {
          this.hauntingPlayer = StartOfRound.Instance.allPlayerScripts[index];
          break;
        }
      }
    }
    Debug.Log((object) string.Format("Little girl: Haunting player with playerClientId: {0}; actualClientId: {1}", (object) this.hauntingPlayer.playerClientId, (object) this.hauntingPlayer.actualClientId));
    this.ChangeOwnershipOfEnemy(this.hauntingPlayer.actualClientId);
    this.hauntingLocalPlayer = (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) this.hauntingPlayer;
    if (this.switchHauntedPlayerCoroutine != null)
      this.StopCoroutine(this.switchHauntedPlayerCoroutine);
    this.switchHauntedPlayerCoroutine = this.StartCoroutine(this.setSwitchingHauntingPlayer());
  }

  private IEnumerator setSwitchingHauntingPlayer()
  {
    yield return (object) new WaitForSeconds(10f);
    this.switchedHauntingPlayer = false;
  }

  [ClientRpc]
  private void ChooseNewHauntingPlayerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(67448504U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 67448504U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.ChoosePlayerToHaunt();
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead)
      return;
    int num = StartOfRound.Instance.allPlayersDead ? 1 : 0;
  }

  public override void Update()
  {
    base.Update();
    if (this.IsServer && !this.hauntingPlayer.isPlayerControlled)
    {
      if (this.switchedHauntingPlayer)
        return;
      this.switchedHauntingPlayer = true;
      this.ChooseNewHauntingPlayerClientRpc();
    }
    else if (!this.IsOwner)
    {
      if (!this.enemyMeshEnabled)
        return;
      this.enemyMeshEnabled = false;
      this.EnableEnemyMesh(false, true);
    }
    else if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) this.hauntingPlayer)
    {
      this.ChangeOwnershipOfEnemy(this.hauntingPlayer.actualClientId);
    }
    else
    {
      if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
        return;
      this.creatureSFX.volume = Mathf.Lerp(this.creatureSFX.volume, this.SFXVolumeLerpTo, 5f * Time.deltaTime);
      if ((double) this.creatureSFX.volume <= 0.0099999997764825821 && (double) this.SFXVolumeLerpTo == 0.0 && this.creatureSFX.isPlaying)
        this.creatureSFX.Stop();
      switch (this.currentBehaviourStateIndex)
      {
        case 0:
          if (!this.staringInHaunt)
          {
            SoundManager.Instance.SetDiageticMixerSnapshot();
            this.heartbeatMusic.volume = Mathf.Lerp(this.heartbeatMusic.volume, 0.0f, 4f * Time.deltaTime);
            float num = this.hauntInterval;
            if (this.couldNotStareLastAttempt)
              num = 4f;
            if ((double) this.timer > (double) num)
            {
              this.timer = 0.0f;
              this.TryFindingHauntPosition();
              break;
            }
            this.timer += Time.deltaTime;
            break;
          }
          if (this.disappearingFromStare)
          {
            if (!this.choseDisappearingPosition)
            {
              this.choseDisappearingPosition = true;
              this.SetDestinationToPosition(this.FindPositionOutOfLOS());
              this.agent.speed = 5.25f;
              this.creatureAnimator.SetBool("Walk", true);
              this.creatureVoice.Stop();
              break;
            }
            if (this.disappearOnDelayCoroutine == null)
            {
              if (this.disappearByVanishing)
              {
                RoundManager.Instance.FlickerLights(true, true);
                this.MessWithLightsServerRpc();
                this.disappearOnDelayCoroutine = this.StartCoroutine(this.disappearOnDelay());
                break;
              }
              if (Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, this.transform.position + Vector3.up * 0.4f, StartOfRound.Instance.collidersAndRoomMask))
              {
                this.DisappearDuringHaunt();
                break;
              }
              if ((double) Vector3.Distance(this.transform.position, this.destination) < 0.20000000298023224 || (double) Vector3.Distance(this.transform.position, this.hauntingPlayer.transform.position) < 4.0)
              {
                this.disappearOnDelayCoroutine = this.StartCoroutine(this.disappearOnDelay());
                break;
              }
              break;
            }
            break;
          }
          this.turnCompass.LookAt(this.hauntingPlayer.transform);
          this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.turnCompass.eulerAngles.y, this.transform.eulerAngles.z);
          this.creatureAnimator.SetBool("Walk", false);
          if ((double) this.timer > (double) this.staringTimer)
          {
            this.timer = 0.0f;
            this.disappearingFromStare = true;
            break;
          }
          if (!Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, this.transform.position + Vector3.up * 0.4f, StartOfRound.Instance.collidersAndRoomMask))
          {
            if (this.hauntingPlayer.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.4f, 60f, 100, 5f))
            {
              SoundManager.Instance.SetDiageticMixerSnapshot(1);
              this.heartbeatMusic.volume = Mathf.Lerp(this.heartbeatMusic.volume, 1f, 3f * Time.deltaTime);
              this.timer += Time.deltaTime * 1.25f;
              if (!this.seenByPlayerThisTime)
              {
                this.seenByPlayerThisTime = true;
                ++this.timesSeenByPlayer;
                if (((double) this.timesSeenByPlayer >= (double) UnityEngine.Random.Range(3, 5) || this.timesStared - this.timesSeenByPlayer > 2) && UnityEngine.Random.Range(0, 100) < 85)
                  this.BeginChasing();
              }
            }
            else
            {
              SoundManager.Instance.SetDiageticMixerSnapshot();
              this.heartbeatMusic.volume = Mathf.Lerp(this.heartbeatMusic.volume, 0.0f, 3f * Time.deltaTime);
              this.timer += Time.deltaTime;
            }
            float num = Vector3.Distance(this.hauntingPlayer.gameplayCamera.transform.position, this.transform.position);
            if ((double) num < 7.0)
            {
              if (!this.playerApproachedThisTime && UnityEngine.Random.Range(0, 100) < 25 && this.timesSeenByPlayer <= 1)
                this.disappearingFromStare = true;
              else if ((double) num < 5.0)
              {
                if (UnityEngine.Random.Range(0, 100) > 35 && this.timesSeenByPlayer >= 2)
                {
                  this.BeginChasing();
                }
                else
                {
                  this.disappearingFromStare = true;
                  this.disappearByVanishing = true;
                }
              }
              this.playerApproachedThisTime = true;
              break;
            }
            break;
          }
          this.timer += Time.deltaTime * 3f;
          break;
        case 1:
          if ((double) this.chaseTimer <= 0.0 || (double) Vector3.Distance(this.transform.position, this.hauntingPlayer.transform.position) > 50.0)
            this.StopChasing();
          else
            this.chaseTimer -= Time.deltaTime;
          if ((double) this.timer >= 5.0)
          {
            this.TryTeleportingAroundPlayer();
            this.timer = 0.0f;
            break;
          }
          this.timer += Time.deltaTime;
          break;
      }
      if (this.isEnemyDead)
        return;
      int num1 = StartOfRound.Instance.allPlayersDead ? 1 : 0;
    }
  }

  [ServerRpc]
  private void MessWithLightsServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1320241094U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1320241094U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.MessWithLightsClientRpc();
  }

  [ClientRpc]
  private void MessWithLightsClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1481377371U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1481377371U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    RoundManager.Instance.FlickerLights(true, true);
    if (this.timesSeenByPlayer > 0)
      GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.9f);
    else
      GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.2f);
  }

  [ServerRpc]
  private void FlipLightsBreakerServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      if ((long) this.OwnerClientId != (long) networkManager.LocalClientId)
      {
        if (networkManager.LogLevel > LogLevel.Normal)
          return;
        Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
        return;
      }
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(164274866U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 164274866U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.MessWithLightsClientRpc();
  }

  [ClientRpc]
  private void FlipLightsBreakerClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(859211137U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 859211137U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    BreakerBox objectOfType = UnityEngine.Object.FindObjectOfType<BreakerBox>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
      return;
    objectOfType.SetSwitchesOff();
    RoundManager.Instance.TurnOnAllLights(false);
    GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.2f);
  }

  private void BeginChasing()
  {
    if (this.currentBehaviourStateIndex == 1)
      return;
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.staringInHaunt = false;
    this.disappearingFromStare = false;
    this.disappearByVanishing = false;
    this.choseDisappearingPosition = false;
    this.agent.speed = 5.25f;
    this.creatureAnimator.SetBool("Walk", true);
    ++this.timesChased;
    if (this.timesChased != 1 && UnityEngine.Random.Range(0, 100) < 65)
      this.FlipLightsBreakerServerRpc();
    else
      this.MessWithLightsServerRpc();
    this.chaseTimer = 20f;
    this.timer = 0.0f;
    this.SetMovingTowardsTargetPlayer(this.hauntingPlayer);
    this.moveTowardsDestination = true;
  }

  private void StopChasing()
  {
    this.SwitchToBehaviourStateOnLocalClient(0);
    this.creatureVoice.Stop();
    this.EnableEnemyMesh(false, true);
    this.SFXVolumeLerpTo = 0.0f;
    this.timer = 0.0f;
    this.creatureAnimator.SetBool("Walk", false);
    this.moveTowardsDestination = false;
  }

  private void TryTeleportingAroundPlayer()
  {
    if (this.hauntingPlayer.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.4f, 70f, 100, 10f))
      return;
    Vector3 pos = this.TryFindingHauntPosition(false, false);
    if (!(pos != Vector3.zero))
      return;
    this.creatureSFX.volume = 0.0f;
    this.agent.Warp(RoundManager.Instance.GetNavMeshPosition(pos, this.navHit));
  }

  private IEnumerator disappearOnDelay()
  {
    yield return (object) new WaitForSeconds(0.1f);
    this.DisappearDuringHaunt();
    this.disappearOnDelayCoroutine = (Coroutine) null;
  }

  private void DisappearDuringHaunt()
  {
    this.EnableEnemyMesh(false, true);
    this.disappearingFromStare = false;
    this.choseDisappearingPosition = false;
    this.disappearByVanishing = false;
    this.staringInHaunt = false;
    this.SFXVolumeLerpTo = 0.0f;
  }

  private Vector3 FindPositionOutOfLOS()
  {
    Vector3 direction = this.transform.right;
    float num = Vector3.Distance(this.transform.position, this.hauntingPlayer.transform.position);
    for (int index = 0; index < 8; ++index)
    {
      Debug.DrawRay(this.transform.position + Vector3.up * 0.4f, direction * 8f, Color.red, 1f);
      Ray ray = new Ray(this.transform.position + Vector3.up * 0.4f, direction);
      RaycastHit hitInfo;
      if (Physics.Raycast(ray, out hitInfo, 8f, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && (double) Vector3.Distance(hitInfo.point, this.hauntingPlayer.transform.position) - (double) num > -1.0 && Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, ray.GetPoint(hitInfo.distance - 0.1f), StartOfRound.Instance.collidersAndRoomMaskAndDefault))
      {
        Debug.DrawRay(this.transform.position + Vector3.up * 0.4f, direction * 8f, Color.green, 1f);
        Debug.Log((object) "Girl: Found hide position with raycast");
        return RoundManager.Instance.GetNavMeshPosition(hitInfo.point, this.navHit);
      }
      direction = Quaternion.Euler(0.0f, 45f, 0.0f) * direction;
    }
    for (int index = 0; index < this.allAINodes.Length; ++index)
    {
      if ((double) Vector3.Distance(this.allAINodes[index].transform.position, this.transform.position) < 7.0 && Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, this.allAINodes[index].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
      {
        Debug.Log((object) "Girl: Found hide position with AI nodes");
        Debug.DrawRay(this.allAINodes[index].transform.position, Vector3.up * 7f, Color.green, 1f);
        return RoundManager.Instance.GetNavMeshPosition(this.allAINodes[index].transform.position, this.navHit);
      }
    }
    Debug.Log((object) "Girl: Unable to find a location to hide away; vanishing instead");
    this.disappearByVanishing = true;
    return this.transform.position;
  }

  private Vector3 TryFindingHauntPosition(bool staringMode = true, bool mustBeInLOS = true)
  {
    if (this.hauntingPlayer.isInsideFactory)
    {
      for (int index = 0; index < this.allAINodes.Length; ++index)
      {
        if ((!mustBeInLOS || !Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, this.allAINodes[index].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) && !this.hauntingPlayer.HasLineOfSightToPosition(this.allAINodes[index].transform.position, 80f, 100, 8f))
        {
          Debug.DrawLine(this.hauntingPlayer.gameplayCamera.transform.position, this.allAINodes[index].transform.position, Color.green, 2f);
          Debug.Log((object) string.Format("Player distance to haunt position: {0}", (object) Vector3.Distance(this.hauntingPlayer.transform.position, this.allAINodes[index].transform.position)));
          if (staringMode)
            this.SetHauntStarePosition(this.allAINodes[index].transform.position);
          return this.allAINodes[index].transform.position;
        }
      }
    }
    else if (this.hauntingPlayer.isInElevator)
    {
      for (int index = 0; index < this.outsideNodes.Length; ++index)
      {
        if ((!mustBeInLOS || !Physics.Linecast(this.hauntingPlayer.gameplayCamera.transform.position, this.outsideNodes[index].transform.position, StartOfRound.Instance.collidersAndRoomMaskAndDefault)) && !this.hauntingPlayer.HasLineOfSightToPosition(this.outsideNodes[index].transform.position, 80f, 100, 8f))
        {
          if (staringMode)
            this.SetHauntStarePosition(this.outsideNodes[index].transform.position, 25f);
          return this.outsideNodes[index].transform.position;
        }
      }
    }
    this.couldNotStareLastAttempt = true;
    return Vector3.zero;
  }

  private void SetHauntStarePosition(Vector3 newPosition, float timeToStare = 15f)
  {
    this.couldNotStareLastAttempt = false;
    this.agent.Warp(RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(newPosition, 1f, this.navHit));
    this.moveTowardsDestination = false;
    this.destination = this.transform.position;
    this.agent.SetDestination(this.destination);
    this.agent.speed = 0.0f;
    this.EnableEnemyMesh(true, true);
    this.enemyMeshEnabled = true;
    Debug.Log((object) "Girl: STARTING HAUNT STARE");
    this.staringInHaunt = true;
    this.staringTimer = timeToStare;
    this.seenByPlayerThisTime = false;
    this.playerApproachedThisTime = false;
    ++this.timesStared;
    this.SFXVolumeLerpTo = 1f;
    this.creatureSFX.volume = 1f;
    if (UnityEngine.Random.Range(0, 100) < 85)
    {
      Debug.Log((object) "girL: Playing sound");
      RoundManager.PlayRandomClip(this.creatureVoice, this.appearStaringSFX);
    }
    this.creatureVoice.clip = this.breathingSFX;
    this.creatureVoice.Play();
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if (!this.hauntingLocalPlayer)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, overrideIsInsideFactoryCheck: true);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    Debug.Log((object) "Girl: collided with player");
    if ((UnityEngine.Object) playerControllerB == (UnityEngine.Object) this.hauntingPlayer)
    {
      if (this.staringInHaunt && this.currentBehaviourStateIndex == 0)
      {
        this.disappearByVanishing = true;
      }
      else
      {
        if (this.currentBehaviourStateIndex != 1)
          return;
        this.hauntingPlayer.KillPlayer(Vector3.zero, deathAnimation: 1);
        this.EnableEnemyMesh(false, true);
        this.creatureSFX.Stop();
      }
    }
    else
    {
      Debug.Log((object) "Girl: collided with player who cannot see it");
      if (!this.staringInHaunt || this.currentBehaviourStateIndex != 0)
        return;
      this.disappearByVanishing = true;
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_DressGirlAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(67448504U, new NetworkManager.RpcReceiveHandler(DressGirlAI.__rpc_handler_67448504)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1320241094U, new NetworkManager.RpcReceiveHandler(DressGirlAI.__rpc_handler_1320241094)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1481377371U, new NetworkManager.RpcReceiveHandler(DressGirlAI.__rpc_handler_1481377371)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(164274866U, new NetworkManager.RpcReceiveHandler(DressGirlAI.__rpc_handler_164274866)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(859211137U, new NetworkManager.RpcReceiveHandler(DressGirlAI.__rpc_handler_859211137)));
  }

  private static void __rpc_handler_67448504(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DressGirlAI) target).ChooseNewHauntingPlayerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1320241094(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((DressGirlAI) target).MessWithLightsServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1481377371(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DressGirlAI) target).MessWithLightsClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_164274866(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if ((long) rpcParams.Server.Receive.SenderClientId != (long) target.OwnerClientId)
    {
      if (networkManager.LogLevel > LogLevel.Normal)
        return;
      Debug.LogError((object) "Only the owner can invoke a ServerRpc that requires ownership!");
    }
    else
    {
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((DressGirlAI) target).FlipLightsBreakerServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_859211137(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DressGirlAI) target).FlipLightsBreakerClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (DressGirlAI);
}
