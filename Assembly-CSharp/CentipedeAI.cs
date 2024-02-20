// Decompiled with JetBrains decompiler
// Type: CentipedeAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

#nullable disable
public class CentipedeAI : EnemyAI
{
  public PlayerControllerB clingingToPlayer;
  public AudioClip fallShriek;
  public AudioClip hitGroundSFX;
  public AudioClip hitCentipede;
  public AudioClip[] shriekClips;
  private int offsetNodeAmount = 6;
  private Vector3 mainEntrancePosition;
  public AnimationCurve fallToGroundCurve;
  public Vector3 ceilingHidingPoint;
  private RaycastHit rayHit;
  public Transform tempTransform;
  private Ray ray;
  private bool clingingToCeiling;
  private Coroutine ceilingAnimationCoroutine;
  private bool startedCeilingAnimationCoroutine;
  private Coroutine killAnimationCoroutine;
  private Vector3 propelVelocity = Vector3.zero;
  private float damagePlayerInterval;
  private bool clingingToLocalClient;
  private bool clingingToDeadBody;
  private bool inDroppingOffPlayerAnim;
  private Vector3 firstKilledPlayerPosition = Vector3.zero;
  private bool pathToFirstKilledBodyIsClear = true;
  private bool syncedPositionInPrepForCeilingAnimation;
  public Transform modelContainer;
  private float updateOffsetPositionInterval;
  private Vector3 offsetTargetPos;
  private bool triggeredFall;
  public AudioSource clingingToPlayer2DAudio;
  public AudioClip clingToPlayer3D;
  private float chaseTimer;
  private float stuckTimer;
  private Coroutine beginClingingToCeilingCoroutine;
  private Coroutine dropFromCeilingCoroutine;
  private bool singlePlayerSecondChanceGiven;
  private bool choseHidingSpotNoPlayersNearby;

  public override void Start()
  {
    this.mainEntrancePosition = RoundManager.FindMainEntrancePosition();
    this.offsetTargetPos = this.transform.position;
    base.Start();
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.livingPlayers == 0 || this.isEnemyDead)
      return;
    if (this.currentBehaviourStateIndex == 0 && this.firstKilledPlayerPosition != Vector3.zero && this.pathToFirstKilledBodyIsClear && (double) Vector3.Distance(this.transform.position, this.firstKilledPlayerPosition) < 13.0)
    {
      this.choseHidingSpotNoPlayersNearby = false;
      this.ChooseHidingSpotNearPlayer(this.firstKilledPlayerPosition, true);
    }
    else if (!this.TargetClosestPlayer())
    {
      if (!this.choseHidingSpotNoPlayersNearby)
      {
        this.choseHidingSpotNoPlayersNearby = true;
        this.SetDestinationToNode(this.ChooseFarthestNodeFromPosition(this.mainEntrancePosition, offset: (this.allAINodes.Length / 2 + this.thisEnemyIndex) % this.allAINodes.Length, log: true));
      }
      else if (this.PathIsIntersectedByLineOfSight(this.destination, avoidLineOfSight: false))
        this.choseHidingSpotNoPlayersNearby = false;
      if (this.currentBehaviourStateIndex != 2)
        return;
      this.SwitchToBehaviourState(0);
    }
    else
    {
      this.choseHidingSpotNoPlayersNearby = false;
      if (this.currentBehaviourStateIndex == 0)
      {
        this.ChooseHidingSpotNearPlayer(this.targetPlayer.transform.position);
      }
      else
      {
        if (this.currentBehaviourStateIndex != 2)
          return;
        this.movingTowardsTargetPlayer = true;
      }
    }
  }

  public void ChooseHidingSpotNearPlayer(
    Vector3 targetPos,
    bool targetingPositionOfFirstKilledPlayer = false)
  {
    this.movingTowardsTargetPlayer = false;
    if ((Object) this.targetNode != (Object) null)
    {
      if (!this.PathIsIntersectedByLineOfSight(this.targetNode.position))
      {
        this.SetDestinationToNode(this.targetNode);
        return;
      }
      if (targetingPositionOfFirstKilledPlayer)
        this.pathToFirstKilledBodyIsClear = false;
    }
    int num = (this.offsetNodeAmount + this.thisEnemyIndex) % this.allAINodes.Length;
    if (targetingPositionOfFirstKilledPlayer)
      Random.Range(0, 3);
    Transform position = this.ChooseClosestNodeToPosition(targetPos, true, this.offsetNodeAmount);
    if ((Object) position != (Object) null)
      this.SetDestinationToNode(position);
    else if (targetingPositionOfFirstKilledPlayer)
      this.pathToFirstKilledBodyIsClear = false;
    else
      this.SetDestinationToNode(this.ChooseClosestNodeToPosition(this.transform.position));
  }

  private void SetDestinationToNode(Transform moveTowardsNode)
  {
    this.targetNode = moveTowardsNode;
    this.SetDestinationToPosition(this.targetNode.position);
  }

  private void LateUpdate()
  {
    if (this.isEnemyDead)
      this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, 0.0f);
    else if ((Object) this.clingingToPlayer == (Object) null)
    {
      if (!this.clingingToCeiling)
      {
        if ((double) this.updateOffsetPositionInterval <= 0.0)
        {
          this.offsetTargetPos = RoundManager.Instance.RandomlyOffsetPosition(this.transform.position, 1.5f);
          this.updateOffsetPositionInterval = 0.04f;
        }
        else
        {
          this.modelContainer.position = Vector3.Lerp(this.modelContainer.position, this.offsetTargetPos, 3f * Time.deltaTime);
          this.updateOffsetPositionInterval -= Time.deltaTime;
        }
      }
      else
        this.modelContainer.localPosition = Vector3.zero;
    }
    else
    {
      this.modelContainer.localPosition = Vector3.zero;
      if (this.clingingToDeadBody && (Object) this.clingingToPlayer.deadBody != (Object) null)
      {
        this.transform.position = this.clingingToPlayer.deadBody.bodyParts[0].transform.position;
        this.transform.eulerAngles = this.clingingToPlayer.deadBody.bodyParts[0].transform.eulerAngles;
      }
      else
        this.UpdatePositionToClingingPlayerHead();
    }
  }

  private void UpdatePositionToClingingPlayerHead()
  {
    if (this.clingingToLocalClient)
    {
      this.transform.position = this.clingingToPlayer.gameplayCamera.transform.position;
      this.transform.eulerAngles = this.clingingToPlayer.gameplayCamera.transform.eulerAngles;
    }
    else
    {
      this.transform.position = this.clingingToPlayer.playerGlobalHead.position + this.clingingToPlayer.playerGlobalHead.up * 0.38f;
      this.transform.eulerAngles = this.clingingToPlayer.playerGlobalHead.eulerAngles;
    }
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.IsOwner)
        {
          this.IncreaseSpeedSlowly(10f);
          this.movingTowardsTargetPlayer = false;
          if ((Object) this.targetNode != (Object) null)
          {
            this.tempTransform.position = new Vector3(this.targetNode.position.x, this.transform.position.y, this.targetNode.position.z);
            float num = Vector3.Distance(this.transform.position, this.tempTransform.position);
            if ((double) num < 0.30000001192092896 && !Physics.Linecast(this.transform.position, this.targetNode.position, 256))
              this.RaycastToCeiling();
            else if ((double) num < 2.5 && !this.syncedPositionInPrepForCeilingAnimation)
            {
              this.syncedPositionInPrepForCeilingAnimation = true;
              this.SyncPositionToClients();
            }
          }
          if ((double) this.agent.velocity.sqrMagnitude < 1.0 / 500.0)
          {
            this.stuckTimer += Time.deltaTime;
            if ((double) this.stuckTimer > 4.0)
            {
              this.stuckTimer = 0.0f;
              ++this.offsetNodeAmount;
              this.targetNode = (Transform) null;
            }
          }
        }
        this.chaseTimer = 0.0f;
        break;
      case 1:
        if (!this.clingingToCeiling)
        {
          if (this.startedCeilingAnimationCoroutine || this.ceilingAnimationCoroutine != null)
            break;
          this.startedCeilingAnimationCoroutine = true;
          this.ceilingAnimationCoroutine = this.StartCoroutine(this.clingToCeiling());
          break;
        }
        this.transform.position = Vector3.SmoothDamp(this.transform.position, this.ceilingHidingPoint, ref this.propelVelocity, 0.1f);
        this.ray = new Ray(this.transform.position, Vector3.down);
        if (!Physics.SphereCast(this.ray, 2.15f, out this.rayHit, 20f, StartOfRound.Instance.playersMask) || !((Object) this.rayHit.transform == (Object) GameNetworkManager.Instance.localPlayerController.transform) || (bool) (Object) this.clingingToPlayer || Physics.Linecast(this.rayHit.transform.position, this.transform.position, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore) || this.triggeredFall)
          break;
        this.triggeredFall = true;
        this.TriggerCentipedeFallServerRpc(NetworkManager.Singleton.LocalClientId);
        break;
      case 2:
        this.triggeredFall = false;
        if (this.clingingToCeiling)
        {
          if (this.startedCeilingAnimationCoroutine || this.ceilingAnimationCoroutine != null)
            break;
          this.startedCeilingAnimationCoroutine = true;
          this.ceilingAnimationCoroutine = this.StartCoroutine(this.fallFromCeiling());
          break;
        }
        if (!this.IsOwner)
          break;
        this.IncreaseSpeedSlowly();
        this.chaseTimer += Time.deltaTime;
        if ((double) this.chaseTimer <= 10.0)
          break;
        this.chaseTimer = 0.0f;
        this.SwitchToBehaviourState(0);
        break;
      case 3:
        if (!((Object) this.clingingToPlayer != (Object) null))
          break;
        if (this.IsOwner && !this.clingingToPlayer.isInsideFactory && !this.clingingToPlayer.isPlayerDead)
        {
          this.KillEnemyOnOwnerClient();
          break;
        }
        if (this.clingingToLocalClient)
        {
          GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
          this.DamagePlayerOnIntervals();
        }
        else if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position, 60f, 12))
          GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.6f, 0.9f);
        if (!((Object) this.clingingToPlayer != (Object) null) || !this.clingingToPlayer.isPlayerDead || this.inDroppingOffPlayerAnim)
          break;
        this.inDroppingOffPlayerAnim = true;
        this.StopClingingToPlayer(true);
        break;
    }
  }

  private void DamagePlayerOnIntervals()
  {
    if ((double) this.damagePlayerInterval <= 0.0 && !this.inDroppingOffPlayerAnim)
    {
      if ((double) this.stunNormalizedTimer > 0.0 || StartOfRound.Instance.connectedPlayersAmount <= 0 && this.clingingToPlayer.health <= 15 && !this.singlePlayerSecondChanceGiven)
      {
        this.singlePlayerSecondChanceGiven = true;
        this.inDroppingOffPlayerAnim = true;
        this.StopClingingServerRpc(false);
      }
      else
      {
        this.clingingToPlayer.DamagePlayer(10, causeOfDeath: CauseOfDeath.Suffocation);
        this.damagePlayerInterval = 2f;
      }
    }
    else
      this.damagePlayerInterval -= Time.deltaTime;
  }

  private void IncreaseSpeedSlowly(float increaseSpeed = 1.5f)
  {
    if ((double) this.stunNormalizedTimer > 0.0)
    {
      this.creatureAnimator.SetBool("stunned", true);
      this.agent.speed = 0.0f;
    }
    else
    {
      this.creatureAnimator.SetBool("stunned", false);
      this.agent.speed = Mathf.Clamp(this.agent.speed + Time.deltaTime * 1.5f, 0.0f, 5.5f);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void StopClingingServerRpc(bool playerDead)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4105250505U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in playerDead, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 4105250505U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.StopClingingClientRpc(playerDead);
  }

  [ClientRpc]
  public void StopClingingClientRpc(bool playerDead)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1106241822U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in playerDead, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1106241822U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.inDroppingOffPlayerAnim = true;
    this.StopClingingToPlayer(playerDead);
  }

  private void OnEnable()
  {
    StartOfRound.Instance.playerTeleportedEvent.AddListener(new UnityAction<PlayerControllerB>(this.OnPlayerTeleport));
  }

  private void OnDisable()
  {
    StartOfRound.Instance.playerTeleportedEvent.RemoveListener(new UnityAction<PlayerControllerB>(this.OnPlayerTeleport));
  }

  private void OnPlayerTeleport(PlayerControllerB playerTeleported)
  {
    if (!((Object) this.clingingToPlayer == (Object) playerTeleported) || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  private void StopClingingToPlayer(bool playerDead)
  {
    if ((Object) this.clingingToPlayer.currentVoiceChatAudioSource == (Object) null)
      StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
    if ((Object) this.clingingToPlayer.currentVoiceChatAudioSource != (Object) null)
    {
      this.clingingToPlayer.currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = 1f;
      OccludeAudio component = this.clingingToPlayer.currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
      component.overridingLowPass = false;
      component.lowPassOverride = 20000f;
      this.clingingToPlayer.voiceMuffledByEnemy = false;
    }
    if (this.clingingToLocalClient)
      this.clingingToPlayer2DAudio.Stop();
    else
      this.creatureSFX.Stop();
    this.clingingToLocalClient = false;
    if (this.killAnimationCoroutine != null)
      this.StopCoroutine(this.killAnimationCoroutine);
    this.killAnimationCoroutine = this.StartCoroutine(this.UnclingFromPlayer(this.clingingToPlayer, playerDead));
  }

  private IEnumerator UnclingFromPlayer(PlayerControllerB playerBeingKilled, bool playerDead = true)
  {
    CentipedeAI centipedeAi = this;
    if (playerDead)
    {
      centipedeAi.clingingToDeadBody = true;
      yield return (object) new WaitForSeconds(1.5f);
      centipedeAi.clingingToDeadBody = false;
    }
    centipedeAi.clingingToPlayer = (PlayerControllerB) null;
    centipedeAi.creatureAnimator.SetBool("clingingToPlayer", false);
    centipedeAi.ray = new Ray(centipedeAi.transform.position, Vector3.down);
    Vector3 groundPosition = centipedeAi.transform.position;
    Vector3 startPosition = centipedeAi.transform.position;
    groundPosition = !Physics.Raycast(centipedeAi.ray, out centipedeAi.rayHit, 40f, 256) ? RoundManager.Instance.GetNavMeshPosition(centipedeAi.transform.position) : centipedeAi.rayHit.point;
    float fallTime = 0.2f;
    while ((double) fallTime < 1.0)
    {
      yield return (object) null;
      fallTime += Time.deltaTime * 4f;
      centipedeAi.transform.position = Vector3.Lerp(startPosition, groundPosition, centipedeAi.fallToGroundCurve.Evaluate(fallTime));
    }
    if (centipedeAi.IsOwner)
      centipedeAi.agent.speed = 0.0f;
    else
      centipedeAi.transform.eulerAngles = new Vector3(0.0f, centipedeAi.transform.eulerAngles.y, 0.0f);
    centipedeAi.serverPosition = centipedeAi.transform.position;
    centipedeAi.inSpecialAnimation = false;
    centipedeAi.inDroppingOffPlayerAnim = false;
    centipedeAi.SwitchToBehaviourStateOnLocalClient(0);
    if (playerDead)
    {
      centipedeAi.firstKilledPlayerPosition = centipedeAi.transform.position;
      centipedeAi.pathToFirstKilledBodyIsClear = true;
    }
    centipedeAi.movingTowardsTargetPlayer = false;
    centipedeAi.targetNode = (Transform) null;
  }

  public override void CancelSpecialAnimationWithPlayer()
  {
    base.CancelSpecialAnimationWithPlayer();
    int num = this.IsOwner ? 1 : 0;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.stunNormalizedTimer >= 0.0 || this.currentBehaviourStateIndex != 2 || (Object) this.clingingToPlayer != (Object) null)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((Object) playerControllerB != (Object) null))
      return;
    this.clingingToPlayer = playerControllerB;
    this.ClingToPlayerServerRpc(playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void ClingToPlayerServerRpc(ulong playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2791977891U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendServerRpc(ref bufferWriter, 2791977891U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ClingToPlayerClientRpc(playerObjectId);
  }

  [ClientRpc]
  public void ClingToPlayerClientRpc(ulong playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2474017466U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 2474017466U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.ClingToPlayer(StartOfRound.Instance.allPlayerScripts[playerObjectId]);
  }

  private void ClingToPlayer(PlayerControllerB playerScript)
  {
    if (this.ceilingAnimationCoroutine != null)
    {
      this.StopCoroutine(this.ceilingAnimationCoroutine);
      this.ceilingAnimationCoroutine = (Coroutine) null;
    }
    this.startedCeilingAnimationCoroutine = false;
    this.clingingToCeiling = false;
    this.clingingToLocalClient = (Object) playerScript == (Object) GameNetworkManager.Instance.localPlayerController;
    this.clingingToPlayer = playerScript;
    this.inSpecialAnimation = true;
    this.agent.enabled = false;
    playerScript.DropAllHeldItems();
    this.creatureAnimator.SetBool("clingingToPlayer", true);
    if ((Object) this.clingingToPlayer.currentVoiceChatAudioSource == (Object) null)
      StartOfRound.Instance.RefreshPlayerVoicePlaybackObjects();
    if ((Object) this.clingingToPlayer.currentVoiceChatAudioSource != (Object) null)
    {
      this.clingingToPlayer.currentVoiceChatAudioSource.GetComponent<AudioLowPassFilter>().lowpassResonanceQ = 5f;
      OccludeAudio component = this.clingingToPlayer.currentVoiceChatAudioSource.GetComponent<OccludeAudio>();
      component.overridingLowPass = true;
      component.lowPassOverride = 500f;
      this.clingingToPlayer.voiceMuffledByEnemy = true;
    }
    if (this.clingingToLocalClient)
    {
      this.clingingToPlayer2DAudio.Play();
      GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
    }
    else
    {
      this.creatureSFX.clip = this.clingToPlayer3D;
      this.creatureSFX.Play();
    }
    this.inDroppingOffPlayerAnim = false;
    this.SwitchToBehaviourStateOnLocalClient(3);
  }

  private IEnumerator fallFromCeiling()
  {
    CentipedeAI centipedeAi = this;
    centipedeAi.targetNode = (Transform) null;
    Vector3 startPosition = centipedeAi.transform.position;
    Vector3 groundPosition = centipedeAi.transform.position;
    centipedeAi.ray = new Ray(centipedeAi.transform.position, Vector3.down);
    if (Physics.Raycast(centipedeAi.ray, out centipedeAi.rayHit, 20f, 268435712))
    {
      groundPosition = centipedeAi.rayHit.point;
    }
    else
    {
      Debug.LogError((object) "Centipede: I could not get a raycast to the ground after falling from the ceiling! Choosing the closest nav mesh position to self.");
      startPosition = RoundManager.Instance.GetNavMeshPosition(centipedeAi.ray.GetPoint(4f), sampleRadius: 7f);
      if (centipedeAi.IsOwner && !RoundManager.Instance.GotNavMeshPositionResult)
        centipedeAi.KillEnemyOnOwnerClient(true);
    }
    float fallTime = 0.0f;
    while ((double) fallTime < 1.0)
    {
      yield return (object) null;
      fallTime += Time.deltaTime * 2.5f;
      centipedeAi.transform.position = Vector3.Lerp(startPosition, groundPosition, centipedeAi.fallToGroundCurve.Evaluate(fallTime));
    }
    centipedeAi.creatureSFX.PlayOneShot(centipedeAi.hitGroundSFX);
    float distToPlayer = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, centipedeAi.transform.position);
    if ((double) distToPlayer < 13.0)
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
    centipedeAi.serverPosition = centipedeAi.transform.position;
    if (centipedeAi.IsOwner)
      centipedeAi.agent.speed = 0.0f;
    else
      centipedeAi.transform.eulerAngles = new Vector3(0.0f, centipedeAi.transform.eulerAngles.y, 0.0f);
    centipedeAi.clingingToCeiling = false;
    centipedeAi.inSpecialAnimation = false;
    yield return (object) new WaitForSeconds(0.5f);
    RoundManager.PlayRandomClip(centipedeAi.creatureSFX, centipedeAi.shriekClips);
    if ((double) distToPlayer < 7.0)
      GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.5f);
    centipedeAi.ceilingAnimationCoroutine = (Coroutine) null;
    centipedeAi.startedCeilingAnimationCoroutine = false;
  }

  [ServerRpc(RequireOwnership = false)]
  public void TriggerCentipedeFallServerRpc(ulong clientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1047857261U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendServerRpc(ref bufferWriter, 1047857261U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.thisNetworkObject.ChangeOwnership(clientId);
    this.SwitchToBehaviourClientRpc(2);
  }

  private IEnumerator clingToCeiling()
  {
    CentipedeAI centipedeAi = this;
    yield return (object) new WaitForSeconds(0.52f);
    if (centipedeAi.currentBehaviourStateIndex != 1)
    {
      centipedeAi.clingingToCeiling = false;
      centipedeAi.startedCeilingAnimationCoroutine = false;
    }
    else
    {
      centipedeAi.clingingToCeiling = true;
      centipedeAi.ceilingAnimationCoroutine = (Coroutine) null;
      centipedeAi.startedCeilingAnimationCoroutine = false;
    }
  }

  private void RaycastToCeiling()
  {
    this.ray = new Ray(this.transform.position, Vector3.up);
    if (Physics.Raycast(this.ray, out this.rayHit, 20f, 256))
    {
      this.ceilingHidingPoint = this.ray.GetPoint(this.rayHit.distance - 0.8f);
      this.ceilingHidingPoint = RoundManager.Instance.RandomlyOffsetPosition(this.ceilingHidingPoint, 2.25f);
      this.SwitchToBehaviourStateOnLocalClient(1);
      this.syncedPositionInPrepForCeilingAnimation = false;
      this.inSpecialAnimation = true;
      this.agent.enabled = false;
      this.SwitchToHidingOnCeilingServerRpc(this.ceilingHidingPoint);
    }
    else
    {
      ++this.offsetNodeAmount;
      this.targetNode = (Transform) null;
      Debug.LogError((object) "Centipede: Raycast to ceiling failed. Setting different node offset and resuming search for a hiding spot.");
    }
  }

  [ServerRpc]
  public void SwitchToHidingOnCeilingServerRpc(Vector3 ceilingPoint)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2005305321U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in ceilingPoint);
      this.__endSendServerRpc(ref bufferWriter, 2005305321U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SwitchToHidingOnCeilingClientRpc(ceilingPoint);
  }

  [ClientRpc]
  public void SwitchToHidingOnCeilingClientRpc(Vector3 ceilingPoint)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2626887057U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in ceilingPoint);
      this.__endSendClientRpc(ref bufferWriter, 2626887057U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.syncedPositionInPrepForCeilingAnimation = false;
    this.inSpecialAnimation = true;
    this.agent.enabled = false;
    this.ceilingHidingPoint = ceilingPoint;
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    this.creatureSFX.PlayOneShot(this.hitCentipede);
    this.StartCoroutine(this.delayedShriek());
    if (this.IsServer)
      this.ReactBehaviourToBeingHurt();
    this.enemyHP -= force;
    if (this.enemyHP > 0 || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  public override void SetEnemyStunned(
    bool setToStunned,
    float setToStunTime = 1f,
    PlayerControllerB setStunnedByPlayer = null)
  {
    base.SetEnemyStunned(setToStunned, setToStunTime);
    if (!this.IsServer)
      return;
    this.ReactBehaviourToBeingHurt();
  }

  public void ReactBehaviourToBeingHurt()
  {
    switch (this.currentBehaviourStateIndex)
    {
      case 2:
        this.GetHitAndRunAwayServerRpc();
        this.targetNode = (Transform) null;
        break;
      case 3:
        if (this.inDroppingOffPlayerAnim)
          break;
        this.inDroppingOffPlayerAnim = true;
        this.StopClingingServerRpc(false);
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void GetHitAndRunAwayServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3824648183U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3824648183U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.GetHitAndRunAwayClientRpc();
  }

  [ClientRpc]
  public void GetHitAndRunAwayClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2602771441U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2602771441U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.SwitchToBehaviourStateOnLocalClient(0);
    this.targetNode = (Transform) null;
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy();
    this.agent.enabled = false;
    if ((Object) this.clingingToPlayer != (Object) null)
    {
      this.UpdatePositionToClingingPlayerHead();
      this.StopClingingToPlayer(false);
    }
    if (this.clingingToCeiling && this.ceilingAnimationCoroutine == null)
      this.ceilingAnimationCoroutine = this.StartCoroutine(this.fallFromCeiling());
    this.modelContainer.localPosition = Vector3.zero;
  }

  private IEnumerator delayedShriek()
  {
    // ISSUE: reference to a compiler-generated field
    int num = this.\u003C\u003E1__state;
    CentipedeAI centipedeAi = this;
    if (num != 0)
    {
      if (num != 1)
        return false;
      // ISSUE: reference to a compiler-generated field
      this.\u003C\u003E1__state = -1;
      centipedeAi.creatureVoice.pitch = 1.7f;
      RoundManager.PlayRandomClip(centipedeAi.creatureVoice, centipedeAi.shriekClips, false);
      return false;
    }
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = -1;
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E2__current = (object) new WaitForSeconds(0.2f);
    // ISSUE: reference to a compiler-generated field
    this.\u003C\u003E1__state = 1;
    return true;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_CentipedeAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4105250505U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_4105250505)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1106241822U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_1106241822)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2791977891U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_2791977891)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2474017466U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_2474017466)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1047857261U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_1047857261)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2005305321U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_2005305321)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2626887057U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_2626887057)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3824648183U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_3824648183)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2602771441U, new NetworkManager.RpcReceiveHandler(CentipedeAI.__rpc_handler_2602771441)));
  }

  private static void __rpc_handler_4105250505(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool playerDead;
    reader.ReadValueSafe<bool>(out playerDead, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CentipedeAI) target).StopClingingServerRpc(playerDead);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1106241822(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool playerDead;
    reader.ReadValueSafe<bool>(out playerDead, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CentipedeAI) target).StopClingingClientRpc(playerDead);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2791977891(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CentipedeAI) target).ClingToPlayerServerRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2474017466(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    ulong playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CentipedeAI) target).ClingToPlayerClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1047857261(
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
    ((CentipedeAI) target).TriggerCentipedeFallServerRpc(clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2005305321(
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
      Vector3 ceilingPoint;
      reader.ReadValueSafe(out ceilingPoint);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((CentipedeAI) target).SwitchToHidingOnCeilingServerRpc(ceilingPoint);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2626887057(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 ceilingPoint;
    reader.ReadValueSafe(out ceilingPoint);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CentipedeAI) target).SwitchToHidingOnCeilingClientRpc(ceilingPoint);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3824648183(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CentipedeAI) target).GetHitAndRunAwayServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2602771441(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CentipedeAI) target).GetHitAndRunAwayClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (CentipedeAI);
}
