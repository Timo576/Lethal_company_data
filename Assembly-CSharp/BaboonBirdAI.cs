// Decompiled with JetBrains decompiler
// Type: BaboonBirdAI
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
public class BaboonBirdAI : EnemyAI
{
  public Dictionary<Transform, Threat> threats = new Dictionary<Transform, Threat>();
  public Transform focusedThreatTransform;
  public Threat focusedThreat;
  public bool focusingOnThreat;
  public bool focusedThreatIsInView;
  private int focusLevel;
  private float fearLevel;
  private float fearLevelNoDistComparison;
  private Vector3 agentLocalVelocity;
  private float velX;
  private float velZ;
  private Vector3 previousPosition;
  public Transform animationContainer;
  public MultiAimConstraint headLookRig;
  public Transform headLookTarget;
  private Ray lookRay;
  public float fov;
  public float visionDistance;
  private int visibleThreatsMask = 524296;
  private int scrapMask = 64;
  private int leadershipLevel;
  private int previousBehaviourState = -1;
  public BaboonHawkGroup scoutingGroup;
  private float miscAnimationTimer;
  private int currentMiscAnimation;
  private Vector3 lookTarget;
  private Vector3 peekTarget;
  private float peekTimer;
  public AISearchRoutine scoutingSearchRoutine;
  public static Vector3 baboonCampPosition;
  public float scoutTimer;
  public float timeToScout;
  private float timeSinceRestWhileScouting;
  private float restingDuringScouting;
  private bool eyesClosed;
  private bool restingAtCamp;
  private float restAtCampTimer;
  private float chosenDistanceToCamp = 1f;
  private float timeSincePingingBirdInterest;
  private float timeSinceLastMiscAnimation;
  private int aggressiveMode;
  private int previousAggressiveMode;
  private float fightTimer;
  public AudioSource aggressionAudio;
  private Vector3 debugSphere;
  public Collider ownCollider;
  private float timeSinceAggressiveDisplay;
  private float timeSpentFocusingOnThreat;
  private float timeSinceFighting;
  private bool doingKillAnimation;
  private Coroutine killAnimCoroutine;
  private float timeSinceHitting;
  public Transform deadBodyPoint;
  public AudioClip[] cawScreamSFX;
  public AudioClip[] cawLaughSFX;
  private float noiseTimer;
  private float noiseInterval;
  public GrabbableObject focusedScrap;
  public GrabbableObject heldScrap;
  public bool movingToScrap;
  public Transform grabTarget;
  public TwoBoneIKConstraint leftArmRig;
  public TwoBoneIKConstraint rightArmRig;
  private bool oddAIInterval;
  private DeadBodyInfo killAnimationBody;
  private float timeSinceBeingAttackedByPlayer;
  private float timeSinceJoiningOrLeavingScoutingGroup;
  private BaboonBirdAI biggestBaboon;

  public override void Start()
  {
    base.Start();
    if (!this.IsOwner)
      return;
    System.Random randomSeed = new System.Random(StartOfRound.Instance.randomMapSeed + this.thisEnemyIndex);
    this.leadershipLevel = randomSeed.Next(0, 500);
    if (BaboonBirdAI.baboonCampPosition == Vector3.zero)
    {
      List<GameObject> gameObjectList = new List<GameObject>();
      for (int index = 0; index < RoundManager.Instance.outsideAINodes.Length - 2; index += 2)
      {
        if ((double) Vector3.Distance(RoundManager.Instance.outsideAINodes[index].transform.position, StartOfRound.Instance.elevatorTransform.position) > 30.0 && !this.PathIsIntersectedByLineOfSight(RoundManager.Instance.outsideAINodes[index].transform.position, avoidLineOfSight: false))
          gameObjectList.Add(RoundManager.Instance.outsideAINodes[index]);
      }
      BaboonBirdAI.baboonCampPosition = gameObjectList.Count != 0 ? RoundManager.Instance.GetRandomNavMeshPositionInBoxPredictable(gameObjectList[randomSeed.Next(0, gameObjectList.Count)].transform.position, 15f, RoundManager.Instance.navHit, randomSeed) : this.transform.position;
    }
    this.SyncInitialValuesServerRpc(this.leadershipLevel, BaboonBirdAI.baboonCampPosition);
  }

  [ServerRpc]
  public void SyncInitialValuesServerRpc(int syncLeadershipLevel, Vector3 campPosition)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3452382367U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, syncLeadershipLevel);
      bufferWriter.WriteValueSafe(in campPosition);
      this.__endSendServerRpc(ref bufferWriter, 3452382367U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SyncInitialValuesClientRpc(syncLeadershipLevel, campPosition);
  }

  [ClientRpc]
  public void SyncInitialValuesClientRpc(int syncLeadershipLevel, Vector3 campPosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3856685904U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, syncLeadershipLevel);
      bufferWriter.WriteValueSafe(in campPosition);
      this.__endSendClientRpc(ref bufferWriter, 3856685904U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.leadershipLevel = syncLeadershipLevel;
    BaboonBirdAI.baboonCampPosition = campPosition;
    this.transform.localScale = this.transform.localScale * Mathf.Max((float) ((double) this.leadershipLevel / 200.0 * 0.60000002384185791), 0.9f);
  }

  public void LateUpdate()
  {
    if (!this.inSpecialAnimation && ((UnityEngine.Object) this.focusedThreatTransform == (UnityEngine.Object) null || this.currentBehaviourStateIndex != 2) && (double) this.peekTimer < 0.0 || this.isEnemyDead)
    {
      this.agent.angularSpeed = 300f;
      this.headLookRig.weight = Mathf.Lerp(this.headLookRig.weight, 0.0f, Time.deltaTime * 10f);
    }
    else
    {
      this.agent.angularSpeed = 0.0f;
      this.headLookRig.weight = Mathf.Lerp(this.headLookRig.weight, 1f, Time.deltaTime * 10f);
      if ((double) this.peekTimer >= 0.0)
      {
        this.peekTimer -= Time.deltaTime;
        this.AnimateLooking(this.peekTarget);
      }
      else
        this.AnimateLooking(this.lookTarget);
    }
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.timeSinceHitting < 0.5 || Physics.Linecast(this.transform.position + Vector3.up * 0.7f + Vector3.Normalize(this.transform.position + Vector3.up * 0.7f - (other.transform.position + Vector3.up * 0.4f)) * 0.5f, other.transform.position + Vector3.up * 0.4f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other, this.inSpecialAnimation || this.doingKillAnimation);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.timeSinceHitting = 0.0f;
    playerControllerB.DamagePlayer(30);
    if (playerControllerB.isPlayerDead)
    {
      this.StabPlayerDeathAnimServerRpc((int) playerControllerB.playerClientId);
    }
    else
    {
      this.creatureAnimator.ResetTrigger("Hit");
      this.creatureAnimator.SetTrigger("Hit");
      this.creatureSFX.PlayOneShot(this.enemyType.audioClips[5]);
      WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.audioClips[5]);
      RoundManager.Instance.PlayAudibleNoise(this.creatureSFX.transform.position, 8f, 0.7f);
    }
  }

  public override void OnCollideWithEnemy(Collider other, EnemyAI enemyScript = null)
  {
    base.OnCollideWithEnemy(other);
    if ((UnityEngine.Object) enemyScript.enemyType == (UnityEngine.Object) this.enemyType || (double) this.timeSinceHitting < 0.75 || !this.IsOwner || !enemyScript.enemyType.canDie)
      return;
    this.timeSinceHitting = 0.0f;
    this.creatureAnimator.ResetTrigger("Hit");
    this.creatureAnimator.SetTrigger("Hit");
    this.creatureSFX.PlayOneShot(this.enemyType.audioClips[5]);
    WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.audioClips[5]);
    RoundManager.Instance.PlayAudibleNoise(this.creatureSFX.transform.position, 8f, 0.7f);
    enemyScript.HitEnemy(playHitSFX: true);
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit, playHitSFX);
    if (this.isEnemyDead)
      return;
    this.creatureAnimator.SetTrigger("TakeDamage");
    if ((UnityEngine.Object) playerWhoHit != (UnityEngine.Object) null)
    {
      this.timeSinceBeingAttackedByPlayer = 0.0f;
      Threat threat;
      if (this.threats.TryGetValue(playerWhoHit.transform, out threat))
      {
        threat.hasAttacked = true;
        this.fightTimer = 7f;
      }
    }
    this.enemyHP -= force;
    if (this.IsOwner && this.enemyHP <= 0 && !this.isEnemyDead)
      this.KillEnemyOnOwnerClient();
    this.StopKillAnimation();
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    this.creatureAnimator.SetBool("IsDead", true);
    if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null && this.IsOwner)
      this.DropHeldItemAndSync();
    this.StopKillAnimation();
  }

  public void StopKillAnimation()
  {
    if (this.killAnimCoroutine != null)
      this.StopCoroutine(this.killAnimCoroutine);
    this.agent.acceleration = 17f;
    this.inSpecialAnimation = false;
    this.doingKillAnimation = false;
    if (!((UnityEngine.Object) this.killAnimationBody != (UnityEngine.Object) null))
      return;
    this.killAnimationBody.attachedLimb = (Rigidbody) null;
    this.killAnimationBody.attachedTo = (Transform) null;
    this.killAnimationBody = (DeadBodyInfo) null;
  }

  [ServerRpc(RequireOwnership = false)]
  public void StabPlayerDeathAnimServerRpc(int playerObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2476579270U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObject);
      this.__endSendServerRpc(ref bufferWriter, 2476579270U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.doingKillAnimation)
      return;
    if (this.IsOwner && (UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
      this.DropHeldItemAndSync();
    this.doingKillAnimation = true;
    this.StabPlayerDeathAnimClientRpc(playerObject);
  }

  [ClientRpc]
  public void StabPlayerDeathAnimClientRpc(int playerObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3749667856U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObject);
      this.__endSendClientRpc(ref bufferWriter, 3749667856U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.doingKillAnimation = true;
    this.inSpecialAnimation = true;
    this.agent.acceleration = 70f;
    this.agent.speed = 0.0f;
    if (this.killAnimCoroutine != null)
      this.StopCoroutine(this.killAnimCoroutine);
    this.killAnimCoroutine = this.StartCoroutine(this.killPlayerAnimation(playerObject));
  }

  private IEnumerator killPlayerAnimation(int playerObject)
  {
    BaboonBirdAI baboonBirdAi = this;
    PlayerControllerB killedPlayer = StartOfRound.Instance.allPlayerScripts[playerObject];
    baboonBirdAi.creatureAnimator.ResetTrigger("KillAnimation");
    baboonBirdAi.creatureAnimator.SetTrigger("KillAnimation");
    baboonBirdAi.creatureVoice.PlayOneShot(baboonBirdAi.enemyType.audioClips[4]);
    WalkieTalkie.TransmitOneShotAudio(baboonBirdAi.creatureVoice, baboonBirdAi.enemyType.audioClips[4]);
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (double) Time.realtimeSinceStartup - (double) startTime > 1.0 || (UnityEngine.Object) killedPlayer.deadBody != (UnityEngine.Object) null));
    if ((UnityEngine.Object) killedPlayer.deadBody != (UnityEngine.Object) null)
    {
      baboonBirdAi.killAnimationBody = killedPlayer.deadBody;
      baboonBirdAi.killAnimationBody.attachedLimb = killedPlayer.deadBody.bodyParts[5];
      baboonBirdAi.killAnimationBody.attachedTo = baboonBirdAi.deadBodyPoint;
      baboonBirdAi.killAnimationBody.matchPositionExactly = true;
      baboonBirdAi.killAnimationBody.canBeGrabbedBackByPlayers = false;
      yield return (object) null;
      yield return (object) new WaitForSeconds(1.7f);
      baboonBirdAi.killAnimationBody.attachedLimb = (Rigidbody) null;
      baboonBirdAi.killAnimationBody.attachedTo = (Transform) null;
    }
    baboonBirdAi.agent.acceleration = 17f;
    baboonBirdAi.inSpecialAnimation = false;
    baboonBirdAi.doingKillAnimation = false;
  }

  private void InteractWithScrap()
  {
    if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
    {
      this.focusedScrap = (GrabbableObject) null;
      if ((double) Vector3.Distance(this.transform.position, BaboonBirdAI.baboonCampPosition) >= (double) UnityEngine.Random.Range(1f, 7f) && !this.heldScrap.isHeld)
        return;
      this.DropHeldItemAndSync();
    }
    else
    {
      if (!((UnityEngine.Object) this.focusedScrap != (UnityEngine.Object) null))
        return;
      if (this.debugEnemyAI)
        Debug.DrawRay(this.focusedScrap.transform.position, Vector3.up * 3f, Color.yellow);
      if (!this.CanGrabScrap(this.focusedScrap))
      {
        this.focusedScrap = (GrabbableObject) null;
      }
      else
      {
        if ((double) Vector3.Distance(this.transform.position, this.focusedScrap.transform.position) >= 0.40000000596046448 || Physics.Linecast(this.transform.position, this.focusedScrap.transform.position + Vector3.up * 0.5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
          return;
        this.GrabItemAndSync(this.focusedScrap.NetworkObject);
      }
    }
  }

  private bool CanGrabScrap(GrabbableObject scrap)
  {
    return scrap.itemProperties.itemId != 1531 && (!scrap.isInShipRoom || this.isInsidePlayerShip) && !this.isEnemyDead && !scrap.heldByPlayerOnServer && !scrap.isHeld && ((UnityEngine.Object) scrap == (UnityEngine.Object) this.heldScrap || !scrap.isHeldByEnemy) && (double) Vector3.Distance(scrap.transform.position, BaboonBirdAI.baboonCampPosition) > 8.0;
  }

  private void DropHeldItemAndSync()
  {
    if ((UnityEngine.Object) this.heldScrap == (UnityEngine.Object) null)
      Debug.LogError((object) string.Format("Baboon #{0} Error: DropItemAndSync called when baboon has no scrap!", (object) this.thisEnemyIndex));
    NetworkObject networkObject = this.heldScrap.NetworkObject;
    if ((UnityEngine.Object) networkObject == (UnityEngine.Object) null)
      Debug.LogError((object) string.Format("Baboon #{0} Error: No network object in held scrap {1}", (object) this.thisEnemyIndex, (object) this.heldScrap.gameObject.name));
    Vector3 itemFloorPosition = this.heldScrap.GetItemFloorPosition();
    this.DropScrap(networkObject, itemFloorPosition);
    this.DropScrapServerRpc((NetworkObjectReference) networkObject, itemFloorPosition, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  [ServerRpc]
  public void DropScrapServerRpc(
    NetworkObjectReference item,
    Vector3 targetFloorPosition,
    int clientWhoSentRPC)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1418775270U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in item, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe(in targetFloorPosition);
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSentRPC);
      this.__endSendServerRpc(ref bufferWriter, 1418775270U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DropScrapClientRpc(item, targetFloorPosition, clientWhoSentRPC);
  }

  [ClientRpc]
  public void DropScrapClientRpc(
    NetworkObjectReference item,
    Vector3 targetFloorPosition,
    int clientWhoSentRPC)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1865475504U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in item, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe(in targetFloorPosition);
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSentRPC);
      this.__endSendClientRpc(ref bufferWriter, 1865475504U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || clientWhoSentRPC == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    NetworkObject networkObject;
    if (item.TryGet(out networkObject))
      this.DropScrap(networkObject, targetFloorPosition);
    else
      Debug.LogError((object) string.Format("Baboon #{0}; Error, was not able to get network object from dropped item client rpc", (object) this.thisEnemyIndex));
  }

  private void DropScrap(NetworkObject item, Vector3 targetFloorPosition)
  {
    if ((UnityEngine.Object) this.heldScrap == (UnityEngine.Object) null)
      Debug.LogError((object) "Baboon: my held item is null when attempting to drop it!!");
    else if (this.heldScrap.isHeld)
    {
      this.heldScrap.DiscardItemFromEnemy();
      this.heldScrap.isHeldByEnemy = false;
      this.heldScrap = (GrabbableObject) null;
      Debug.Log((object) string.Format("Baboon #{0}: Dropped item which was held by a player", (object) this.thisEnemyIndex));
    }
    else
    {
      this.heldScrap.parentObject = (Transform) null;
      this.heldScrap.transform.SetParent(StartOfRound.Instance.propsContainer, true);
      this.heldScrap.EnablePhysics(true);
      this.heldScrap.fallTime = 0.0f;
      this.heldScrap.startFallingPosition = this.heldScrap.transform.parent.InverseTransformPoint(this.heldScrap.transform.position);
      this.heldScrap.targetFloorPosition = this.heldScrap.transform.parent.InverseTransformPoint(targetFloorPosition);
      this.heldScrap.floorYRot = -1;
      this.heldScrap.DiscardItemFromEnemy();
      this.heldScrap.isHeldByEnemy = false;
      this.heldScrap = (GrabbableObject) null;
      Debug.Log((object) string.Format("Baboon #{0}: Dropped item", (object) this.thisEnemyIndex));
    }
  }

  private void GrabItemAndSync(NetworkObject item)
  {
    if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
      Debug.LogError((object) string.Format("Baboon #{0} Error: GrabItemAndSync called when baboon is already carrying scrap!", (object) this.thisEnemyIndex));
    this.GrabScrap(item);
    this.GrabScrapServerRpc((NetworkObjectReference) item, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
  }

  [ServerRpc]
  public void GrabScrapServerRpc(NetworkObjectReference item, int clientWhoSentRPC)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(869682226U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in item, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSentRPC);
      this.__endSendServerRpc(ref bufferWriter, 869682226U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (!item.TryGet(out networkObject))
    {
      Debug.LogError((object) string.Format("Baboon #{0} error: Could not get grabbed network object from reference on server", (object) this.thisEnemyIndex));
    }
    else
    {
      if (!(bool) (UnityEngine.Object) networkObject.GetComponent<GrabbableObject>() || networkObject.GetComponent<GrabbableObject>().heldByPlayerOnServer)
        return;
      this.GrabScrapClientRpc(item, clientWhoSentRPC);
    }
  }

  [ClientRpc]
  public void GrabScrapClientRpc(NetworkObjectReference item, int clientWhoSentRPC)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1564051222U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in item, new FastBufferWriter.ForNetworkSerializable());
      BytePacker.WriteValueBitPacked(bufferWriter, clientWhoSentRPC);
      this.__endSendClientRpc(ref bufferWriter, 1564051222U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || clientWhoSentRPC == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    NetworkObject networkObject;
    if (item.TryGet(out networkObject))
      this.GrabScrap(networkObject);
    else
      Debug.LogError((object) string.Format("Baboon #{0}; Error, was not able to get id from grabbed item client rpc", (object) this.thisEnemyIndex));
  }

  private void GrabScrap(NetworkObject item)
  {
    if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
    {
      Debug.Log((object) string.Format("Baboon #{0}: Trying to grab another item ({1}) while hands are already full with item ({2}). Dropping the currently held one.", (object) this.thisEnemyIndex, (object) item.gameObject.name, (object) this.heldScrap.gameObject.name));
      this.DropScrap(this.heldScrap.GetComponent<NetworkObject>(), this.heldScrap.GetItemFloorPosition());
    }
    GrabbableObject component = item.gameObject.GetComponent<GrabbableObject>();
    this.heldScrap = component;
    component.parentObject = this.grabTarget;
    component.hasHitGround = false;
    component.GrabItemFromEnemy((EnemyAI) this);
    component.isHeldByEnemy = true;
    component.EnablePhysics(false);
    Debug.Log((object) string.Format("Baboon #{0}: Grabbing item!!! {1}", (object) this.thisEnemyIndex, (object) this.heldScrap.gameObject.name));
  }

  public override void ReachedNodeInSearch()
  {
    base.ReachedNodeInSearch();
    if (this.currentSearch.nodesEliminatedInCurrentSearch <= 14 || (double) this.timeSinceRestWhileScouting <= 17.0 || (double) this.timeSinceAggressiveDisplay <= 6.0)
      return;
    this.timeSinceRestWhileScouting = 0.0f;
    this.restingDuringScouting = 12f;
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead)
    {
      this.agent.speed = 0.0f;
      if (!this.scoutingSearchRoutine.inProgress)
        return;
      this.StopSearch(this.scoutingSearchRoutine, false);
    }
    else
    {
      if ((double) this.stunNormalizedTimer > 0.0 || (double) this.miscAnimationTimer > 0.0)
      {
        this.agent.speed = 0.0f;
        if (this.doingKillAnimation && (double) this.stunNormalizedTimer >= 0.0)
          this.StopKillAnimation();
        if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null && this.IsOwner)
          this.DropHeldItemAndSync();
        if ((UnityEngine.Object) this.stunnedByPlayer != (UnityEngine.Object) null)
          this.PingBaboonInterest(this.stunnedByPlayer.gameplayCamera.transform.position, 4);
      }
      if (this.inSpecialAnimation)
      {
        this.agent.speed = 0.0f;
      }
      else
      {
        if (!this.eyesClosed)
          this.DoLOSCheck();
        this.InteractWithScrap();
        switch (this.currentBehaviourStateIndex)
        {
          case 0:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              this.timeToScout = (float) UnityEngine.Random.Range(25, 70);
              this.scoutTimer = 0.0f;
              this.restingAtCamp = false;
              this.restAtCampTimer = 0.0f;
              this.SetAggressiveMode(0);
              this.previousBehaviourState = this.currentBehaviourStateIndex;
            }
            if (!this.IsOwner)
              break;
            if ((UnityEngine.Object) this.focusedScrap != (UnityEngine.Object) null)
              this.SetDestinationToPosition(this.focusedScrap.transform.position);
            if (this.scoutingGroup == null || (UnityEngine.Object) this.scoutingGroup.leader == (UnityEngine.Object) this || !this.scoutingGroup.members.Contains(this))
            {
              BaboonHawkGroup scoutingGroup = this.scoutingGroup;
              if ((double) this.restingDuringScouting >= 0.0)
              {
                if (this.scoutingSearchRoutine.inProgress)
                  this.StopSearch(this.scoutingSearchRoutine, false);
                if (!this.creatureAnimator.GetBool("sit"))
                  this.EnemyEnterRestModeServerRpc(false, false);
                this.creatureAnimator.SetBool("sit", true);
                this.restingDuringScouting -= this.AIIntervalTime;
                this.agent.speed = 0.0f;
              }
              else
              {
                if (!this.scoutingSearchRoutine.inProgress && (UnityEngine.Object) this.focusedScrap == (UnityEngine.Object) null)
                  this.StartSearch(BaboonBirdAI.baboonCampPosition, this.scoutingSearchRoutine);
                if (this.creatureAnimator.GetBool("sit"))
                {
                  this.EnemyGetUpServerRpc();
                  this.creatureAnimator.SetBool("sit", false);
                }
                this.agent.speed = 10f;
              }
            }
            else
            {
              if (this.scoutingSearchRoutine.inProgress)
                this.StopSearch(this.scoutingSearchRoutine);
              if (this.creatureAnimator.GetBool("sit"))
              {
                this.EnemyGetUpServerRpc();
                this.creatureAnimator.SetBool("sit", false);
              }
              this.agent.speed = 12f;
              if ((double) Vector3.Distance(this.transform.position, this.scoutingGroup.leader.transform.position) > 60.0 || this.PathIsIntersectedByLineOfSight(this.scoutingGroup.leader.transform.position, avoidLineOfSight: false))
                this.LeaveCurrentScoutingGroup(true);
              else if ((double) Vector3.Distance(this.destination, this.scoutingGroup.leader.transform.position) > 8.0 && (UnityEngine.Object) this.focusedScrap == (UnityEngine.Object) null)
                this.SetDestinationToPosition(RoundManager.Instance.GetRandomNavMeshPositionInRadiusSpherical(this.scoutingGroup.leader.transform.position, 6f, RoundManager.Instance.navHit));
            }
            if ((double) this.scoutTimer < (double) this.timeToScout && (UnityEngine.Object) this.heldScrap == (UnityEngine.Object) null)
            {
              this.scoutTimer += this.AIIntervalTime;
              break;
            }
            this.SwitchToBehaviourState(1);
            break;
          case 1:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              this.restingDuringScouting = 0.0f;
              this.scoutTimer = 0.0f;
              this.chosenDistanceToCamp = UnityEngine.Random.Range(1f, 7f);
              this.LeaveCurrentScoutingGroup(true);
              this.SetAggressiveMode(0);
              this.previousBehaviourState = this.currentBehaviourStateIndex;
            }
            if (this.scoutingSearchRoutine.inProgress)
              this.StopSearch(this.scoutingSearchRoutine);
            if ((UnityEngine.Object) this.focusedScrap != (UnityEngine.Object) null)
              this.SetDestinationToPosition(this.focusedScrap.transform.position);
            else
              this.SetDestinationToPosition(BaboonBirdAI.baboonCampPosition);
            if ((double) Vector3.Distance(this.transform.position, BaboonBirdAI.baboonCampPosition) < (double) this.chosenDistanceToCamp && (double) this.peekTimer < 0.0)
            {
              if (!this.restingAtCamp)
              {
                this.restingAtCamp = true;
                this.restAtCampTimer = UnityEngine.Random.Range(15f, 30f);
                if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
                  this.DropHeldItemAndSync();
                bool sleep = false;
                if (UnityEngine.Random.Range(0, 100) < 35)
                  sleep = true;
                this.EnemyEnterRestModeServerRpc(sleep, true);
              }
              else if ((double) this.restAtCampTimer <= 0.0)
                this.SwitchToBehaviourState(0);
              else
                this.restAtCampTimer -= this.AIIntervalTime;
              this.agent.speed = 0.0f;
              break;
            }
            if (this.restingAtCamp)
            {
              this.restingAtCamp = false;
              this.EnemyGetUpServerRpc();
            }
            this.creatureAnimator.SetBool("sit", false);
            this.creatureAnimator.SetBool("sleep", false);
            this.agent.speed = 9f;
            break;
          case 2:
            if (this.previousBehaviourState != this.currentBehaviourStateIndex)
            {
              this.timeSpentFocusingOnThreat = 0.0f;
              this.creatureAnimator.SetBool("sleep", false);
              this.creatureAnimator.SetBool("sit", false);
              this.EnemyGetUpServerRpc();
              this.previousBehaviourState = this.currentBehaviourStateIndex;
            }
            if (this.focusedThreat == null || !this.focusingOnThreat)
              this.StopFocusingThreat();
            if (this.scoutingSearchRoutine.inProgress)
              this.StopSearch(this.scoutingSearchRoutine, false);
            this.agent.speed = 9f;
            float a = this.fearLevelNoDistComparison * 2f;
            if (this.focusedThreat.interestLevel <= 0 || this.enemyHP <= 3)
              a = Mathf.Max(a, 1f);
            float num1 = this.GetComfortableDistanceToThreat(this.focusedThreat) + a;
            float num2 = Vector3.Distance(this.transform.position, this.focusedThreat.lastSeenPosition);
            bool flag1 = false;
            float num3 = Time.realtimeSinceStartup - this.focusedThreat.timeLastSeen;
            if ((double) num3 > 5.0)
            {
              this.SetThreatInView(false);
              this.focusLevel = 0;
              this.StopFocusingThreat();
              break;
            }
            if ((double) num3 > 3.0)
            {
              this.SetThreatInView(false);
              this.focusLevel = 1;
              if ((double) num1 - (double) num2 > 2.0)
              {
                this.StopFocusingThreat();
                break;
              }
            }
            else if ((double) num3 > 1.0)
            {
              flag1 = true;
              this.focusedThreatIsInView = false;
              this.SetThreatInView(false);
              this.focusLevel = 2;
              this.SetAggressiveMode(0);
            }
            else if ((double) num3 < 0.550000011920929)
            {
              flag1 = true;
              this.SetThreatInView(true);
            }
            bool flag2 = (double) this.fearLevel > 0.0 && (double) this.fearLevel < 4.0 || this.focusedThreat.interestLevel > 0 || (double) this.fearLevel < -6.0 || this.focusedThreat.hasAttacked;
            if (this.aggressiveMode == 2)
            {
              this.focusLevel = 3;
              if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null)
              {
                this.DropHeldItemAndSync();
                this.focusedScrap = this.heldScrap;
              }
              Vector3 vector3 = this.focusedThreat.threatScript.GetThreatTransform().position + this.focusedThreat.threatScript.GetThreatVelocity() * 10f;
              Debug.DrawRay(vector3, Vector3.up * 5f, Color.red, this.AIIntervalTime);
              this.SetDestinationToPosition(vector3, true);
              if ((double) this.fightTimer > 4.0 || (double) this.timeSinceBeingAttackedByPlayer < 4.0 || (double) this.fightTimer > 2.0 && ((double) this.fearLevel >= 1.0 || !flag2) || this.enemyHP <= 3 && !flag2)
              {
                this.scoutTimer = this.timeToScout - 20f;
                this.fightTimer = -7f;
                this.SetAggressiveMode(1);
                break;
              }
              if ((double) num2 > 4.0)
              {
                this.fightTimer += this.AIIntervalTime * 2f;
                break;
              }
              if ((double) num2 > 1.0)
              {
                this.fightTimer += this.AIIntervalTime;
                break;
              }
              this.fightTimer += this.AIIntervalTime / 2f;
              break;
            }
            bool flag3 = false;
            if ((UnityEngine.Object) this.focusedScrap != (UnityEngine.Object) null && (!flag1 || (double) this.fearLevel <= 2.0))
            {
              this.SetDestinationToPosition(this.focusedScrap.transform.position);
              flag3 = true;
            }
            Vector3 start = this.focusedThreat.lastSeenPosition + this.focusedThreat.threatScript.GetThreatVelocity() * -17f;
            Debug.DrawRay(start, Vector3.up * 3f, Color.red, this.AIIntervalTime);
            Ray ray = new Ray(this.transform.position + Vector3.up * 0.5f, Vector3.Normalize((this.transform.position + Vector3.up * 0.5f - start) * 100f));
            RaycastHit hitInfo;
            Vector3 vector3_1 = !Physics.Raycast(ray, out hitInfo, num1 - num2, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) ? RoundManager.Instance.GetNavMeshPosition(ray.GetPoint(num1 - num2), RoundManager.Instance.navHit, 8f) : RoundManager.Instance.GetNavMeshPosition(hitInfo.point, RoundManager.Instance.navHit, 8f);
            Debug.DrawRay(vector3_1, Vector3.up, Color.blue, this.AIIntervalTime);
            if (!flag3)
              this.debugSphere = !this.SetDestinationToPosition(vector3_1, true) ? vector3_1 : vector3_1;
            if ((double) this.fightTimer > 7.0 && (double) this.timeSinceFighting > 4.0)
            {
              this.fightTimer = -6f;
              this.SetAggressiveMode(2);
              break;
            }
            bool flag4 = false;
            if (this.scoutingGroup != null)
            {
              for (int index = 0; index < this.scoutingGroup.members.Count; ++index)
              {
                if (this.scoutingGroup.members[index].aggressiveMode == 2)
                  flag4 = true;
              }
            }
            float num4 = this.GetComfortableDistanceToThreat(this.focusedThreat) - num2;
            if ((double) this.fearLevel <= -5.0)
            {
              if ((double) this.noiseTimer >= (double) this.noiseInterval)
              {
                this.noiseInterval = UnityEngine.Random.Range(0.2f, 0.7f);
                this.noiseTimer = 0.0f;
                RoundManager.PlayRandomClip(this.creatureVoice, this.cawLaughSFX, audibleNoiseID: 1105);
              }
              else
                this.noiseTimer += Time.deltaTime;
            }
            if (flag1 && ((double) num4 > 8.0 & flag2 || (double) num2 < 5.0) || (double) this.timeSinceBeingAttackedByPlayer < 4.0)
            {
              if ((double) this.timeSinceFighting > 5.0)
                this.fightTimer += (float) ((double) this.AIIntervalTime * 10.600000381469727 / ((double) this.focusedThreat.distanceToThreat * 0.30000001192092896));
              this.SetAggressiveMode(1);
              break;
            }
            if ((((double) num4 <= 4.0 ? 0 : ((double) this.fearLevel < 3.0 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0)
            {
              this.fightTimer += (float) ((double) this.AIIntervalTime * 7.4000000953674316 / ((double) this.focusedThreat.distanceToThreat * 0.30000001192092896));
              this.SetAggressiveMode(1);
              break;
            }
            if ((double) num4 >= 2.0)
              break;
            if ((double) this.timeSinceAggressiveDisplay > 2.5)
              this.SetAggressiveMode(0);
            this.fightTimer -= Mathf.Max(-6f, this.AIIntervalTime * 0.2f);
            if ((double) this.timeSpentFocusingOnThreat <= 4.0 + (double) this.focusedThreat.interestLevel * 8.0 || flag4)
              break;
            if ((double) this.fightTimer > 4.0)
            {
              this.fightTimer -= Mathf.Max(-6f, (float) ((double) this.AIIntervalTime * 0.5 * ((double) this.focusedThreat.distanceToThreat * 0.10000000149011612)));
              break;
            }
            this.StopFocusingThreat();
            break;
        }
      }
    }
  }

  private void StopFocusingThreat()
  {
    if (this.currentBehaviourStateIndex != 2)
      return;
    this.aggressiveMode = 0;
    this.focusingOnThreat = false;
    this.focusedThreatIsInView = false;
    this.focusedThreatTransform = (Transform) null;
    this.focusedThreat = (Threat) null;
    if ((UnityEngine.Object) this.heldScrap == (UnityEngine.Object) null)
      this.SwitchToBehaviourStateOnLocalClient(0);
    else
      this.SwitchToBehaviourStateOnLocalClient(1);
    this.StopFocusingThreatServerRpc((UnityEngine.Object) this.heldScrap == (UnityEngine.Object) null);
  }

  [ServerRpc]
  public void StopFocusingThreatServerRpc(bool enterScoutingMode)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1546030380U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enterScoutingMode, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1546030380U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopFocusingThreatClientRpc(enterScoutingMode);
  }

  [ClientRpc]
  public void StopFocusingThreatClientRpc(bool enterScoutingMode)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3360048400U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in enterScoutingMode, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3360048400U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.aggressiveMode = 0;
    this.focusedThreatTransform = (Transform) null;
    this.focusedThreat = (Threat) null;
    if (enterScoutingMode)
      this.SwitchToBehaviourStateOnLocalClient(0);
    else
      this.SwitchToBehaviourStateOnLocalClient(1);
  }

  private void SetAggressiveMode(int mode)
  {
    if (this.aggressiveMode == mode)
      return;
    this.aggressiveMode = mode;
    this.SetAggressiveModeServerRpc(mode);
  }

  [ServerRpc]
  public void SetAggressiveModeServerRpc(int mode)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(443869275U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, mode);
      this.__endSendServerRpc(ref bufferWriter, 443869275U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetAggressiveModeClientRpc(mode);
  }

  [ClientRpc]
  public void SetAggressiveModeClientRpc(int mode)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1782649174U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, mode);
      this.__endSendClientRpc(ref bufferWriter, 1782649174U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.aggressiveMode = mode;
  }

  private void SetThreatInView(bool inView)
  {
    if (this.focusedThreatIsInView == inView)
      return;
    this.focusedThreatIsInView = inView;
    this.SetThreatInViewServerRpc(inView);
  }

  [ServerRpc]
  public void SetThreatInViewServerRpc(bool inView)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3428942850U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in inView, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 3428942850U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SetThreatInViewClientRpc(inView);
  }

  [ClientRpc]
  public void SetThreatInViewClientRpc(bool inView)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2073937320U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in inView, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2073937320U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.focusedThreatIsInView = inView;
  }

  [ServerRpc]
  public void EnemyEnterRestModeServerRpc(bool sleep, bool atCamp)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1806580287U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in sleep, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in atCamp, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1806580287U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EnemyEnterRestModeClientRpc(sleep, atCamp);
  }

  [ClientRpc]
  public void EnemyEnterRestModeClientRpc(bool sleep, bool atCamp)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1567928363U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in sleep, new FastBufferWriter.ForPrimitives());
      bufferWriter.WriteValueSafe<bool>(in atCamp, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1567928363U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.restingAtCamp = atCamp;
    if (sleep)
    {
      this.eyesClosed = true;
      this.creatureAnimator.SetBool(nameof (sleep), true);
      this.creatureAnimator.SetBool("sit", false);
    }
    else
    {
      this.eyesClosed = false;
      this.creatureAnimator.SetBool(nameof (sleep), false);
      this.creatureAnimator.SetBool("sit", true);
    }
  }

  [ServerRpc]
  public void EnemyGetUpServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3614203845U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3614203845U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EnemyGetUpClientRpc();
  }

  [ClientRpc]
  public void EnemyGetUpClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1155909339U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1155909339U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.creatureAnimator.SetBool("sit", false);
  }

  public override void OnDrawGizmos()
  {
    if (!this.debugEnemyAI)
      return;
    if (this.currentBehaviourStateIndex == 1)
    {
      Gizmos.DrawCube(this.transform.position + Vector3.up * 2f, new Vector3(0.2f, 0.2f, 0.2f));
    }
    else
    {
      if (this.scoutingGroup == null)
        return;
      if ((UnityEngine.Object) this.scoutingGroup.leader == (UnityEngine.Object) this)
      {
        Gizmos.DrawSphere(this.transform.position + Vector3.up * 2f, 0.6f);
      }
      else
      {
        Gizmos.DrawLine(this.scoutingGroup.leader.transform.position + Vector3.up * 2f, this.transform.position + Vector3.up * 2f);
        Gizmos.DrawSphere(this.transform.position + Vector3.up * 2f, 0.1f);
      }
    }
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    if (!this.IsOwner || this.isEnemyDead)
      return;
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    if ((double) Vector3.Distance(noisePosition, this.transform.position + Vector3.up * 0.4f) < 0.75 || noiseID == 1105 || noiseID == 24751)
      return;
    float num1 = Vector3.Distance(noisePosition, this.transform.position);
    float num2 = noiseLoudness / num1;
    if (this.eyesClosed)
      num2 *= 0.75f;
    if ((double) num2 < 0.11999999731779099 && (double) this.peekTimer >= 0.0 && this.focusLevel > 0)
      return;
    if (this.focusLevel >= 3)
    {
      if ((double) num1 > 3.0 || (double) num2 <= 0.059999998658895493)
        return;
    }
    else if (this.focusLevel == 2)
    {
      if ((double) num1 > 25.0 || (double) num2 <= 0.05000000074505806)
        return;
    }
    else if (this.focusLevel == 1 && ((double) num1 > 40.0 || (double) num2 <= 0.05000000074505806))
      return;
    this.PingBaboonInterest(noisePosition, this.focusLevel);
  }

  private void AnimateLooking(Vector3 lookAtPosition)
  {
    this.headLookTarget.position = Vector3.Lerp(this.headLookTarget.position, lookAtPosition, 15f * Time.deltaTime);
    Vector3 position = this.headLookTarget.position with
    {
      y = this.transform.position.y
    };
    if ((double) Vector3.Angle(this.transform.forward, position - this.transform.position) <= 30.0)
      return;
    RoundManager.Instance.tempTransform.position = this.transform.position;
    RoundManager.Instance.tempTransform.LookAt(position);
    this.transform.rotation = Quaternion.Lerp(this.transform.rotation, RoundManager.Instance.tempTransform.rotation, 4f * Time.deltaTime);
    this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    this.timeSinceHitting += Time.deltaTime;
    if ((double) this.stunNormalizedTimer > 0.0 || (double) this.miscAnimationTimer > 0.0)
      this.agent.speed = 0.0f;
    this.creatureAnimator.SetBool("stunned", (double) this.stunNormalizedTimer > 0.0);
    if ((double) this.miscAnimationTimer <= 0.0)
      this.currentMiscAnimation = -1;
    else
      this.miscAnimationTimer -= Time.deltaTime;
    this.CalculateAnimationDirection(2f);
    this.timeSinceLastMiscAnimation += Time.deltaTime;
    this.timeSincePingingBirdInterest += Time.deltaTime;
    this.timeSinceBeingAttackedByPlayer += Time.deltaTime;
    this.timeSinceJoiningOrLeavingScoutingGroup += Time.deltaTime;
    if (this.debugEnemyAI)
    {
      if (this.focusedThreat != null && this.focusingOnThreat)
        HUDManager.Instance.SetDebugText(string.Format("{0}; {1}; \n Focused threat level: {2}", (object) this.fearLevel.ToString("0.0"), (object) this.fearLevelNoDistComparison.ToString("0.0"), (object) this.focusedThreat.threatLevel));
      else
        HUDManager.Instance.SetDebugText(this.fearLevel.ToString("0.0") + "; " + this.fearLevelNoDistComparison.ToString("0.0"));
    }
    if ((UnityEngine.Object) this.heldScrap != (UnityEngine.Object) null && !this.isEnemyDead)
    {
      this.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(this.creatureAnimator.GetLayerWeight(1), 1f, 12f * Time.deltaTime));
      this.rightArmRig.weight = Mathf.Lerp(this.rightArmRig.weight, 0.0f, 12f * Time.deltaTime);
      this.leftArmRig.weight = Mathf.Lerp(this.leftArmRig.weight, 0.0f, 12f * Time.deltaTime);
    }
    else
    {
      this.creatureAnimator.SetLayerWeight(1, Mathf.Lerp(this.creatureAnimator.GetLayerWeight(1), 0.0f, 12f * Time.deltaTime));
      this.rightArmRig.weight = Mathf.Lerp(this.rightArmRig.weight, 1f, 12f * Time.deltaTime);
      this.leftArmRig.weight = Mathf.Lerp(this.leftArmRig.weight, 1f, 12f * Time.deltaTime);
    }
    switch (this.aggressiveMode)
    {
      case 0:
        if (this.previousAggressiveMode != this.aggressiveMode)
        {
          this.creatureAnimator.SetBool("aggressiveDisplay", false);
          this.creatureAnimator.SetBool("fighting", false);
          this.previousAggressiveMode = this.aggressiveMode;
        }
        if ((double) this.aggressionAudio.volume <= 0.0)
          this.aggressionAudio.Stop();
        else
          this.aggressionAudio.volume = Mathf.Max(this.aggressionAudio.volume - Time.deltaTime * 5f, 0.0f);
        this.timeSinceAggressiveDisplay = 0.0f;
        break;
      case 1:
        if (this.previousAggressiveMode != this.aggressiveMode)
        {
          this.creatureAnimator.SetBool("aggressiveDisplay", true);
          this.creatureAnimator.SetBool("fighting", false);
          RoundManager.PlayRandomClip(this.creatureVoice, this.cawScreamSFX, audibleNoiseID: 1105);
          WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.enemyType.audioClips[1]);
          this.aggressionAudio.clip = this.enemyType.audioClips[2];
          this.aggressionAudio.Play();
          this.previousAggressiveMode = this.aggressiveMode;
        }
        this.timeSinceAggressiveDisplay += Time.deltaTime;
        this.aggressionAudio.volume = Mathf.Min(this.aggressionAudio.volume + Time.deltaTime * 4f, 1f);
        break;
      case 2:
        if (this.previousAggressiveMode != this.aggressiveMode)
        {
          this.creatureAnimator.SetBool("fighting", true);
          this.aggressionAudio.clip = this.enemyType.audioClips[3];
          this.aggressionAudio.Play();
          this.previousAggressiveMode = this.aggressiveMode;
        }
        this.timeSinceAggressiveDisplay += Time.deltaTime;
        this.aggressionAudio.volume = Mathf.Min(this.aggressionAudio.volume + Time.deltaTime * 5f, 1f);
        break;
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.creatureAnimator.SetBool("sleep", false);
        this.restingAtCamp = false;
        this.eyesClosed = false;
        this.focusedThreatTransform = (Transform) null;
        break;
      case 1:
        this.focusedThreatTransform = (Transform) null;
        break;
      case 2:
        if ((UnityEngine.Object) this.focusedThreatTransform != (UnityEngine.Object) null && this.focusedThreatIsInView)
          this.lookTarget = this.focusedThreatTransform.position;
        this.timeSpentFocusingOnThreat += Time.deltaTime;
        this.timeSinceFighting += Time.deltaTime;
        break;
    }
  }

  private float GetComfortableDistanceToThreat(Threat focusedThreat)
  {
    return Mathf.Min((float) focusedThreat.threatLevel * 6f, 25f);
  }

  private void ReactToThreat(Threat closestThreat)
  {
    if ((double) Vector3.Distance(closestThreat.lastSeenPosition, BaboonBirdAI.baboonCampPosition) < 18.0)
      ++closestThreat.interestLevel;
    if (closestThreat == this.focusedThreat || this.focusedThreat != null && this.focusedThreat.threatLevel > closestThreat.threatLevel || (double) closestThreat.distanceToThreat >= (double) this.GetComfortableDistanceToThreat(closestThreat))
      return;
    NetworkObject component = closestThreat.threatScript.GetThreatTransform().gameObject.GetComponent<NetworkObject>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null)
    {
      Debug.LogError((object) "Baboon: Error, threat did not contain network object. All objects implementing IVisibleThreat must have a NetworkObject");
    }
    else
    {
      this.fightTimer = 0.0f;
      this.focusingOnThreat = true;
      this.StartFocusOnThreatServerRpc((NetworkObjectReference) component);
      this.focusedThreat = closestThreat;
      this.focusedThreatTransform = closestThreat.threatScript.GetThreatLookTransform();
    }
  }

  [ServerRpc]
  public void StartFocusOnThreatServerRpc(NetworkObjectReference netObject)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3933590138U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 3933590138U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StartFocusOnThreatClientRpc(netObject);
  }

  [ClientRpc]
  public void StartFocusOnThreatClientRpc(NetworkObjectReference netObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(991811456U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in netObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 991811456U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(2);
    NetworkObject networkObject;
    if (!netObject.TryGet(out networkObject))
    {
      Debug.LogError((object) string.Format("Baboon: Error, could not get network object from id for StartFocusOnThreatClientRpc; id: {0}", (object) networkObject.NetworkObjectId));
    }
    else
    {
      IVisibleThreat component;
      if (!networkObject.transform.TryGetComponent<IVisibleThreat>(out component))
      {
        Debug.LogError((object) string.Format("Baboon: Error, threat transform did not contain IVisibleThreat in StartFocusOnThreatClientRpc; id: {0}", (object) networkObject.NetworkObjectId));
      }
      else
      {
        this.focusingOnThreat = true;
        this.focusedThreatTransform = component.GetThreatLookTransform();
      }
    }
  }

  private float ReactToOtherBaboonSighted(BaboonBirdAI otherBaboon)
  {
    float otherBaboonSighted = 0.0f;
    if (otherBaboon.isEnemyDead)
      otherBaboonSighted += 4f;
    else if (otherBaboon.currentBehaviourStateIndex != 1 && this.currentBehaviourStateIndex != 1)
    {
      if (otherBaboon.currentBehaviourStateIndex == 2 && otherBaboon.focusedThreatIsInView && (UnityEngine.Object) otherBaboon.focusedThreatTransform != (UnityEngine.Object) null)
      {
        int pingImportance = 3;
        if ((double) otherBaboon.fearLevel > 2.0 || otherBaboon.focusLevel >= 3)
          pingImportance = 4;
        this.PingBaboonInterest(otherBaboon.focusedThreatTransform.position, pingImportance);
      }
      if ((double) this.timeSinceJoiningOrLeavingScoutingGroup < 4.0 || otherBaboon.currentBehaviourStateIndex == 1 || this.scoutingGroup != null && (double) Time.realtimeSinceStartup - (double) this.scoutingGroup.timeAtLastCallToGroup < 1.0 || this.scoutingGroup != null && (this.scoutingGroup.members.Contains(otherBaboon) || !((UnityEngine.Object) this.scoutingGroup.leader != (UnityEngine.Object) otherBaboon)))
        return otherBaboonSighted;
      if (otherBaboon.scoutingGroup != null)
      {
        if (otherBaboon.scoutingGroup.leader.leadershipLevel > this.biggestBaboon.leadershipLevel)
          this.biggestBaboon = otherBaboon;
        return otherBaboonSighted;
      }
      if (otherBaboon.leadershipLevel > this.biggestBaboon.leadershipLevel)
      {
        this.biggestBaboon = otherBaboon;
        return otherBaboonSighted;
      }
    }
    return otherBaboonSighted;
  }

  private void DoLOSCheck()
  {
    Threat closestThreat1 = (Threat) null;
    Threat closestThreat2 = (Threat) null;
    float num1 = 0.0f;
    float num2 = 0.0f;
    float num3 = 0.0f;
    float num4 = 0.0f;
    int num5 = Physics.OverlapSphereNonAlloc(this.eye.position + this.eye.forward * 38f + this.eye.up * 8f, 40f, RoundManager.Instance.tempColliderResults, this.visibleThreatsMask, QueryTriggerInteraction.Collide);
    this.biggestBaboon = this;
    if (this.scoutingGroup != null && (UnityEngine.Object) this.scoutingGroup.leader != (UnityEngine.Object) null)
      this.biggestBaboon = this.scoutingGroup.leader;
    for (int index = 0; index < num5; ++index)
    {
      if (!((UnityEngine.Object) RoundManager.Instance.tempColliderResults[index] == (UnityEngine.Object) this.ownCollider))
      {
        float num6 = Vector3.Distance(this.eye.position, RoundManager.Instance.tempColliderResults[index].transform.position);
        float num7 = Vector3.Angle(RoundManager.Instance.tempColliderResults[index].transform.position - this.eye.position, this.eye.forward);
        if ((double) num6 <= 2.0 || (double) num7 <= (double) this.fov)
        {
          RaycastHit hitInfo;
          if (Physics.Linecast(this.transform.position + Vector3.up * 0.7f, RoundManager.Instance.tempColliderResults[index].transform.position + Vector3.up * 0.5f, out hitInfo, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
          {
            if (this.debugEnemyAI)
              Debug.DrawRay(hitInfo.point, Vector3.up * 0.5f, Color.magenta, this.AIIntervalTime);
          }
          else
          {
            EnemyAICollisionDetect component1 = RoundManager.Instance.tempColliderResults[index].transform.GetComponent<EnemyAICollisionDetect>();
            if ((UnityEngine.Object) component1 != (UnityEngine.Object) null && component1.mainScript.GetType() == typeof (BaboonBirdAI))
            {
              float otherBaboonSighted = this.ReactToOtherBaboonSighted(component1.mainScript as BaboonBirdAI);
              num3 += otherBaboonSighted;
              num4 += otherBaboonSighted;
            }
            else
            {
              IVisibleThreat component2;
              if (RoundManager.Instance.tempColliderResults[index].transform.TryGetComponent<IVisibleThreat>(out component2))
              {
                float visibility = component2.GetVisibility();
                if ((double) visibility >= 1.0 || (double) visibility != 0.0 && ((double) visibility >= 0.20000000298023224 || (double) num6 <= 10.0) && ((double) visibility >= 0.60000002384185791 || (double) num6 <= 20.0 || (double) num7 <= 30.0) && ((double) visibility >= 0.800000011920929 || (double) num6 <= 16.0 || (double) num7 <= 80.0))
                {
                  if (this.debugEnemyAI)
                    Debug.Log((object) string.Format("Baboon hawk: Seeing visible threat: {0}; type: {1}", (object) RoundManager.Instance.tempColliderResults[index].transform.name, (object) component2.type));
                  Threat threat;
                  if (!this.threats.TryGetValue(RoundManager.Instance.tempColliderResults[index].transform, out threat))
                    threat = new Threat();
                  else if ((double) Time.realtimeSinceStartup - (double) threat.timeLastSeen >= 0.5)
                    threat.distanceMovedTowardsBaboon = threat.distanceToThreat - num6;
                  else
                    continue;
                  threat.type = component2.type;
                  threat.timeLastSeen = Time.realtimeSinceStartup;
                  threat.lastSeenPosition = RoundManager.Instance.tempColliderResults[index].transform.position + Vector3.up * 0.5f;
                  threat.distanceToThreat = num6;
                  threat.threatLevel = component2.GetThreatLevel(this.eye.position);
                  threat.threatScript = component2;
                  if ((double) threat.distanceMovedTowardsBaboon > 1.0)
                    ++threat.threatLevel;
                  else if ((double) Mathf.Abs(threat.distanceMovedTowardsBaboon) < 1.0 || (double) threat.distanceMovedTowardsBaboon < -1.0)
                    --threat.threatLevel;
                  threat.interestLevel = component2.GetInterestLevel();
                  float num8 = (float) threat.threatLevel / (threat.distanceToThreat * 0.2f);
                  if ((double) Vector3.Distance(threat.lastSeenPosition, BaboonBirdAI.baboonCampPosition) < 9.0)
                  {
                    threat.interestLevel += 2;
                    num8 *= 0.5f;
                  }
                  if (threat.hasAttacked)
                  {
                    ++threat.interestLevel;
                    if (this.scoutingGroup != null && this.scoutingGroup.members.Count > 3)
                      num8 -= (float) this.scoutingGroup.members.Count / 1.5f;
                    else
                      num8 += 2f;
                  }
                  num3 += num8;
                  num4 += (float) threat.threatLevel;
                  if ((double) threat.threatLevel < (double) num2)
                  {
                    closestThreat2 = threat;
                    num2 = (float) threat.threatLevel;
                  }
                  else if ((double) num8 > (double) num1)
                  {
                    num1 = num8;
                    closestThreat1 = threat;
                  }
                  this.threats.TryAdd(RoundManager.Instance.tempColliderResults[index].transform, threat);
                }
              }
            }
          }
        }
      }
    }
    this.oddAIInterval = !this.oddAIInterval;
    if (this.oddAIInterval && this.aggressiveMode != 2 && !this.eyesClosed && !this.restingAtCamp)
    {
      GrabbableObject grabbableObject = (GrabbableObject) null;
      int num9 = 0;
      int num10 = Physics.OverlapSphereNonAlloc(this.eye.position + this.eye.forward * 28f + this.eye.up * 6f, 30f, RoundManager.Instance.tempColliderResults, this.scrapMask, QueryTriggerInteraction.Collide);
      for (int index = 0; index < num10; ++index)
      {
        float num11 = Vector3.Angle(RoundManager.Instance.tempColliderResults[index].transform.position - this.eye.position, this.eye.forward);
        float num12 = Vector3.Distance(this.eye.position, RoundManager.Instance.tempColliderResults[index].transform.position);
        if ((double) num12 > 2.0 && (double) num11 > (double) this.fov)
          Debug.Log((object) string.Format("Baboon #{0}; could not see threat, b", (object) this.thisEnemyIndex));
        else if (!Physics.Linecast(this.transform.position + Vector3.up * 0.7f, RoundManager.Instance.tempColliderResults[index].transform.position + Vector3.up * 0.5f, out RaycastHit _, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore) && (double) num12 < 20.0 && (bool) (UnityEngine.Object) RoundManager.Instance.tempColliderResults[index].gameObject.GetComponent<GrabbableObject>())
        {
          GrabbableObject component = RoundManager.Instance.tempColliderResults[index].gameObject.GetComponent<GrabbableObject>();
          if (component.scrapValue > 3 && component.scrapValue > num9 && this.CanGrabScrap(component))
          {
            num9 = component.scrapValue;
            grabbableObject = component;
          }
        }
      }
      if ((UnityEngine.Object) grabbableObject != (UnityEngine.Object) null)
        this.focusedScrap = grabbableObject;
    }
    if ((UnityEngine.Object) this.biggestBaboon != (UnityEngine.Object) this)
      this.JoinScoutingGroup(this.biggestBaboon);
    if (this.scoutingGroup != null)
    {
      num3 -= (float) this.scoutingGroup.members.Count;
      num4 -= (float) this.scoutingGroup.members.Count;
    }
    this.fearLevel = num3 + 1f;
    this.fearLevelNoDistComparison = num4;
    float num13 = 0.0f;
    if (this.focusingOnThreat)
      num13 = 2f;
    if ((double) this.fearLevel > (double) num13 && closestThreat1 != null)
    {
      this.ReactToThreat(closestThreat1);
    }
    else
    {
      if ((double) this.fearLevel > -(double) num13 || closestThreat2 == null)
        return;
      this.ReactToThreat(closestThreat2);
    }
  }

  public void PingBaboonInterest(Vector3 interestPosition, int pingImportance)
  {
    if (this.focusedThreat != null && pingImportance < this.focusLevel)
    {
      Debug.Log((object) string.Format("Baboon bird #{0}: Did NOT listen to ping of importance {1} as focus level is {2}", (object) this.thisEnemyIndex, (object) pingImportance, (object) this.focusLevel));
    }
    else
    {
      if (pingImportance < this.focusLevel && (double) this.timeSincePingingBirdInterest < (double) Mathf.Max(0.6f, (float) this.focusLevel / 2f) || this.focusingOnThreat && (double) Vector3.Distance(this.focusedThreat.lastSeenPosition, interestPosition) < 4.0)
        return;
      this.timeSincePingingBirdInterest = 0.0f;
      this.peekTimer = 0.7f / (float) Mathf.Max(this.focusLevel / Mathf.Max(pingImportance, 1), 1);
      this.peekTarget = interestPosition;
      if (this.currentBehaviourStateIndex == 1)
      {
        this.eyesClosed = false;
        this.peekTimer = Mathf.Max(this.peekTimer, 1.5f);
      }
      this.PingBirdInterestServerRpc(this.peekTarget, this.peekTimer);
    }
  }

  [ServerRpc]
  public void PingBirdInterestServerRpc(Vector3 lookPosition, float timeToPeek)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1670979535U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in lookPosition);
      bufferWriter.WriteValueSafe<float>(in timeToPeek, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1670979535U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.PingBirdInterestClientRpc(lookPosition, timeToPeek);
  }

  [ClientRpc]
  public void PingBirdInterestClientRpc(Vector3 lookPosition, float timeToPeek)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2348332192U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in lookPosition);
      bufferWriter.WriteValueSafe<float>(in timeToPeek, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 2348332192U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.peekTimer = timeToPeek;
    this.peekTarget = lookPosition;
  }

  private void JoinScoutingGroup(BaboonBirdAI otherBaboon)
  {
    if (otherBaboon.scoutingGroup != null && otherBaboon.scoutingGroup == this.scoutingGroup && otherBaboon.scoutingGroup.members.Contains(this) || this.PathIsIntersectedByLineOfSight(otherBaboon.transform.position, true, false) || (double) Vector3.Distance(this.transform.position, otherBaboon.transform.position) > 56.0)
      return;
    this.timeSinceJoiningOrLeavingScoutingGroup = 0.0f;
    if (otherBaboon.scoutingGroup != this.scoutingGroup)
      this.LeaveCurrentScoutingGroup(false);
    if (otherBaboon.scoutingGroup == null)
    {
      otherBaboon.StartScoutingGroup(this, true);
    }
    else
    {
      if (this.scoutingGroup != null)
        return;
      this.scoutingGroup = otherBaboon.scoutingGroup;
      this.JoinScoutingGroupServerRpc((NetworkObjectReference) otherBaboon.NetworkObject);
      this.StartMiscAnimationServerRpc(0);
    }
  }

  public void StartScoutingGroup(BaboonBirdAI firstMember, bool syncWithClients)
  {
    if (this.scoutingGroup != null)
      return;
    this.timeSinceJoiningOrLeavingScoutingGroup = 0.0f;
    this.scoutingGroup = new BaboonHawkGroup();
    this.scoutingGroup.leader = this;
    this.scoutingGroup.members.Add(firstMember);
    firstMember.scoutingGroup = this.scoutingGroup;
    this.scoutingGroup.isEmpty = false;
    if (!syncWithClients)
      return;
    if ((double) this.miscAnimationTimer <= 0.0)
      this.StartMiscAnimationServerRpc(0);
    this.StartScoutingGroupServerRpc((NetworkObjectReference) firstMember.NetworkObject);
  }

  private void LeaveCurrentScoutingGroup(bool sync)
  {
    if (this.scoutingGroup == null)
      return;
    this.timeSinceJoiningOrLeavingScoutingGroup = 0.0f;
    if (this.scoutingGroup.members.Contains(this))
    {
      this.scoutingGroup.members.Remove(this);
      if (this.scoutingGroup.members.Count <= 0)
        this.scoutingGroup.isEmpty = true;
    }
    else if ((UnityEngine.Object) this.scoutingGroup.leader == (UnityEngine.Object) this)
    {
      if (this.scoutingGroup.members != null && this.scoutingGroup.members.Count > 0)
      {
        int num = -1;
        int index1 = -1;
        for (int index2 = 0; index2 < this.scoutingGroup.members.Count; ++index2)
        {
          if (this.scoutingGroup.members[index2].leadershipLevel > num)
          {
            index1 = index2;
            num = this.scoutingGroup.members[index2].leadershipLevel;
          }
        }
        this.scoutingGroup.leader = this.scoutingGroup.members[index1];
        this.scoutingGroup.members.RemoveAt(index1);
      }
      else
        this.scoutingGroup.isEmpty = true;
    }
    else
      Debug.LogError((object) string.Format("Baboon #{0}: Scouting group was not null but did not contain me as a member!", (object) this.thisEnemyIndex));
    this.scoutingGroup = (BaboonHawkGroup) null;
  }

  [ServerRpc]
  public void LeaveScoutingGroupServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2459653399U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2459653399U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LeaveScoutingGroupClientRpc();
  }

  [ClientRpc]
  public void LeaveScoutingGroupClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(696889160U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 696889160U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.LeaveCurrentScoutingGroup(false);
  }

  [ServerRpc]
  public void StartScoutingGroupServerRpc(NetworkObjectReference leaderNetworkObject)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3367846835U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in leaderNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 3367846835U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StartScoutingGroupClientRpc(leaderNetworkObject);
  }

  [ClientRpc]
  public void StartScoutingGroupClientRpc(NetworkObjectReference leaderNetworkObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1737299197U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in leaderNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 1737299197U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (!leaderNetworkObject.TryGet(out networkObject))
      Debug.LogError((object) string.Format("Baboon enemy #{0}: Could not get network object from reference in JoinScoutingGroupClientRpc; {1}", (object) this.thisEnemyIndex, (object) leaderNetworkObject.NetworkObjectId));
    else
      this.StartScoutingGroup(networkObject.gameObject.GetComponent<BaboonBirdAI>(), false);
  }

  [ServerRpc]
  public void JoinScoutingGroupServerRpc(NetworkObjectReference otherBaboonNetworkObject)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1775372234U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in otherBaboonNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 1775372234U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.JoinScoutingGroupClientRpc(otherBaboonNetworkObject);
  }

  [ClientRpc]
  public void JoinScoutingGroupClientRpc(NetworkObjectReference otherBaboonNetworkObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1078565091U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in otherBaboonNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 1078565091U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    NetworkObject networkObject;
    if (!otherBaboonNetworkObject.TryGet(out networkObject))
    {
      Debug.LogError((object) string.Format("Baboon enemy #{0}: Could not get network object from reference in JoinScoutingGroupClientRpc; {1}", (object) this.thisEnemyIndex, (object) otherBaboonNetworkObject.NetworkObjectId));
    }
    else
    {
      BaboonBirdAI component = networkObject.gameObject.GetComponent<BaboonBirdAI>();
      if (component.scoutingGroup == this.scoutingGroup && component.scoutingGroup.members.Contains(this) || component.scoutingGroup == null)
        return;
      if (component.scoutingGroup != this.scoutingGroup)
        this.LeaveCurrentScoutingGroup(false);
      this.scoutingGroup = component.scoutingGroup;
      if (this.scoutingGroup.members.Contains(this))
        return;
      this.scoutingGroup.members.Add(this);
    }
  }

  public void CallToOtherBaboon(BaboonBirdAI otherBaboon)
  {
    if ((double) this.timeSinceJoiningOrLeavingScoutingGroup <= 1.0)
      return;
    if (this.scoutingGroup != null)
      this.scoutingGroup.timeAtLastCallToGroup = Time.realtimeSinceStartup;
    this.StartMiscAnimation(0);
    otherBaboon.PingBaboonInterest(this.transform.position, 2);
  }

  private void StartMiscAnimation(int anim)
  {
    if (this.isEnemyDead || (double) this.timeSinceLastMiscAnimation <= 0.40000000596046448)
      return;
    this.timeSinceLastMiscAnimation = 0.0f;
    this.StartMiscAnimationServerRpc(anim);
  }

  [ServerRpc]
  public void StartMiscAnimationServerRpc(int miscAnimationId)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1580405641U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, miscAnimationId);
      this.__endSendServerRpc(ref bufferWriter, 1580405641U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.isEnemyDead || this.enemyType.miscAnimations.Length <= miscAnimationId || (UnityEngine.Object) this.creatureVoice == (UnityEngine.Object) null || this.currentMiscAnimation != -1 && this.enemyType.miscAnimations[this.currentMiscAnimation].priority > this.enemyType.miscAnimations[miscAnimationId].priority)
      return;
    this.StartMiscAnimationClientRpc(miscAnimationId);
  }

  [ClientRpc]
  public void StartMiscAnimationClientRpc(int miscAnimationId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3995026000U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, miscAnimationId);
      this.__endSendClientRpc(ref bufferWriter, 3995026000U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.isEnemyDead || this.enemyType.miscAnimations.Length <= miscAnimationId || (UnityEngine.Object) this.creatureVoice == (UnityEngine.Object) null || this.currentMiscAnimation != -1 && this.enemyType.miscAnimations[this.currentMiscAnimation].priority > this.enemyType.miscAnimations[miscAnimationId].priority)
      return;
    this.currentMiscAnimation = miscAnimationId;
    this.miscAnimationTimer = this.enemyType.miscAnimations[miscAnimationId].AnimLength;
    if (this.inSpecialAnimation && !this.doingKillAnimation)
      return;
    this.creatureVoice.pitch = UnityEngine.Random.Range(0.8f, 1.2f);
    this.creatureVoice.PlayOneShot(this.enemyType.miscAnimations[miscAnimationId].AnimVoiceclip, UnityEngine.Random.Range(0.6f, 1f));
    WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.enemyType.miscAnimations[miscAnimationId].AnimVoiceclip, 0.7f);
    this.creatureAnimator.ResetTrigger(this.enemyType.miscAnimations[miscAnimationId].AnimString);
    this.creatureAnimator.SetTrigger(this.enemyType.miscAnimations[miscAnimationId].AnimString);
  }

  private void CalculateAnimationDirection(float maxSpeed = 1f)
  {
    this.agentLocalVelocity = this.animationContainer.InverseTransformDirection(Vector3.ClampMagnitude(this.transform.position - this.previousPosition, 1f) / (Time.deltaTime * 2f));
    this.velX = Mathf.Lerp(this.velX, this.agentLocalVelocity.x, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityX", Mathf.Clamp(this.velX, -maxSpeed, maxSpeed));
    this.velZ = Mathf.Lerp(this.velZ, this.agentLocalVelocity.z, 10f * Time.deltaTime);
    this.creatureAnimator.SetFloat("VelocityZ", Mathf.Clamp(this.velZ, -maxSpeed, maxSpeed));
    this.previousPosition = this.transform.position;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_BaboonBirdAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3452382367U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3452382367)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3856685904U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3856685904)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2476579270U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_2476579270)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3749667856U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3749667856)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1418775270U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1418775270)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1865475504U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1865475504)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(869682226U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_869682226)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1564051222U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1564051222)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1546030380U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1546030380)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3360048400U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3360048400)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(443869275U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_443869275)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1782649174U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1782649174)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3428942850U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3428942850)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2073937320U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_2073937320)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1806580287U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1806580287)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1567928363U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1567928363)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3614203845U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3614203845)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1155909339U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1155909339)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3933590138U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3933590138)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(991811456U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_991811456)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1670979535U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1670979535)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2348332192U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_2348332192)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2459653399U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_2459653399)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(696889160U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_696889160)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3367846835U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3367846835)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1737299197U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1737299197)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1775372234U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1775372234)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1078565091U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1078565091)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1580405641U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_1580405641)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3995026000U, new NetworkManager.RpcReceiveHandler(BaboonBirdAI.__rpc_handler_3995026000)));
  }

  private static void __rpc_handler_3452382367(
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
      int syncLeadershipLevel;
      ByteUnpacker.ReadValueBitPacked(reader, out syncLeadershipLevel);
      Vector3 campPosition;
      reader.ReadValueSafe(out campPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).SyncInitialValuesServerRpc(syncLeadershipLevel, campPosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3856685904(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int syncLeadershipLevel;
    ByteUnpacker.ReadValueBitPacked(reader, out syncLeadershipLevel);
    Vector3 campPosition;
    reader.ReadValueSafe(out campPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).SyncInitialValuesClientRpc(syncLeadershipLevel, campPosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2476579270(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObject;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((BaboonBirdAI) target).StabPlayerDeathAnimServerRpc(playerObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3749667856(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObject;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).StabPlayerDeathAnimClientRpc(playerObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1418775270(
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
      NetworkObjectReference networkObjectReference;
      reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, new FastBufferWriter.ForNetworkSerializable());
      Vector3 targetFloorPosition;
      reader.ReadValueSafe(out targetFloorPosition);
      int clientWhoSentRPC;
      ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSentRPC);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).DropScrapServerRpc(networkObjectReference, targetFloorPosition, clientWhoSentRPC);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1865475504(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference networkObjectReference;
    reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, new FastBufferWriter.ForNetworkSerializable());
    Vector3 targetFloorPosition;
    reader.ReadValueSafe(out targetFloorPosition);
    int clientWhoSentRPC;
    ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSentRPC);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).DropScrapClientRpc(networkObjectReference, targetFloorPosition, clientWhoSentRPC);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_869682226(
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
      NetworkObjectReference networkObjectReference;
      reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, new FastBufferWriter.ForNetworkSerializable());
      int clientWhoSentRPC;
      ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSentRPC);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).GrabScrapServerRpc(networkObjectReference, clientWhoSentRPC);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1564051222(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference networkObjectReference;
    reader.ReadValueSafe<NetworkObjectReference>(out networkObjectReference, new FastBufferWriter.ForNetworkSerializable());
    int clientWhoSentRPC;
    ByteUnpacker.ReadValueBitPacked(reader, out clientWhoSentRPC);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).GrabScrapClientRpc(networkObjectReference, clientWhoSentRPC);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1546030380(
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
      bool enterScoutingMode;
      reader.ReadValueSafe<bool>(out enterScoutingMode, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).StopFocusingThreatServerRpc(enterScoutingMode);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3360048400(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool enterScoutingMode;
    reader.ReadValueSafe<bool>(out enterScoutingMode, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).StopFocusingThreatClientRpc(enterScoutingMode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_443869275(
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
      int mode;
      ByteUnpacker.ReadValueBitPacked(reader, out mode);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).SetAggressiveModeServerRpc(mode);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1782649174(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int mode;
    ByteUnpacker.ReadValueBitPacked(reader, out mode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).SetAggressiveModeClientRpc(mode);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3428942850(
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
      bool inView;
      reader.ReadValueSafe<bool>(out inView, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).SetThreatInViewServerRpc(inView);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2073937320(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool inView;
    reader.ReadValueSafe<bool>(out inView, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).SetThreatInViewClientRpc(inView);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1806580287(
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
      bool sleep;
      reader.ReadValueSafe<bool>(out sleep, new FastBufferWriter.ForPrimitives());
      bool atCamp;
      reader.ReadValueSafe<bool>(out atCamp, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).EnemyEnterRestModeServerRpc(sleep, atCamp);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1567928363(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool sleep;
    reader.ReadValueSafe<bool>(out sleep, new FastBufferWriter.ForPrimitives());
    bool atCamp;
    reader.ReadValueSafe<bool>(out atCamp, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).EnemyEnterRestModeClientRpc(sleep, atCamp);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3614203845(
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
      ((BaboonBirdAI) target).EnemyGetUpServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1155909339(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).EnemyGetUpClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3933590138(
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
      NetworkObjectReference netObject;
      reader.ReadValueSafe<NetworkObjectReference>(out netObject, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).StartFocusOnThreatServerRpc(netObject);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_991811456(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference netObject;
    reader.ReadValueSafe<NetworkObjectReference>(out netObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).StartFocusOnThreatClientRpc(netObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1670979535(
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
      Vector3 lookPosition;
      reader.ReadValueSafe(out lookPosition);
      float timeToPeek;
      reader.ReadValueSafe<float>(out timeToPeek, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).PingBirdInterestServerRpc(lookPosition, timeToPeek);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2348332192(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 lookPosition;
    reader.ReadValueSafe(out lookPosition);
    float timeToPeek;
    reader.ReadValueSafe<float>(out timeToPeek, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).PingBirdInterestClientRpc(lookPosition, timeToPeek);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2459653399(
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
      ((BaboonBirdAI) target).LeaveScoutingGroupServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_696889160(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).LeaveScoutingGroupClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3367846835(
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
      NetworkObjectReference leaderNetworkObject;
      reader.ReadValueSafe<NetworkObjectReference>(out leaderNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).StartScoutingGroupServerRpc(leaderNetworkObject);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1737299197(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference leaderNetworkObject;
    reader.ReadValueSafe<NetworkObjectReference>(out leaderNetworkObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).StartScoutingGroupClientRpc(leaderNetworkObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1775372234(
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
      NetworkObjectReference otherBaboonNetworkObject;
      reader.ReadValueSafe<NetworkObjectReference>(out otherBaboonNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).JoinScoutingGroupServerRpc(otherBaboonNetworkObject);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1078565091(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference otherBaboonNetworkObject;
    reader.ReadValueSafe<NetworkObjectReference>(out otherBaboonNetworkObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).JoinScoutingGroupClientRpc(otherBaboonNetworkObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1580405641(
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
      int miscAnimationId;
      ByteUnpacker.ReadValueBitPacked(reader, out miscAnimationId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((BaboonBirdAI) target).StartMiscAnimationServerRpc(miscAnimationId);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3995026000(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int miscAnimationId;
    ByteUnpacker.ReadValueBitPacked(reader, out miscAnimationId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BaboonBirdAI) target).StartMiscAnimationClientRpc(miscAnimationId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (BaboonBirdAI);
}
