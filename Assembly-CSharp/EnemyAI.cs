// Decompiled with JetBrains decompiler
// Type: EnemyAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
public abstract class EnemyAI : NetworkBehaviour
{
  public EnemyType enemyType;
  [Space(5f)]
  public SkinnedMeshRenderer[] skinnedMeshRenderers;
  public MeshRenderer[] meshRenderers;
  public Animator creatureAnimator;
  public AudioSource creatureVoice;
  public AudioSource creatureSFX;
  public Transform eye;
  public AudioClip dieSFX;
  [Space(3f)]
  public EnemyBehaviourState[] enemyBehaviourStates;
  public EnemyBehaviourState currentBehaviourState;
  public int currentBehaviourStateIndex;
  public int previousBehaviourStateIndex;
  private int currentOwnershipOnThisClient = -1;
  public bool isInsidePlayerShip;
  [Header("AI Calculation / Netcode")]
  public float AIIntervalTime = 0.2f;
  public bool inSpecialAnimation;
  public PlayerControllerB inSpecialAnimationWithPlayer;
  [HideInInspector]
  public Vector3 serverPosition;
  [HideInInspector]
  public Vector3 serverRotation;
  private float previousYRotation;
  private float targetYRotation;
  public NavMeshAgent agent;
  [HideInInspector]
  public NavMeshPath path1;
  public GameObject[] allAINodes;
  public Transform targetNode;
  public Transform favoriteSpot;
  [HideInInspector]
  public float tempDist;
  [HideInInspector]
  public float mostOptimalDistance;
  [HideInInspector]
  public float pathDistance;
  [HideInInspector]
  public NetworkObject thisNetworkObject;
  public int thisEnemyIndex;
  public bool isClientCalculatingAI;
  public float updatePositionThreshold = 1f;
  private Vector3 tempVelocity;
  public PlayerControllerB targetPlayer;
  public bool movingTowardsTargetPlayer;
  public bool moveTowardsDestination = true;
  public Vector3 destination;
  public float addPlayerVelocityToDestination;
  private float updateDestinationInterval;
  public float syncMovementSpeed = 0.22f;
  public float timeSinceSpawn;
  public float exitVentAnimationTime = 1f;
  public bool ventAnimationFinished;
  [Space(5f)]
  public bool isEnemyDead;
  public bool daytimeEnemyLeaving;
  public int enemyHP = 3;
  private GameObject[] nodesTempArray;
  public float openDoorSpeedMultiplier;
  public bool useSecondaryAudiosOnAnimatedObjects;
  public AISearchRoutine currentSearch;
  public Coroutine searchCoroutine;
  public Coroutine chooseTargetNodeCoroutine;
  private RaycastHit raycastHit;
  private Ray LOSRay;
  public bool DebugEnemy;
  public int stunnedIndefinitely;
  public float stunNormalizedTimer;
  public float postStunInvincibilityTimer;
  public PlayerControllerB stunnedByPlayer;
  private float setDestinationToPlayerInterval;
  public bool debugEnemyAI;
  private bool removedPowerLevel;
  public bool isOutside;
  private System.Random searchRoutineRandom;

  public virtual void SetEnemyStunned(
    bool setToStunned,
    float setToStunTime = 1f,
    PlayerControllerB setStunnedByPlayer = null)
  {
    if (this.isEnemyDead || !this.enemyType.canBeStunned)
      return;
    if (setToStunned)
    {
      if ((double) this.postStunInvincibilityTimer >= 0.0)
        return;
      if ((double) this.stunNormalizedTimer <= 0.0 && (UnityEngine.Object) this.creatureVoice != (UnityEngine.Object) null)
        this.creatureVoice.PlayOneShot(this.enemyType.stunSFX);
      this.stunnedByPlayer = setStunnedByPlayer;
      this.postStunInvincibilityTimer = 0.5f;
      this.stunNormalizedTimer = setToStunTime;
    }
    else
    {
      this.stunnedByPlayer = (PlayerControllerB) null;
      if ((double) this.stunNormalizedTimer <= 0.0)
        return;
      this.stunNormalizedTimer = 0.0f;
    }
  }

  public virtual void Start()
  {
    try
    {
      this.agent = this.gameObject.GetComponentInChildren<NavMeshAgent>();
      this.skinnedMeshRenderers = this.gameObject.GetComponentsInChildren<SkinnedMeshRenderer>();
      this.meshRenderers = this.gameObject.GetComponentsInChildren<MeshRenderer>();
      if ((UnityEngine.Object) this.creatureAnimator == (UnityEngine.Object) null)
        this.creatureAnimator = this.gameObject.GetComponentInChildren<Animator>();
      this.thisNetworkObject = this.gameObject.GetComponentInChildren<NetworkObject>();
      this.serverPosition = this.transform.position;
      this.thisEnemyIndex = RoundManager.Instance.numberOfEnemiesInScene;
      ++RoundManager.Instance.numberOfEnemiesInScene;
      this.isOutside = this.enemyType.isOutsideEnemy;
      if (this.enemyType.isOutsideEnemy)
      {
        this.allAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
        if ((UnityEngine.Object) GameNetworkManager.Instance.localPlayerController != (UnityEngine.Object) null)
          this.EnableEnemyMesh(!StartOfRound.Instance.hangarDoorsClosed || !GameNetworkManager.Instance.localPlayerController.isInHangarShipRoom);
      }
      else
        this.allAINodes = GameObject.FindGameObjectsWithTag("AINode");
      this.path1 = new NavMeshPath();
      this.openDoorSpeedMultiplier = this.enemyType.doorSpeedMultiplier;
      if (this.IsOwner)
        this.SyncPositionToClients();
      else
        this.SetClientCalculatingAI(false);
    }
    catch (Exception ex)
    {
      Debug.LogError((object) string.Format("Error when initializing enemy variables for {0} : {1}", (object) this.gameObject.name, (object) ex));
    }
  }

  public PlayerControllerB MeetsStandardPlayerCollisionConditions(
    Collider other,
    bool inKillAnimation = false,
    bool overrideIsInsideFactoryCheck = false)
  {
    if (this.isEnemyDead)
      return (PlayerControllerB) null;
    if (!this.ventAnimationFinished)
      return (PlayerControllerB) null;
    if (inKillAnimation)
      return (PlayerControllerB) null;
    if ((double) this.stunNormalizedTimer >= 0.0)
      return (PlayerControllerB) null;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if ((UnityEngine.Object) component == (UnityEngine.Object) null || (UnityEngine.Object) component != (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
      return (PlayerControllerB) null;
    return !this.PlayerIsTargetable(component, overrideInsideFactoryCheck: overrideIsInsideFactoryCheck) ? (PlayerControllerB) null : component;
  }

  public virtual void OnCollideWithPlayer(Collider other)
  {
    if (!this.debugEnemyAI)
      return;
    Debug.Log((object) (this.gameObject.name + ": Collided with player!"));
  }

  public virtual void OnCollideWithEnemy(Collider other, EnemyAI collidedEnemy = null)
  {
    if (!this.IsServer || !this.debugEnemyAI)
      return;
    Debug.Log((object) (this.gameObject.name + " collided with enemy!: " + other.gameObject.name));
  }

  public void SwitchToBehaviourState(int stateIndex)
  {
    this.SwitchToBehaviourStateOnLocalClient(stateIndex);
    this.SwitchToBehaviourServerRpc(stateIndex);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SwitchToBehaviourServerRpc(int stateIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2081148948U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, stateIndex);
      this.__endSendServerRpc(ref bufferWriter, 2081148948U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || !this.NetworkObject.IsSpawned)
      return;
    this.SwitchToBehaviourClientRpc(stateIndex);
  }

  [ClientRpc]
  public void SwitchToBehaviourClientRpc(int stateIndex)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2962895088U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, stateIndex);
      this.__endSendClientRpc(ref bufferWriter, 2962895088U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || stateIndex == this.currentBehaviourStateIndex)
      return;
    this.SwitchToBehaviourStateOnLocalClient(stateIndex);
  }

  public void SwitchToBehaviourStateOnLocalClient(int stateIndex)
  {
    Debug.Log((object) string.Format("Current behaviour state: {0}", (object) this.currentBehaviourStateIndex));
    if (this.currentBehaviourStateIndex == stateIndex)
      return;
    Debug.Log((object) string.Format("CHANGING BEHAVIOUR STATE!!! to {0}", (object) stateIndex));
    this.previousBehaviourStateIndex = this.currentBehaviourStateIndex;
    this.currentBehaviourStateIndex = stateIndex;
    this.currentBehaviourState = this.enemyBehaviourStates[stateIndex];
    this.PlayAudioOfCurrentState();
    this.PlayAnimationOfCurrentState();
  }

  public void PlayAnimationOfCurrentState()
  {
    if ((UnityEngine.Object) this.creatureAnimator == (UnityEngine.Object) null)
      return;
    if (this.currentBehaviourState.IsAnimTrigger)
      this.creatureAnimator.SetTrigger(this.currentBehaviourState.parameterString);
    else
      this.creatureAnimator.SetBool(this.currentBehaviourState.parameterString, this.currentBehaviourState.boolValue);
  }

  public void PlayAudioOfCurrentState()
  {
    if ((bool) (UnityEngine.Object) this.creatureVoice)
    {
      if (this.currentBehaviourState.playOneShotVoice)
      {
        this.creatureVoice.PlayOneShot(this.currentBehaviourState.VoiceClip);
        WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.currentBehaviourState.VoiceClip, this.creatureVoice.volume);
      }
      else if ((UnityEngine.Object) this.currentBehaviourState.VoiceClip != (UnityEngine.Object) null)
      {
        this.creatureVoice.clip = this.currentBehaviourState.VoiceClip;
        this.creatureVoice.Play();
      }
    }
    if (!(bool) (UnityEngine.Object) this.creatureSFX)
      return;
    if (this.currentBehaviourState.playOneShotSFX)
    {
      this.creatureSFX.PlayOneShot(this.currentBehaviourState.SFXClip);
      WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.currentBehaviourState.SFXClip, this.creatureSFX.volume);
    }
    else
    {
      if (!((UnityEngine.Object) this.currentBehaviourState.SFXClip != (UnityEngine.Object) null))
        return;
      this.creatureSFX.clip = this.currentBehaviourState.SFXClip;
      this.creatureSFX.Play();
    }
  }

  public void SetMovingTowardsTargetPlayer(PlayerControllerB playerScript)
  {
    this.movingTowardsTargetPlayer = true;
    this.targetPlayer = playerScript;
  }

  public bool SetDestinationToPosition(Vector3 position, bool checkForPath = false)
  {
    if (checkForPath)
    {
      position = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 1.75f);
      this.path1 = new NavMeshPath();
      if (!this.agent.CalculatePath(position, this.path1) || (double) Vector3.Distance(this.path1.corners[this.path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, 2.7f)) > 1.5499999523162842)
        return false;
    }
    this.moveTowardsDestination = true;
    this.movingTowardsTargetPlayer = false;
    this.destination = RoundManager.Instance.GetNavMeshPosition(position, RoundManager.Instance.navHit, -1f);
    return true;
  }

  public virtual void DoAIInterval()
  {
    if (this.moveTowardsDestination)
      this.agent.SetDestination(this.destination);
    this.SyncPositionToClients();
  }

  public void SyncPositionToClients()
  {
    if ((double) Vector3.Distance(this.serverPosition, this.transform.position) <= (double) this.updatePositionThreshold)
      return;
    this.serverPosition = this.transform.position;
    if (this.IsServer)
      this.UpdateEnemyPositionClientRpc(this.serverPosition);
    else
      this.UpdateEnemyPositionServerRpc(this.serverPosition);
  }

  public PlayerControllerB CheckLineOfSightForPlayer(
    float width = 45f,
    int range = 60,
    int proximityAwareness = -1)
  {
    if (this.isOutside && !this.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
      range = Mathf.Clamp(range, 0, 30);
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      Vector3 position = StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position;
      if ((double) Vector3.Distance(position, this.eye.position) < (double) range && !Physics.Linecast(this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && ((double) Vector3.Angle(this.eye.forward, position - this.eye.position) < (double) width || proximityAwareness != -1 && (double) Vector3.Distance(this.eye.position, position) < (double) proximityAwareness))
        return StartOfRound.Instance.allPlayerScripts[index];
    }
    return (PlayerControllerB) null;
  }

  public PlayerControllerB CheckLineOfSightForClosestPlayer(
    float width = 45f,
    int range = 60,
    int proximityAwareness = -1,
    float bufferDistance = 0.0f)
  {
    if (this.isOutside && !this.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
      range = Mathf.Clamp(range, 0, 30);
    float num1 = 1000f;
    int index1 = -1;
    for (int index2 = 0; index2 < StartOfRound.Instance.allPlayerScripts.Length; ++index2)
    {
      Vector3 position = StartOfRound.Instance.allPlayerScripts[index2].gameplayCamera.transform.position;
      if (!Physics.Linecast(this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
      {
        Vector3 to = position - this.eye.position;
        float num2 = Vector3.Distance(this.eye.position, position);
        if (((double) Vector3.Angle(this.eye.forward, to) < (double) width || proximityAwareness != -1 && (double) num2 < (double) proximityAwareness) && (double) num2 < (double) num1)
        {
          num1 = num2;
          index1 = index2;
        }
      }
    }
    if ((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null && index1 != -1 && (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[index1] && (double) bufferDistance > 0.0 && (double) Mathf.Abs(num1 - Vector3.Distance(this.transform.position, this.targetPlayer.transform.position)) < (double) bufferDistance)
      return (PlayerControllerB) null;
    if (index1 < 0)
      return (PlayerControllerB) null;
    this.mostOptimalDistance = num1;
    return StartOfRound.Instance.allPlayerScripts[index1];
  }

  public PlayerControllerB[] GetAllPlayersInLineOfSight(
    float width = 45f,
    int range = 60,
    Transform eyeObject = null,
    float proximityCheck = -1f,
    int layerMask = -1)
  {
    if (layerMask == -1)
      layerMask = StartOfRound.Instance.collidersAndRoomMaskAndDefault;
    if ((UnityEngine.Object) eyeObject == (UnityEngine.Object) null)
      eyeObject = this.eye;
    if (this.isOutside && !this.enemyType.canSeeThroughFog && TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Foggy)
      range = Mathf.Clamp(range, 0, 30);
    List<PlayerControllerB> playerControllerBList = new List<PlayerControllerB>(4);
    for (int index = 0; index < StartOfRound.Instance.allPlayerScripts.Length; ++index)
    {
      if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index]))
      {
        Vector3 position = StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position;
        if ((double) Vector3.Distance(this.eye.position, position) < (double) range && !Physics.Linecast(eyeObject.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
          Vector3 to = position - eyeObject.position;
          if ((double) Vector3.Angle(eyeObject.forward, to) < (double) width || (double) Vector3.Distance(this.transform.position, StartOfRound.Instance.allPlayerScripts[index].transform.position) < (double) proximityCheck)
            playerControllerBList.Add(StartOfRound.Instance.allPlayerScripts[index]);
        }
      }
    }
    if (playerControllerBList.Count == 4)
      return StartOfRound.Instance.allPlayerScripts;
    return playerControllerBList.Count > 0 ? playerControllerBList.ToArray() : (PlayerControllerB[]) null;
  }

  public GameObject CheckLineOfSight(
    List<GameObject> objectsToLookFor,
    float width = 45f,
    int range = 60,
    float proximityAwareness = -1f)
  {
    for (int index = 0; index < objectsToLookFor.Count; ++index)
    {
      if ((UnityEngine.Object) objectsToLookFor[index] == (UnityEngine.Object) null)
      {
        objectsToLookFor.TrimExcess();
        Debug.Log((object) string.Format("size of objectsToLookFor after trimming: {0}", (object) objectsToLookFor.Count));
      }
      else
      {
        Vector3 position = objectsToLookFor[index].transform.position;
        if (!this.isOutside)
        {
          if ((double) position.y > -80.0)
            continue;
        }
        else if ((double) position.y < -100.0)
          continue;
        Physics.Linecast(this.eye.position, position, out RaycastHit _, StartOfRound.Instance.collidersAndRoomMaskAndDefault);
        if ((double) Vector3.Distance(this.eye.position, objectsToLookFor[index].transform.position) < (double) range && !Physics.Linecast(this.eye.position, position, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && ((double) Vector3.Angle(this.eye.forward, position - this.eye.position) < (double) width || (double) Vector3.Distance(this.transform.position, position) < (double) proximityAwareness))
          return objectsToLookFor[index];
      }
    }
    return (GameObject) null;
  }

  public bool HasLineOfSightToPosition(
    Vector3 pos,
    float width = 45f,
    int range = 60,
    float proximityAwareness = -1f)
  {
    if ((UnityEngine.Object) this.eye == (UnityEngine.Object) null)
    {
      Transform transform = this.transform;
    }
    else
    {
      Transform eye = this.eye;
    }
    return (double) Vector3.Distance(this.eye.position, pos) < (double) range && !Physics.Linecast(this.eye.position, pos, StartOfRound.Instance.collidersAndRoomMaskAndDefault) && ((double) Vector3.Angle(this.eye.forward, pos - this.eye.position) < (double) width || (double) Vector3.Distance(this.transform.position, pos) < (double) proximityAwareness);
  }

  public void StartSearch(Vector3 startOfSearch, AISearchRoutine newSearch = null)
  {
    this.StopSearch(this.currentSearch);
    this.movingTowardsTargetPlayer = false;
    if (newSearch == null)
    {
      this.currentSearch = new AISearchRoutine();
      newSearch = this.currentSearch;
    }
    else
      this.currentSearch = newSearch;
    this.currentSearch.currentSearchStartPosition = startOfSearch;
    if (this.currentSearch.unsearchedNodes.Count <= 0)
      this.currentSearch.unsearchedNodes = ((IEnumerable<GameObject>) this.allAINodes).ToList<GameObject>();
    this.searchRoutineRandom = new System.Random(this.RoundUpToNearestFive(startOfSearch.x) + this.RoundUpToNearestFive(startOfSearch.z));
    this.searchCoroutine = this.StartCoroutine(this.CurrentSearchCoroutine());
    this.currentSearch.inProgress = true;
  }

  private int RoundUpToNearestFive(float x) => (int) ((double) x / 5.0) * 5;

  public void StopSearch(AISearchRoutine search, bool clear = true)
  {
    if (search == null)
      return;
    if (this.searchCoroutine != null)
      this.StopCoroutine(this.searchCoroutine);
    if (this.chooseTargetNodeCoroutine != null)
      this.StopCoroutine(this.chooseTargetNodeCoroutine);
    search.calculatingNodeInSearch = false;
    search.inProgress = false;
    if (!clear)
      return;
    search.unsearchedNodes = ((IEnumerable<GameObject>) this.allAINodes).ToList<GameObject>();
    search.timesFinishingSearch = 0;
    search.nodesEliminatedInCurrentSearch = 0;
    search.currentTargetNode = (GameObject) null;
    search.currentSearchStartPosition = Vector3.zero;
    search.nextTargetNode = (GameObject) null;
    search.choseTargetNode = false;
  }

  private IEnumerator CurrentSearchCoroutine()
  {
    EnemyAI enemyAi = this;
    yield return (object) null;
    while (enemyAi.searchCoroutine != null && enemyAi.IsOwner)
    {
      yield return (object) null;
      if (enemyAi.currentSearch.unsearchedNodes.Count <= 0)
      {
        enemyAi.FinishedCurrentSearchRoutine();
        if (!enemyAi.currentSearch.loopSearch)
        {
          enemyAi.currentSearch.inProgress = false;
          enemyAi.searchCoroutine = (Coroutine) null;
          yield break;
        }
        else
        {
          enemyAi.currentSearch.unsearchedNodes = ((IEnumerable<GameObject>) enemyAi.allAINodes).ToList<GameObject>();
          ++enemyAi.currentSearch.timesFinishingSearch;
          enemyAi.currentSearch.nodesEliminatedInCurrentSearch = 0;
          yield return (object) new WaitForSeconds(1f);
        }
      }
      if (enemyAi.currentSearch.choseTargetNode && enemyAi.currentSearch.unsearchedNodes.Contains(enemyAi.currentSearch.nextTargetNode))
      {
        if (enemyAi.debugEnemyAI)
          Debug.Log((object) string.Format("finding next node: {0}; node already found ahead of time", (object) enemyAi.currentSearch.choseTargetNode));
        enemyAi.currentSearch.currentTargetNode = enemyAi.currentSearch.nextTargetNode;
      }
      else
      {
        if (enemyAi.debugEnemyAI)
          Debug.Log((object) "finding next node; calculation not finished ahead of time");
        enemyAi.currentSearch.waitingForTargetNode = true;
        enemyAi.StartCalculatingNextTargetNode();
        // ISSUE: reference to a compiler-generated method
        yield return (object) new WaitUntil(new Func<bool>(enemyAi.\u003CCurrentSearchCoroutine\u003Eb__88_0));
      }
      enemyAi.currentSearch.waitingForTargetNode = false;
      if (enemyAi.currentSearch.unsearchedNodes.Count > 0 && !((UnityEngine.Object) enemyAi.currentSearch.currentTargetNode == (UnityEngine.Object) null))
      {
        if (enemyAi.debugEnemyAI)
        {
          int num = 0;
          for (int index = 0; index < enemyAi.currentSearch.unsearchedNodes.Count; ++index)
          {
            if ((UnityEngine.Object) enemyAi.currentSearch.unsearchedNodes[index] == (UnityEngine.Object) enemyAi.currentSearch.currentTargetNode)
            {
              Debug.Log((object) string.Format("Found node {0} within list of unsearched nodes at index {1}", (object) enemyAi.currentSearch.unsearchedNodes[index], (object) index));
              ++num;
            }
          }
          Debug.Log((object) string.Format("Copies of the node {0} found in list: {1}", (object) enemyAi.currentSearch.currentTargetNode, (object) num));
          Debug.Log((object) string.Format("unsearched nodes contains {0}? : {1}", (object) enemyAi.currentSearch.currentTargetNode, (object) enemyAi.currentSearch.unsearchedNodes.Contains(enemyAi.currentSearch.currentTargetNode)));
          Debug.Log((object) string.Format("Removing {0} from unsearched nodes list with Remove()", (object) enemyAi.currentSearch.currentTargetNode));
        }
        enemyAi.currentSearch.unsearchedNodes.Remove(enemyAi.currentSearch.currentTargetNode);
        if (enemyAi.debugEnemyAI)
          Debug.Log((object) string.Format("Removed. Does list now contain {0}?: {1}", (object) enemyAi.currentSearch.currentTargetNode, (object) enemyAi.currentSearch.unsearchedNodes.Contains(enemyAi.currentSearch.currentTargetNode)));
        enemyAi.SetDestinationToPosition(enemyAi.currentSearch.currentTargetNode.transform.position);
        for (int i = enemyAi.currentSearch.unsearchedNodes.Count - 1; i >= 0; --i)
        {
          if ((double) Vector3.Distance(enemyAi.currentSearch.currentTargetNode.transform.position, enemyAi.currentSearch.unsearchedNodes[i].transform.position) < (double) enemyAi.currentSearch.searchPrecision)
            enemyAi.EliminateNodeFromSearch(i);
          if (i % 10 == 0)
            yield return (object) null;
        }
        enemyAi.StartCalculatingNextTargetNode();
        int timeSpent = 0;
        while (enemyAi.searchCoroutine != null)
        {
          if (enemyAi.debugEnemyAI)
            Debug.Log((object) "Current search not null");
          ++timeSpent;
          if (timeSpent < 32)
          {
            yield return (object) new WaitForSeconds(0.5f);
            if ((double) Vector3.Distance(enemyAi.transform.position, enemyAi.currentSearch.currentTargetNode.transform.position) < (double) enemyAi.currentSearch.searchPrecision)
            {
              if (enemyAi.debugEnemyAI)
                Debug.Log((object) ("Enemy: Reached the target " + enemyAi.currentSearch.currentTargetNode.name));
              enemyAi.ReachedNodeInSearch();
              break;
            }
            if (enemyAi.debugEnemyAI)
              Debug.Log((object) string.Format("Enemy: We have not reached the target node {0}, distance: {1} ; {2}", (object) enemyAi.currentSearch.currentTargetNode.transform.name, (object) Vector3.Distance(enemyAi.transform.position, enemyAi.currentSearch.currentTargetNode.transform.position), (object) enemyAi.currentSearch.searchPrecision));
          }
          else
            break;
        }
        if (enemyAi.debugEnemyAI)
          Debug.Log((object) "Reached destination node");
      }
    }
    if (!enemyAi.IsOwner)
      enemyAi.StopSearch(enemyAi.currentSearch);
  }

  private void StartCalculatingNextTargetNode()
  {
    if (this.debugEnemyAI)
    {
      Debug.Log((object) "Calculating next target node");
      Debug.Log((object) string.Format("Is calculate node coroutine null? : {0}; choseTargetNode: {1}", (object) (this.chooseTargetNodeCoroutine == null), (object) this.currentSearch.choseTargetNode));
    }
    if (this.chooseTargetNodeCoroutine == null)
    {
      if (this.debugEnemyAI)
        Debug.Log((object) "NODE A");
      this.currentSearch.choseTargetNode = false;
      this.chooseTargetNodeCoroutine = this.StartCoroutine(this.ChooseNextNodeInSearchRoutine());
    }
    else
    {
      if (this.currentSearch.calculatingNodeInSearch)
        return;
      if (this.debugEnemyAI)
        Debug.Log((object) "NODE B");
      this.currentSearch.choseTargetNode = false;
      this.currentSearch.calculatingNodeInSearch = true;
      this.StopCoroutine(this.chooseTargetNodeCoroutine);
      this.chooseTargetNodeCoroutine = this.StartCoroutine(this.ChooseNextNodeInSearchRoutine());
    }
  }

  private IEnumerator ChooseNextNodeInSearchRoutine()
  {
    EnemyAI enemyAi = this;
    yield return (object) null;
    float closestDist = 500f;
    bool gotNode = false;
    GameObject chosenNode = (GameObject) null;
    int num = 0;
    while (num < enemyAi.currentSearch.unsearchedNodes.Count)
      ++num;
    for (int i = enemyAi.currentSearch.unsearchedNodes.Count - 1; i >= 0; --i)
    {
      if (!enemyAi.IsOwner)
      {
        enemyAi.currentSearch.calculatingNodeInSearch = false;
        yield break;
      }
      else
      {
        if (i % 5 == 0)
          yield return (object) null;
        if ((double) Vector3.Distance(enemyAi.currentSearch.currentSearchStartPosition, enemyAi.currentSearch.unsearchedNodes[i].transform.position) > (double) enemyAi.currentSearch.searchWidth)
          enemyAi.EliminateNodeFromSearch(i);
        else if (enemyAi.PathIsIntersectedByLineOfSight(enemyAi.currentSearch.unsearchedNodes[i].transform.position, true, false))
          enemyAi.EliminateNodeFromSearch(i);
        else if ((double) enemyAi.pathDistance < (double) closestDist && (!enemyAi.currentSearch.randomized || !gotNode || enemyAi.searchRoutineRandom.Next(0, 100) < 65))
        {
          closestDist = enemyAi.pathDistance;
          chosenNode = enemyAi.currentSearch.unsearchedNodes[i];
          gotNode = true;
        }
      }
    }
    if (enemyAi.debugEnemyAI)
      Debug.Log((object) string.Format("NODE C; chosen node: {0}", (object) chosenNode));
    if (enemyAi.currentSearch.waitingForTargetNode)
    {
      enemyAi.currentSearch.currentTargetNode = chosenNode;
      if (enemyAi.debugEnemyAI)
        Debug.Log((object) "NODE C1");
    }
    else
    {
      enemyAi.currentSearch.nextTargetNode = chosenNode;
      if (enemyAi.debugEnemyAI)
        Debug.Log((object) "NODE C2");
    }
    enemyAi.currentSearch.choseTargetNode = true;
    if (enemyAi.debugEnemyAI)
      Debug.Log((object) string.Format("Chose target node?: {0} ", (object) enemyAi.currentSearch.choseTargetNode));
    enemyAi.currentSearch.calculatingNodeInSearch = false;
    enemyAi.chooseTargetNodeCoroutine = (Coroutine) null;
  }

  public virtual void ReachedNodeInSearch()
  {
  }

  private void EliminateNodeFromSearch(GameObject node)
  {
    this.currentSearch.unsearchedNodes.Remove(node);
    ++this.currentSearch.nodesEliminatedInCurrentSearch;
  }

  private void EliminateNodeFromSearch(int index)
  {
    this.currentSearch.unsearchedNodes.RemoveAt(index);
    ++this.currentSearch.nodesEliminatedInCurrentSearch;
  }

  public virtual void FinishedCurrentSearchRoutine()
  {
  }

  public bool TargetClosestPlayer(float bufferDistance = 1.5f, bool requireLineOfSight = false, float viewWidth = 70f)
  {
    this.mostOptimalDistance = 2000f;
    PlayerControllerB targetPlayer = this.targetPlayer;
    this.targetPlayer = (PlayerControllerB) null;
    for (int index = 0; index < StartOfRound.Instance.connectedPlayersAmount + 1; ++index)
    {
      if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index]) && !this.PathIsIntersectedByLineOfSight(StartOfRound.Instance.allPlayerScripts[index].transform.position, avoidLineOfSight: false) && (!requireLineOfSight || this.HasLineOfSightToPosition(StartOfRound.Instance.allPlayerScripts[index].gameplayCamera.transform.position, viewWidth, 40)))
      {
        this.tempDist = Vector3.Distance(this.transform.position, StartOfRound.Instance.allPlayerScripts[index].transform.position);
        if ((double) this.tempDist < (double) this.mostOptimalDistance)
        {
          this.mostOptimalDistance = this.tempDist;
          this.targetPlayer = StartOfRound.Instance.allPlayerScripts[index];
        }
      }
    }
    if ((UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null && (double) bufferDistance > 0.0 && (UnityEngine.Object) targetPlayer != (UnityEngine.Object) null && (double) Mathf.Abs(this.mostOptimalDistance - Vector3.Distance(this.transform.position, targetPlayer.transform.position)) < (double) bufferDistance)
      this.targetPlayer = targetPlayer;
    return (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null;
  }

  public PlayerControllerB GetClosestPlayer(
    bool requireLineOfSight = false,
    bool cannotBeInShip = false,
    bool cannotBeNearShip = false)
  {
    PlayerControllerB closestPlayer = (PlayerControllerB) null;
    this.mostOptimalDistance = 2000f;
    for (int index1 = 0; index1 < 4; ++index1)
    {
      if (this.PlayerIsTargetable(StartOfRound.Instance.allPlayerScripts[index1], cannotBeInShip))
      {
        if (cannotBeNearShip)
        {
          if (!StartOfRound.Instance.allPlayerScripts[index1].isInElevator)
          {
            bool flag = false;
            for (int index2 = 0; index2 < RoundManager.Instance.spawnDenialPoints.Length; ++index2)
            {
              if ((double) Vector3.Distance(RoundManager.Instance.spawnDenialPoints[index2].transform.position, StartOfRound.Instance.allPlayerScripts[index1].transform.position) < 10.0)
              {
                flag = true;
                break;
              }
            }
            if (flag)
              continue;
          }
          else
            continue;
        }
        if (!requireLineOfSight || !Physics.Linecast(this.transform.position, StartOfRound.Instance.allPlayerScripts[index1].transform.position, 256))
        {
          this.tempDist = Vector3.Distance(this.transform.position, StartOfRound.Instance.allPlayerScripts[index1].transform.position);
          if ((double) this.tempDist < (double) this.mostOptimalDistance)
          {
            this.mostOptimalDistance = this.tempDist;
            closestPlayer = StartOfRound.Instance.allPlayerScripts[index1];
          }
        }
      }
    }
    return closestPlayer;
  }

  public bool PlayerIsTargetable(
    PlayerControllerB playerScript,
    bool cannotBeInShip = false,
    bool overrideInsideFactoryCheck = false)
  {
    if (cannotBeInShip && playerScript.isInHangarShipRoom || !playerScript.isPlayerControlled || playerScript.isPlayerDead || !((UnityEngine.Object) playerScript.inAnimationWithEnemy == (UnityEngine.Object) null) || !overrideInsideFactoryCheck && playerScript.isInsideFactory == this.isOutside || (double) playerScript.sinkingValue >= 0.73000001907348633)
      return false;
    return !this.isOutside || !StartOfRound.Instance.hangarDoorsClosed || playerScript.isInHangarShipRoom == this.isInsidePlayerShip;
  }

  public Transform ChooseFarthestNodeFromPosition(
    Vector3 pos,
    bool avoidLineOfSight = false,
    int offset = 0,
    bool log = false)
  {
    this.nodesTempArray = ((IEnumerable<GameObject>) this.allAINodes).OrderByDescending<GameObject, float>((Func<GameObject, float>) (x => Vector3.Distance(pos, x.transform.position))).ToArray<GameObject>();
    Transform transform = this.nodesTempArray[0].transform;
    for (int index = 0; index < this.nodesTempArray.Length; ++index)
    {
      if (!this.PathIsIntersectedByLineOfSight(this.nodesTempArray[index].transform.position, avoidLineOfSight: avoidLineOfSight))
      {
        this.mostOptimalDistance = Vector3.Distance(pos, this.nodesTempArray[index].transform.position);
        transform = this.nodesTempArray[index].transform;
        if (offset != 0 && index < this.nodesTempArray.Length - 1)
          --offset;
        else
          break;
      }
    }
    return transform;
  }

  public Transform ChooseClosestNodeToPosition(Vector3 pos, bool avoidLineOfSight = false, int offset = 0)
  {
    this.nodesTempArray = ((IEnumerable<GameObject>) this.allAINodes).OrderBy<GameObject, float>((Func<GameObject, float>) (x => Vector3.Distance(pos, x.transform.position))).ToArray<GameObject>();
    Transform transform = this.nodesTempArray[0].transform;
    for (int index = 0; index < this.nodesTempArray.Length; ++index)
    {
      if (!this.PathIsIntersectedByLineOfSight(this.nodesTempArray[index].transform.position, avoidLineOfSight: avoidLineOfSight))
      {
        this.mostOptimalDistance = Vector3.Distance(pos, this.nodesTempArray[index].transform.position);
        transform = this.nodesTempArray[index].transform;
        if (offset != 0 && index < this.nodesTempArray.Length - 1)
          --offset;
        else
          break;
      }
    }
    return transform;
  }

  public bool PathIsIntersectedByLineOfSight(
    Vector3 targetPos,
    bool calculatePathDistance = false,
    bool avoidLineOfSight = true)
  {
    this.pathDistance = 0.0f;
    if (!this.agent.CalculatePath(targetPos, this.path1))
      return true;
    if (this.DebugEnemy)
    {
      for (int index = 1; index < this.path1.corners.Length; ++index)
        Debug.DrawLine(this.path1.corners[index - 1], this.path1.corners[index], Color.red);
    }
    if ((double) Vector3.Distance(this.path1.corners[this.path1.corners.Length - 1], RoundManager.Instance.GetNavMeshPosition(targetPos, RoundManager.Instance.navHit, 2.7f)) > 1.5)
      return true;
    if (calculatePathDistance)
    {
      for (int index = 1; index < this.path1.corners.Length; ++index)
      {
        this.pathDistance += Vector3.Distance(this.path1.corners[index - 1], this.path1.corners[index]);
        if (avoidLineOfSight && Physics.Linecast(this.path1.corners[index - 1], this.path1.corners[index], 262144))
          return true;
      }
    }
    else if (avoidLineOfSight)
    {
      for (int index = 1; index < this.path1.corners.Length; ++index)
      {
        Debug.DrawLine(this.path1.corners[index - 1], this.path1.corners[index], Color.green);
        if (Physics.Linecast(this.path1.corners[index - 1], this.path1.corners[index], 262144))
          return true;
      }
    }
    return false;
  }

  public virtual void Update()
  {
    if (this.enemyType.isDaytimeEnemy && !this.daytimeEnemyLeaving)
      this.CheckTimeOfDayToLeave();
    if (this.stunnedIndefinitely <= 0)
    {
      if ((double) this.stunNormalizedTimer >= 0.0)
      {
        this.stunNormalizedTimer -= Time.deltaTime / this.enemyType.stunTimeMultiplier;
      }
      else
      {
        this.stunnedByPlayer = (PlayerControllerB) null;
        if ((double) this.postStunInvincibilityTimer >= 0.0)
          this.postStunInvincibilityTimer -= Time.deltaTime * 5f;
      }
    }
    if (!this.ventAnimationFinished && (double) this.timeSinceSpawn < (double) this.exitVentAnimationTime + 0.004999999888241291 * (double) RoundManager.Instance.numberOfEnemiesInScene)
    {
      this.timeSinceSpawn += Time.deltaTime;
      if (!this.IsOwner)
      {
        Vector3 serverPosition = this.serverPosition;
        if (!(this.serverPosition != Vector3.zero))
          return;
        this.transform.position = this.serverPosition;
        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.targetYRotation, this.transform.eulerAngles.z);
      }
      else if ((double) this.updateDestinationInterval >= 0.0)
      {
        this.updateDestinationInterval -= Time.deltaTime;
      }
      else
      {
        this.SyncPositionToClients();
        this.updateDestinationInterval = 0.1f;
      }
    }
    else
    {
      if (!this.ventAnimationFinished)
      {
        this.ventAnimationFinished = true;
        if ((UnityEngine.Object) this.creatureAnimator != (UnityEngine.Object) null)
          this.creatureAnimator.SetBool("inSpawningAnimation", false);
      }
      if (!this.IsOwner)
      {
        if (this.currentSearch.inProgress)
          this.StopSearch(this.currentSearch);
        this.SetClientCalculatingAI(false);
        if (!this.inSpecialAnimation)
        {
          this.transform.position = Vector3.SmoothDamp(this.transform.position, this.serverPosition, ref this.tempVelocity, this.syncMovementSpeed);
          this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, Mathf.LerpAngle(this.transform.eulerAngles.y, this.targetYRotation, 15f * Time.deltaTime), this.transform.eulerAngles.z);
        }
        this.timeSinceSpawn += Time.deltaTime;
      }
      else if (this.isEnemyDead)
      {
        this.SetClientCalculatingAI(false);
      }
      else
      {
        if (!this.inSpecialAnimation)
          this.SetClientCalculatingAI(true);
        if (this.movingTowardsTargetPlayer && (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null)
        {
          if ((double) this.setDestinationToPlayerInterval <= 0.0)
          {
            this.setDestinationToPlayerInterval = 0.25f;
            this.destination = RoundManager.Instance.GetNavMeshPosition(this.targetPlayer.transform.position, RoundManager.Instance.navHit, 2.7f);
          }
          else
          {
            this.destination = new Vector3(this.targetPlayer.transform.position.x, this.destination.y, this.targetPlayer.transform.position.z);
            this.setDestinationToPlayerInterval -= Time.deltaTime;
          }
          if ((double) this.addPlayerVelocityToDestination > 0.0)
          {
            if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
              this.destination += Vector3.Normalize(this.targetPlayer.thisController.velocity * 100f) * this.addPlayerVelocityToDestination;
            else if ((double) this.targetPlayer.timeSincePlayerMoving < 0.25)
              this.destination += Vector3.Normalize((this.targetPlayer.serverPlayerPosition - this.targetPlayer.oldPlayerPosition) * 100f) * this.addPlayerVelocityToDestination;
          }
        }
        if (this.inSpecialAnimation)
          return;
        if ((double) this.updateDestinationInterval >= 0.0)
        {
          this.updateDestinationInterval -= Time.deltaTime;
        }
        else
        {
          this.DoAIInterval();
          this.updateDestinationInterval = this.AIIntervalTime;
        }
        if ((double) Mathf.Abs(this.previousYRotation - this.transform.eulerAngles.y) <= 6.0)
          return;
        this.previousYRotation = this.transform.eulerAngles.y;
        this.targetYRotation = this.previousYRotation;
        if (this.IsServer)
          this.UpdateEnemyRotationClientRpc((short) this.previousYRotation);
        else
          this.UpdateEnemyRotationServerRpc((short) this.previousYRotation);
      }
    }
  }

  public void KillEnemyOnOwnerClient(bool overrideDestroy = false)
  {
    if (!this.enemyType.canDie || !this.IsOwner)
      return;
    bool flag = this.enemyType.destroyOnDeath;
    if (overrideDestroy)
      flag = true;
    if (this.isEnemyDead)
      return;
    Debug.Log((object) string.Format("Kill enemy called! destroy: {0}", (object) flag));
    if (flag)
    {
      if (this.IsServer)
      {
        Debug.Log((object) "Kill enemy called on server, destroy true");
        this.KillEnemy(true);
      }
      else
        this.KillEnemyServerRpc(true);
    }
    else
    {
      this.KillEnemy();
      if (!this.NetworkObject.IsSpawned)
        return;
      this.KillEnemyServerRpc(false);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void KillEnemyServerRpc(bool destroy)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1810146992U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in destroy, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 1810146992U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Kill enemy server rpc called with destroy {0}", (object) destroy));
    if (destroy)
      this.KillEnemy(destroy);
    else
      this.KillEnemyClientRpc(destroy);
  }

  [ClientRpc]
  public void KillEnemyClientRpc(bool destroy)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1614111717U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in destroy, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1614111717U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("Kill enemy client rpc called; {0}", (object) destroy));
    if (this.isEnemyDead)
      return;
    this.KillEnemy(destroy);
  }

  public virtual void KillEnemy(bool destroy = false)
  {
    Debug.Log((object) string.Format("Kill enemy called; destroy: {0}", (object) destroy));
    if (destroy)
    {
      if (!this.IsServer)
        return;
      Debug.Log((object) "Despawn network object in kill enemy called!");
      if (!this.thisNetworkObject.IsSpawned)
        return;
      this.thisNetworkObject.Despawn();
    }
    else
    {
      ScanNodeProperties componentInChildren = this.gameObject.GetComponentInChildren<ScanNodeProperties>();
      if ((UnityEngine.Object) componentInChildren != (UnityEngine.Object) null && (bool) (UnityEngine.Object) componentInChildren.gameObject.GetComponent<Collider>())
        componentInChildren.gameObject.GetComponent<Collider>().enabled = false;
      this.isEnemyDead = true;
      if ((UnityEngine.Object) this.creatureVoice != (UnityEngine.Object) null)
        this.creatureVoice.PlayOneShot(this.dieSFX);
      try
      {
        if ((UnityEngine.Object) this.creatureAnimator != (UnityEngine.Object) null)
        {
          this.creatureAnimator.SetBool("Stunned", false);
          this.creatureAnimator.SetBool("stunned", false);
          this.creatureAnimator.SetBool("stun", false);
          this.creatureAnimator.SetTrigger(nameof (KillEnemy));
          this.creatureAnimator.SetBool("Dead", true);
        }
      }
      catch (Exception ex)
      {
        Debug.LogError((object) string.Format("enemy did not have bool in animator in KillEnemy, error returned; {0}", (object) ex));
      }
      this.CancelSpecialAnimationWithPlayer();
      this.SubtractFromPowerLevel();
      this.agent.enabled = false;
    }
  }

  public virtual void CancelSpecialAnimationWithPlayer()
  {
    if ((bool) (UnityEngine.Object) this.inSpecialAnimationWithPlayer)
    {
      this.inSpecialAnimationWithPlayer.inSpecialInteractAnimation = false;
      this.inSpecialAnimationWithPlayer.snapToServerPosition = false;
      this.inSpecialAnimationWithPlayer.inAnimationWithEnemy = (EnemyAI) null;
      this.inSpecialAnimationWithPlayer = (PlayerControllerB) null;
    }
    this.inSpecialAnimation = false;
  }

  public override void OnDestroy()
  {
    base.OnDestroy();
    if (RoundManager.Instance.SpawnedEnemies.Contains(this))
      RoundManager.Instance.SpawnedEnemies.Remove(this);
    this.SubtractFromPowerLevel();
    this.CancelSpecialAnimationWithPlayer();
    if (this.searchCoroutine != null)
      this.StopCoroutine(this.searchCoroutine);
    if (this.chooseTargetNodeCoroutine == null)
      return;
    this.StopCoroutine(this.chooseTargetNodeCoroutine);
  }

  private void SubtractFromPowerLevel()
  {
    if (this.removedPowerLevel)
      return;
    this.removedPowerLevel = true;
    if (this.enemyType.isDaytimeEnemy)
      RoundManager.Instance.currentDaytimeEnemyPower = Mathf.Max(RoundManager.Instance.currentDaytimeEnemyPower - this.enemyType.PowerLevel, 0);
    else if (this.isOutside)
    {
      RoundManager.Instance.currentOutsideEnemyPower = Mathf.Max(RoundManager.Instance.currentOutsideEnemyPower - this.enemyType.PowerLevel, 0);
    }
    else
    {
      RoundManager.Instance.cannotSpawnMoreInsideEnemies = false;
      RoundManager.Instance.currentEnemyPower = Mathf.Max(RoundManager.Instance.currentEnemyPower - this.enemyType.PowerLevel, 0);
    }
  }

  [ServerRpc]
  private void UpdateEnemyRotationServerRpc(short rotationY)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3079913705U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, rotationY);
      this.__endSendServerRpc(ref bufferWriter, 3079913705U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UpdateEnemyRotationClientRpc(rotationY);
  }

  [ClientRpc]
  private void UpdateEnemyRotationClientRpc(short rotationY)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1258118513U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, rotationY);
      this.__endSendClientRpc(ref bufferWriter, 1258118513U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.previousYRotation = this.transform.eulerAngles.y;
    this.targetYRotation = (float) rotationY;
  }

  [ServerRpc]
  private void UpdateEnemyPositionServerRpc(Vector3 newPos)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(255411420U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newPos);
      this.__endSendServerRpc(ref bufferWriter, 255411420U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.UpdateEnemyPositionClientRpc(newPos);
  }

  [ClientRpc]
  private void UpdateEnemyPositionClientRpc(Vector3 newPos)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(4287979896U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in newPos);
      this.__endSendClientRpc(ref bufferWriter, 4287979896U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner)
      return;
    this.serverPosition = newPos;
    this.OnSyncPositionFromServer(newPos);
  }

  public virtual void OnSyncPositionFromServer(Vector3 newPos)
  {
  }

  public virtual void OnDrawGizmos()
  {
    if (!this.IsOwner || !this.debugEnemyAI)
      return;
    Gizmos.DrawSphere(this.destination, 0.5f);
    Gizmos.DrawLine(this.transform.position, this.destination);
  }

  public void ChangeOwnershipOfEnemy(ulong newOwnerClientId)
  {
    int index;
    if (StartOfRound.Instance.ClientPlayerList.TryGetValue(newOwnerClientId, out index))
    {
      Debug.Log((object) string.Format("Switching ownership of {0} #{1} to player #{2} ({3})", (object) this.enemyType.name, (object) this.thisEnemyIndex, (object) index, (object) StartOfRound.Instance.allPlayerScripts[index].playerUsername));
      if (this.currentOwnershipOnThisClient == index)
      {
        Debug.Log((object) string.Format("unable to set owner of {0} #{1} to player #{2}; reason B", (object) this.enemyType.name, (object) this.thisEnemyIndex, (object) index));
      }
      else
      {
        ulong ownerClientId = this.gameObject.GetComponent<NetworkObject>().OwnerClientId;
        if ((long) ownerClientId == (long) newOwnerClientId)
        {
          Debug.Log((object) string.Format("unable to set owner of {0} #{1} to player #{2} with id {3}; current ownerclientId: {4}", (object) this.enemyType.name, (object) this.thisEnemyIndex, (object) index, (object) newOwnerClientId, (object) ownerClientId));
        }
        else
        {
          this.currentOwnershipOnThisClient = index;
          if (!this.IsServer)
          {
            this.ChangeEnemyOwnerServerRpc(newOwnerClientId);
          }
          else
          {
            this.thisNetworkObject.ChangeOwnership(newOwnerClientId);
            this.ChangeEnemyOwnerServerRpc(newOwnerClientId);
          }
        }
      }
    }
    else
      Debug.LogError((object) string.Format("Attempted to switch ownership of enemy {0} to a player which does not have a link between client id and player object. Attempted clientId: {1}", (object) this.gameObject.name, (object) newOwnerClientId));
  }

  [ServerRpc(RequireOwnership = false)]
  public void ChangeEnemyOwnerServerRpc(ulong clientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3587030867U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendServerRpc(ref bufferWriter, 3587030867U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if ((long) this.gameObject.GetComponent<NetworkObject>().OwnerClientId != (long) clientId)
      this.thisNetworkObject.ChangeOwnership(clientId);
    int playerVal;
    if (!StartOfRound.Instance.ClientPlayerList.TryGetValue(clientId, out playerVal))
      return;
    this.ChangeEnemyOwnerClientRpc(playerVal);
  }

  [ClientRpc]
  public void ChangeEnemyOwnerClientRpc(int playerVal)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(245785831U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerVal);
      this.__endSendClientRpc(ref bufferWriter, 245785831U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.currentOwnershipOnThisClient = playerVal;
  }

  public void SetClientCalculatingAI(bool enable)
  {
    this.isClientCalculatingAI = enable;
    this.agent.enabled = enable;
  }

  public virtual void EnableEnemyMesh(bool enable, bool overrideDoNotSet = false)
  {
    int num = !enable ? 23 : 19;
    for (int index = 0; index < this.skinnedMeshRenderers.Length; ++index)
    {
      if (!this.skinnedMeshRenderers[index].CompareTag("DoNotSet") || overrideDoNotSet)
        this.skinnedMeshRenderers[index].gameObject.layer = num;
    }
    for (int index = 0; index < this.meshRenderers.Length; ++index)
    {
      if (!this.meshRenderers[index].CompareTag("DoNotSet") || overrideDoNotSet)
        this.meshRenderers[index].gameObject.layer = num;
    }
  }

  public virtual void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
  }

  public void HitEnemyOnLocalClient(
    int force = 1,
    Vector3 hitDirection = default (Vector3),
    PlayerControllerB playerWhoHit = null,
    bool playHitSFX = false)
  {
    Debug.Log((object) string.Format("Local client hit enemy {0} with force of {1}.", (object) this.agent.transform.name, (object) force));
    int playerWhoHit1 = -1;
    if ((UnityEngine.Object) playerWhoHit != (UnityEngine.Object) null)
    {
      playerWhoHit1 = (int) playerWhoHit.playerClientId;
      this.HitEnemy(force, playerWhoHit, playHitSFX);
    }
    this.HitEnemyServerRpc(force, playerWhoHit1, playHitSFX);
  }

  [ServerRpc(RequireOwnership = false)]
  public void HitEnemyServerRpc(int force, int playerWhoHit, bool playHitSFX)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2814283679U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, force);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoHit);
      bufferWriter.WriteValueSafe<bool>(in playHitSFX, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 2814283679U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.HitEnemyClientRpc(force, playerWhoHit, playHitSFX);
  }

  [ClientRpc]
  public void HitEnemyClientRpc(int force, int playerWhoHit, bool playHitSFX)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(217059478U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, force);
      BytePacker.WriteValueBitPacked(bufferWriter, playerWhoHit);
      bufferWriter.WriteValueSafe<bool>(in playHitSFX, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 217059478U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || playerWhoHit == (int) GameNetworkManager.Instance.localPlayerController.playerClientId)
      return;
    if (playerWhoHit == -1)
      this.HitEnemy(force, playHitSFX: playHitSFX);
    else
      this.HitEnemy(force, StartOfRound.Instance.allPlayerScripts[playerWhoHit], playHitSFX);
  }

  public virtual void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    if (playHitSFX && (UnityEngine.Object) this.enemyType.hitBodySFX != (UnityEngine.Object) null)
    {
      this.creatureSFX.PlayOneShot(this.enemyType.hitBodySFX);
      WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.hitBodySFX);
    }
    if ((UnityEngine.Object) this.creatureVoice != (UnityEngine.Object) null)
      this.creatureVoice.PlayOneShot(this.enemyType.hitEnemyVoiceSFX);
    if (this.debugEnemyAI)
      Debug.Log((object) string.Format("Enemy #{0} was hit with force of {1}", (object) this.thisEnemyIndex, (object) force));
    if (!((UnityEngine.Object) playerWhoHit != (UnityEngine.Object) null))
      return;
    Debug.Log((object) string.Format("Client #{0} hit enemy {1} with force of {2}.", (object) playerWhoHit.playerClientId, (object) this.agent.transform.name, (object) force));
  }

  private void CheckTimeOfDayToLeave()
  {
    if ((UnityEngine.Object) TimeOfDay.Instance == (UnityEngine.Object) null || (double) TimeOfDay.Instance.normalizedTimeOfDay <= (double) this.enemyType.normalizedTimeInDayToLeave)
      return;
    this.daytimeEnemyLeaving = true;
    this.DaytimeEnemyLeave();
  }

  public virtual void DaytimeEnemyLeave()
  {
    if (!this.debugEnemyAI)
      return;
    Debug.Log((object) (this.gameObject.name + ": Daytime enemy leave function called"));
  }

  public void LogEnemyError(string error)
  {
    Debug.LogError((object) string.Format("{0} #{1}: {2}", (object) this.enemyType.name, (object) this.thisEnemyIndex, (object) error));
  }

  public virtual void AnimationEventA()
  {
  }

  public virtual void AnimationEventB()
  {
  }

  public virtual void ShipTeleportEnemy()
  {
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_EnemyAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2081148948U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_2081148948)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2962895088U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_2962895088)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1810146992U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_1810146992)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1614111717U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_1614111717)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3079913705U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_3079913705)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1258118513U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_1258118513)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(255411420U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_255411420)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4287979896U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_4287979896)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3587030867U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_3587030867)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(245785831U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_245785831)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2814283679U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_2814283679)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(217059478U, new NetworkManager.RpcReceiveHandler(EnemyAI.__rpc_handler_217059478)));
  }

  private static void __rpc_handler_2081148948(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int stateIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out stateIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((EnemyAI) target).SwitchToBehaviourServerRpc(stateIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2962895088(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int stateIndex;
    ByteUnpacker.ReadValueBitPacked(reader, out stateIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).SwitchToBehaviourClientRpc(stateIndex);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1810146992(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool destroy;
    reader.ReadValueSafe<bool>(out destroy, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((EnemyAI) target).KillEnemyServerRpc(destroy);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1614111717(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool destroy;
    reader.ReadValueSafe<bool>(out destroy, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).KillEnemyClientRpc(destroy);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3079913705(
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
      short rotationY;
      ByteUnpacker.ReadValueBitPacked(reader, out rotationY);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((EnemyAI) target).UpdateEnemyRotationServerRpc(rotationY);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1258118513(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    short rotationY;
    ByteUnpacker.ReadValueBitPacked(reader, out rotationY);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).UpdateEnemyRotationClientRpc(rotationY);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_255411420(
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
      Vector3 newPos;
      reader.ReadValueSafe(out newPos);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((EnemyAI) target).UpdateEnemyPositionServerRpc(newPos);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_4287979896(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 newPos;
    reader.ReadValueSafe(out newPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).UpdateEnemyPositionClientRpc(newPos);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3587030867(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong clientId;
    ByteUnpacker.ReadValueBitPacked(reader, out clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((EnemyAI) target).ChangeEnemyOwnerServerRpc(clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_245785831(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerVal;
    ByteUnpacker.ReadValueBitPacked(reader, out playerVal);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).ChangeEnemyOwnerClientRpc(playerVal);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2814283679(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int force;
    ByteUnpacker.ReadValueBitPacked(reader, out force);
    int playerWhoHit;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoHit);
    bool playHitSFX;
    reader.ReadValueSafe<bool>(out playHitSFX, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((EnemyAI) target).HitEnemyServerRpc(force, playerWhoHit, playHitSFX);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_217059478(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int force;
    ByteUnpacker.ReadValueBitPacked(reader, out force);
    int playerWhoHit;
    ByteUnpacker.ReadValueBitPacked(reader, out playerWhoHit);
    bool playHitSFX;
    reader.ReadValueSafe<bool>(out playHitSFX, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((EnemyAI) target).HitEnemyClientRpc(force, playerWhoHit, playHitSFX);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (EnemyAI);
}
