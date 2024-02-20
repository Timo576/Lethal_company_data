// Decompiled with JetBrains decompiler
// Type: MouthDogAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Animations.Rigging;

#nullable disable
public class MouthDogAI : EnemyAI, INoiseListener, IVisibleThreat
{
  public float noiseApproximation = 14f;
  public int suspicionLevel;
  private Vector3 previousPosition;
  public DampedTransform neckDampedTransform;
  private RoundManager roundManager;
  private float AITimer;
  private List<GameObject> allAINodesWithinRange = new List<GameObject>();
  private bool hasEnteredChaseModeFully;
  private bool startedChaseModeCoroutine;
  public AudioClip screamSFX;
  public AudioClip breathingSFX;
  public AudioClip killPlayerSFX;
  private float hearNoiseCooldown;
  private bool inLunge;
  private float lungeCooldown;
  private bool inKillAnimation;
  public Transform mouthGrip;
  public bool endingLunge;
  private Ray ray;
  private RaycastHit rayHit;
  private Vector3 lastHeardNoisePosition;
  private Vector3 noisePositionGuess;
  private float lastHeardNoiseDistanceWhenHeard;
  private bool heardOtherHowl;
  private DeadBodyInfo carryingBody;
  private System.Random enemyRandom;
  private Coroutine killPlayerCoroutine;
  private const int suspicionThreshold = 5;
  private const int alertThreshold = 9;
  private const int maxSuspicionLevel = 11;
  public AISearchRoutine roamPlanet;
  private Collider debugCollider;
  private float timeSinceHittingOtherEnemy;

  ThreatType IVisibleThreat.type => ThreatType.EyelessDog;

  int IVisibleThreat.GetThreatLevel(Vector3 seenByPosition)
  {
    int threatLevel = this.enemyHP >= 2 ? 5 : 3;
    if (this.creatureAnimator.GetBool("StartedChase"))
      threatLevel += 3;
    return threatLevel;
  }

  int IVisibleThreat.GetInterestLevel() => 0;

  Transform IVisibleThreat.GetThreatLookTransform() => this.eye;

  Transform IVisibleThreat.GetThreatTransform() => this.transform;

  Vector3 IVisibleThreat.GetThreatVelocity() => this.IsOwner ? this.agent.velocity : Vector3.zero;

  float IVisibleThreat.GetVisibility()
  {
    if (this.isEnemyDead)
      return 0.0f;
    return this.creatureAnimator.GetBool("StartedChase") ? 1f : 0.75f;
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    int livingPlayers = StartOfRound.Instance.livingPlayers;
  }

  public override void Start()
  {
    base.Start();
    this.roundManager = UnityEngine.Object.FindObjectOfType<RoundManager>();
    this.useSecondaryAudiosOnAnimatedObjects = true;
    if (UnityEngine.Random.Range(0, 10) < 2)
      this.creatureVoice.pitch = UnityEngine.Random.Range(0.6f, 1.3f);
    else
      this.creatureVoice.pitch = UnityEngine.Random.Range(0.9f, 1.1f);
    this.enemyRandom = new System.Random(StartOfRound.Instance.randomMapSeed + this.thisEnemyIndex);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead || !this.ventAnimationFinished)
      return;
    if ((double) this.stunNormalizedTimer > 0.0 && !this.isEnemyDead)
    {
      if ((UnityEngine.Object) this.stunnedByPlayer != (UnityEngine.Object) null && this.currentBehaviourStateIndex != 2 && this.IsOwner)
        this.EnrageDogOnLocalClient(this.stunnedByPlayer.transform.position, Vector3.Distance(this.transform.position, this.stunnedByPlayer.transform.position));
      this.creatureAnimator.SetLayerWeight(1, 1f);
    }
    else
      this.creatureAnimator.SetLayerWeight(1, 0.0f);
    this.hearNoiseCooldown -= Time.deltaTime;
    this.timeSinceHittingOtherEnemy += Time.deltaTime;
    this.creatureAnimator.SetFloat("speedMultiplier", Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f).sqrMagnitude / (Time.deltaTime / 4f));
    this.previousPosition = this.transform.position;
    if (this.currentBehaviourStateIndex == 2 || this.currentBehaviourStateIndex == 3)
    {
      if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 50f, 25, 10f))
        GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.4f, 0.5f);
    }
    else if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 50f, 30, 5f))
      GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.25f, 0.3f);
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.neckDampedTransform.weight = 1f;
        if (!this.IsOwner)
          break;
        this.agent.speed = 3.5f;
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        if (!this.IsOwner || this.roamPlanet.inProgress)
          break;
        this.StartSearch(this.transform.position, this.roamPlanet);
        break;
      case 1:
        if (this.hasEnteredChaseModeFully)
        {
          this.hasEnteredChaseModeFully = false;
          this.creatureVoice.Stop();
          this.startedChaseModeCoroutine = false;
          this.creatureAnimator.SetBool("StartedChase", false);
        }
        this.neckDampedTransform.weight = Mathf.Lerp(this.neckDampedTransform.weight, 1f, 8f * Time.deltaTime);
        if (!this.IsOwner)
          break;
        if (this.IsOwner && this.roamPlanet.inProgress)
          this.StopSearch(this.roamPlanet);
        this.agent.speed = 4.5f;
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        this.AITimer -= Time.deltaTime;
        if ((double) this.AITimer > 0.0)
          break;
        this.AITimer = 4f;
        --this.suspicionLevel;
        if (this.suspicionLevel > 1)
          break;
        this.SwitchToBehaviourState(0);
        break;
      case 2:
        if (!this.hasEnteredChaseModeFully)
        {
          if (this.startedChaseModeCoroutine)
            break;
          this.startedChaseModeCoroutine = true;
          this.StartCoroutine(this.enterChaseMode());
          break;
        }
        this.neckDampedTransform.weight = Mathf.Lerp(this.neckDampedTransform.weight, 0.2f, 8f * Time.deltaTime);
        if (!this.IsOwner)
          break;
        if (this.IsOwner && this.roamPlanet.inProgress)
          this.StopSearch(this.roamPlanet);
        if (!this.inLunge)
        {
          this.lungeCooldown -= Time.deltaTime;
          if ((double) Vector3.Distance(this.transform.position, this.noisePositionGuess) < 4.0 && (double) this.lungeCooldown <= 0.0)
          {
            this.inLunge = true;
            this.EnterLunge();
            break;
          }
        }
        this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime, 13f, 18f);
        if ((double) this.stunNormalizedTimer > 0.0)
          this.agent.speed = 0.0f;
        this.AITimer -= Time.deltaTime;
        if ((double) this.AITimer > 0.0)
          break;
        this.AITimer = 3f;
        --this.suspicionLevel;
        if ((double) Vector3.Distance(this.transform.position, this.agent.destination) < 3.0)
          this.SearchForPreviouslyHeardSound();
        if (this.suspicionLevel > 8)
          break;
        this.SwitchToBehaviourState(1);
        break;
      case 3:
        if (!this.IsOwner)
          break;
        this.agent.speed -= Time.deltaTime * 5f;
        if (this.endingLunge || (double) this.agent.speed >= 1.5 || this.inKillAnimation)
          break;
        this.endingLunge = true;
        this.lungeCooldown = 0.25f;
        this.EndLungeServerRpc();
        break;
    }
  }

  private void SearchForPreviouslyHeardSound()
  {
    int num = 0;
    Vector3 vector3;
    for (vector3 = this.transform.position; num < 5 && (double) Vector3.Distance(vector3, this.transform.position) < 4.0; vector3 = this.roundManager.GetRandomNavMeshPositionInRadius(this.lastHeardNoisePosition, this.lastHeardNoiseDistanceWhenHeard / this.noiseApproximation))
      ++num;
    this.SetDestinationToPosition(vector3);
    this.noisePositionGuess = vector3;
  }

  private IEnumerator enterChaseMode()
  {
    MouthDogAI mouthDogAi = this;
    if (mouthDogAi.IsOwner)
      mouthDogAi.agent.speed = 0.05f;
    mouthDogAi.DropCarriedBody();
    mouthDogAi.creatureVoice.PlayOneShot(mouthDogAi.screamSFX);
    if (!mouthDogAi.isEnemyDead)
      mouthDogAi.creatureAnimator.SetTrigger("ChaseHowl");
    if ((double) Vector3.Distance(mouthDogAi.transform.position, GameNetworkManager.Instance.localPlayerController.transform.position) < 16.0)
    {
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
      GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.5f);
    }
    yield return (object) new WaitForSeconds(0.5f);
    if (!mouthDogAi.heardOtherHowl)
      mouthDogAi.CallAllDogsWithHowl();
    mouthDogAi.heardOtherHowl = false;
    yield return (object) new WaitForSeconds(0.2f);
    mouthDogAi.creatureVoice.clip = mouthDogAi.breathingSFX;
    mouthDogAi.creatureVoice.Play();
    mouthDogAi.creatureAnimator.SetBool("StartedChase", true);
    mouthDogAi.hasEnteredChaseModeFully = true;
    mouthDogAi.creatureVoice.PlayOneShot(mouthDogAi.breathingSFX);
  }

  private void CallAllDogsWithHowl()
  {
    MouthDogAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<MouthDogAI>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (!((UnityEngine.Object) objectsOfType[index] == (UnityEngine.Object) this))
        objectsOfType[index].ReactToOtherDogHowl(this.transform.position);
    }
  }

  public void ReactToOtherDogHowl(Vector3 howlPosition)
  {
    this.heardOtherHowl = true;
    this.lastHeardNoiseDistanceWhenHeard = Vector3.Distance(this.transform.position, howlPosition);
    this.noisePositionGuess = this.roundManager.GetRandomNavMeshPositionInRadius(howlPosition, this.lastHeardNoiseDistanceWhenHeard / this.noiseApproximation);
    this.SetDestinationToPosition(this.noisePositionGuess);
    if (this.currentBehaviourStateIndex < 2)
      this.SwitchToBehaviourStateOnLocalClient(2);
    this.suspicionLevel = 8;
    this.lastHeardNoisePosition = howlPosition;
    Debug.Log((object) string.Format("Setting lastHeardNoisePosition to {0}", (object) howlPosition));
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesNoisePlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesNoisePlayedInOneSpot, noiseID);
    if ((double) this.stunNormalizedTimer > 0.0 || noiseID == 7 || noiseID == 546 || this.inKillAnimation || (double) this.hearNoiseCooldown >= 0.0 || timesNoisePlayedInOneSpot > 15)
      return;
    this.hearNoiseCooldown = 0.03f;
    float distanceToNoise = Vector3.Distance(this.transform.position, noisePosition);
    Debug.Log((object) string.Format("dog '{0}': Heard noise! Distance: {1} meters", (object) this.gameObject.name, (object) distanceToNoise));
    float num = 18f * noiseLoudness;
    if (Physics.Linecast(this.transform.position, noisePosition, 256))
    {
      noiseLoudness /= 2f;
      num /= 2f;
    }
    if ((double) noiseLoudness < 0.25)
      return;
    if (this.currentBehaviourStateIndex < 2 && (double) distanceToNoise < (double) num)
      this.suspicionLevel = 9;
    else
      ++this.suspicionLevel;
    bool fullyEnrage = false;
    if (this.suspicionLevel >= 9)
    {
      if (this.currentBehaviourStateIndex < 2)
        fullyEnrage = true;
    }
    else if (this.suspicionLevel >= 5 && this.currentBehaviourStateIndex == 0)
      fullyEnrage = false;
    this.AITimer = 3f;
    this.EnrageDogOnLocalClient(noisePosition, distanceToNoise, fullyEnrage: fullyEnrage);
  }

  private void EnrageDogOnLocalClient(
    Vector3 targetPosition,
    float distanceToNoise,
    bool approximatePosition = true,
    bool fullyEnrage = false)
  {
    Debug.Log((object) string.Format("Mouth dog targetPos 1: {0}; distanceToNoise: {1}", (object) targetPosition, (object) distanceToNoise));
    if (approximatePosition)
      targetPosition = this.roundManager.GetRandomNavMeshPositionInRadius(targetPosition, distanceToNoise / this.noiseApproximation);
    this.noisePositionGuess = targetPosition;
    Debug.Log((object) string.Format("Mouth dog targetPos 2: {0}", (object) targetPosition));
    if (fullyEnrage)
    {
      if (this.currentBehaviourStateIndex < 2)
      {
        this.SwitchToBehaviourState(2);
        this.hearNoiseCooldown = 1f;
        this.suspicionLevel = 12;
      }
      this.suspicionLevel = Mathf.Clamp(this.suspicionLevel, 0, 11);
    }
    else if (this.currentBehaviourStateIndex == 0)
      this.SwitchToBehaviourState(1);
    if (!this.IsOwner)
      this.ChangeOwnershipOfEnemy(NetworkManager.Singleton.LocalClientId);
    if (!this.inLunge)
      this.SetDestinationToPosition(this.noisePositionGuess);
    this.lastHeardNoiseDistanceWhenHeard = distanceToNoise;
    this.lastHeardNoisePosition = targetPosition;
    Debug.Log((object) string.Format("Dog lastheardnoisePosition: {0}", (object) this.lastHeardNoisePosition));
  }

  private void EnterLunge()
  {
    if (!this.IsOwner)
      this.ChangeOwnershipOfEnemy(NetworkManager.Singleton.LocalClientId);
    this.SwitchToBehaviourState(3);
    this.endingLunge = false;
    this.ray = new Ray(this.transform.position + Vector3.up, this.transform.forward);
    this.SetDestinationToPosition(this.roundManager.GetNavMeshPosition(!Physics.Raycast(this.ray, out this.rayHit, 17f, StartOfRound.Instance.collidersAndRoomMask) ? this.ray.GetPoint(17f) : this.rayHit.point));
    this.agent.speed = 13f;
  }

  [ServerRpc(RequireOwnership = false)]
  public void EndLungeServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(43708451U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 43708451U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EndLungeClientRpc();
  }

  [ClientRpc]
  public void EndLungeClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4130373844U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 4130373844U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(2);
    if (!this.isEnemyDead)
      this.creatureAnimator.SetTrigger("EndLungeNoKill");
    this.inLunge = false;
    Debug.Log((object) "Ending lunge");
  }

  private void ChaseLocalPlayer()
  {
    this.SwitchToBehaviourState(2);
    this.ChangeOwnershipOfEnemy(NetworkManager.Singleton.LocalClientId);
    this.SetDestinationToPosition(GameNetworkManager.Instance.localPlayerController.transform.position);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit, playHitSFX);
    this.enemyHP -= force;
    if (this.IsOwner)
    {
      if (this.enemyHP <= 0)
      {
        this.KillEnemyOnOwnerClient();
        return;
      }
      if (this.inKillAnimation)
        this.StopKillAnimationServerRpc();
    }
    if (!((UnityEngine.Object) playerWhoHit != (UnityEngine.Object) null) || this.currentBehaviourStateIndex == 2 || !this.IsOwner)
      return;
    this.EnrageDogOnLocalClient(playerWhoHit.transform.position, Vector3.Distance(this.transform.position, playerWhoHit.transform.position));
  }

  public override void OnCollideWithEnemy(Collider other, EnemyAI collidedEnemy = null)
  {
    base.OnCollideWithEnemy(other, collidedEnemy);
    if ((UnityEngine.Object) collidedEnemy.enemyType == (UnityEngine.Object) this.enemyType || (double) this.timeSinceHittingOtherEnemy < 1.0)
      return;
    if (this.currentBehaviourStateIndex == 2 && !this.inLunge)
    {
      this.transform.LookAt(other.transform.position);
      this.transform.localEulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
      this.inLunge = true;
      this.EnterLunge();
    }
    this.timeSinceHittingOtherEnemy = 0.0f;
    collidedEnemy.HitEnemy(2, playHitSFX: true);
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.inKillAnimation);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    RaycastHit hitInfo;
    if (Physics.Linecast(this.transform.position + Vector3.up + Vector3.Normalize((this.transform.position + Vector3.up - playerControllerB.gameplayCamera.transform.position) * 100f) * 0.5f, playerControllerB.gameplayCamera.transform.position, out hitInfo, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
    {
      if ((UnityEngine.Object) hitInfo.collider == (UnityEngine.Object) this.debugCollider)
        return;
      Debug.Log((object) ("Eyeless dog collide, linecast obstructed: " + hitInfo.collider.gameObject.name));
      this.debugCollider = hitInfo.collider;
    }
    else if (this.currentBehaviourStateIndex == 3)
    {
      playerControllerB.inAnimationWithEnemy = (EnemyAI) this;
      this.KillPlayerServerRpc((int) playerControllerB.playerClientId);
    }
    else if (this.currentBehaviourStateIndex == 0 || this.currentBehaviourStateIndex == 1)
    {
      this.ChaseLocalPlayer();
    }
    else
    {
      if (this.currentBehaviourStateIndex != 2 || this.inLunge)
        return;
      this.transform.LookAt(other.transform.position);
      this.transform.localEulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
      this.inLunge = true;
      this.EnterLunge();
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void KillPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(998670557U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 998670557U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (!this.inKillAnimation)
    {
      this.inKillAnimation = true;
      this.KillPlayerClientRpc(playerId);
    }
    else
      this.CancelKillAnimationWithPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void CancelKillAnimationWithPlayerClientRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2798326268U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 2798326268U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    StartOfRound.Instance.allPlayerScripts[playerObjectId].inAnimationWithEnemy = (EnemyAI) null;
  }

  [ClientRpc]
  public void KillPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2252497379U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 2252497379U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Kill player rpc");
    if (this.killPlayerCoroutine != null)
      this.StopCoroutine(this.killPlayerCoroutine);
    this.killPlayerCoroutine = this.StartCoroutine(this.KillPlayer(playerId));
  }

  private IEnumerator KillPlayer(int playerId)
  {
    MouthDogAI mouthDogAi = this;
    if (mouthDogAi.IsOwner)
      mouthDogAi.agent.speed = Mathf.Clamp(mouthDogAi.agent.speed, 2f, 0.0f);
    Debug.Log((object) "killing player A");
    mouthDogAi.creatureVoice.pitch = UnityEngine.Random.Range(0.96f, 1.04f);
    mouthDogAi.creatureVoice.PlayOneShot(mouthDogAi.killPlayerSFX, 1f);
    PlayerControllerB killPlayer = StartOfRound.Instance.allPlayerScripts[playerId];
    mouthDogAi.inKillAnimation = true;
    if (!mouthDogAi.isEnemyDead)
      mouthDogAi.creatureAnimator.SetTrigger("EndLungeKill");
    Debug.Log((object) "killing player B");
    if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController == (UnityEngine.Object) killPlayer)
      killPlayer.KillPlayer(Vector3.zero, causeOfDeath: CauseOfDeath.Mauling);
    float startTime = Time.timeSinceLevelLoad;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) killPlayer.deadBody != (UnityEngine.Object) null || (double) Time.timeSinceLevelLoad - (double) startTime > 2.0));
    if ((UnityEngine.Object) killPlayer.deadBody == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Giant dog: Player body was not spawned or found within 2 seconds.");
      killPlayer.inAnimationWithEnemy = (EnemyAI) null;
      mouthDogAi.inKillAnimation = false;
    }
    else
    {
      mouthDogAi.TakeBodyInMouth(killPlayer.deadBody);
      startTime = Time.timeSinceLevelLoad;
      Quaternion rotateTo = Quaternion.Euler(new Vector3(0.0f, RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(mouthDogAi.transform.position + Vector3.up * 0.6f), 0.0f));
      Quaternion rotateFrom = mouthDogAi.transform.rotation;
      while ((double) Time.timeSinceLevelLoad - (double) startTime < 2.0)
      {
        yield return (object) null;
        if (mouthDogAi.IsOwner)
          mouthDogAi.transform.rotation = Quaternion.RotateTowards(rotateFrom, rotateTo, 60f * Time.deltaTime);
      }
      yield return (object) new WaitForSeconds(3.01f);
      mouthDogAi.DropCarriedBody();
      mouthDogAi.suspicionLevel = 2;
      mouthDogAi.SwitchToBehaviourStateOnLocalClient(2);
      mouthDogAi.endingLunge = true;
      mouthDogAi.inKillAnimation = false;
    }
  }

  private void StopKillAnimation()
  {
    if (this.killPlayerCoroutine != null)
      this.StopCoroutine(this.killPlayerCoroutine);
    this.creatureVoice.Stop();
    this.DropCarriedBody();
    this.suspicionLevel = 2;
    this.SwitchToBehaviourStateOnLocalClient(2);
    this.endingLunge = true;
    this.inKillAnimation = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void StopKillAnimationServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(19183128U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 19183128U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopKillAnimationClientRpc();
  }

  [ClientRpc]
  public void StopKillAnimationClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4189041149U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 4189041149U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.StopKillAnimation();
  }

  private void TakeBodyInMouth(DeadBodyInfo body)
  {
    this.carryingBody = body;
    this.carryingBody.attachedTo = this.mouthGrip;
    this.carryingBody.attachedLimb = body.bodyParts[5];
    this.carryingBody.matchPositionExactly = true;
  }

  private void DropCarriedBody()
  {
    if ((UnityEngine.Object) this.carryingBody == (UnityEngine.Object) null)
      return;
    this.carryingBody.speedMultiplier = 12f;
    this.carryingBody.attachedTo = (Transform) null;
    this.carryingBody.attachedLimb = (Rigidbody) null;
    this.carryingBody.matchPositionExactly = false;
    this.carryingBody = (DeadBodyInfo) null;
  }

  public override void KillEnemy(bool destroy = false)
  {
    this.StopKillAnimation();
    this.creatureVoice.Stop();
    this.creatureSFX.Stop();
    base.KillEnemy(destroy);
  }

  public override void EnableEnemyMesh(bool enable, bool overrideDoNotSet = false)
  {
    base.EnableEnemyMesh(enable);
    foreach (ParticleSystem componentsInChild in this.gameObject.GetComponentsInChildren<ParticleSystem>())
      componentsInChild.main.playOnAwake = this.enabled;
  }

  public override void OnDrawGizmos()
  {
    base.OnDrawGizmos();
    if (!this.debugEnemyAI)
      return;
    Gizmos.DrawCube(this.noisePositionGuess, Vector3.one);
    Gizmos.DrawLine(this.noisePositionGuess, this.transform.position + Vector3.up);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_MouthDogAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(43708451U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_43708451)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4130373844U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_4130373844)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(998670557U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_998670557)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2798326268U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_2798326268)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2252497379U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_2252497379)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(19183128U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_19183128)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4189041149U, new NetworkManager.RpcReceiveHandler(MouthDogAI.__rpc_handler_4189041149)));
  }

  private static void __rpc_handler_43708451(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((MouthDogAI) target).EndLungeServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4130373844(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MouthDogAI) target).EndLungeClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_998670557(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((MouthDogAI) target).KillPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2798326268(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MouthDogAI) target).CancelKillAnimationWithPlayerClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2252497379(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MouthDogAI) target).KillPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_19183128(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((MouthDogAI) target).StopKillAnimationServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_4189041149(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((MouthDogAI) target).StopKillAnimationClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (MouthDogAI);
}
