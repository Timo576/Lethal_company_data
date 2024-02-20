// Decompiled with JetBrains decompiler
// Type: CrawlerAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class CrawlerAI : EnemyAI
{
  public AISearchRoutine searchForPlayers;
  private float checkLineOfSightInterval;
  public float maxSearchAndRoamRadius = 100f;
  [Space(5f)]
  public float noticePlayerTimer;
  private bool hasEnteredChaseMode;
  private bool lostPlayerInChase;
  private bool beginningChasingThisClient;
  private Collider[] nearPlayerColliders;
  public AudioClip shortRoar;
  public AudioClip[] hitWallSFX;
  public AudioClip bitePlayerSFX;
  private Vector3 previousPosition;
  private float previousVelocity;
  private float averageVelocity;
  private float velocityInterval;
  private float velocityAverageCount;
  private float wallCollisionSFXDebounce;
  private float timeSinceHittingPlayer;
  private bool ateTargetPlayerBody;
  private Coroutine eatPlayerBodyCoroutine;
  public Transform mouthTarget;
  public AudioClip eatPlayerSFX;
  public AudioClip[] hitCrawlerSFX;
  public AudioClip[] longRoarSFX;
  public DeadBodyInfo currentlyHeldBody;
  private bool pullingSecondLimb;
  private float agentSpeedWithNegative;
  private Vector3 lastPositionOfSeenPlayer;
  [Space(5f)]
  public float BaseAcceleration = 55f;
  public float SpeedAccelerationEffect = 2f;
  public float SpeedIncreaseRate = 5f;
  private float lastTimeHit;

  public override void Start()
  {
    base.Start();
    this.nearPlayerColliders = new Collider[4];
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.livingPlayers == 0 || this.isEnemyDead)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.searchForPlayers.inProgress)
          break;
        this.StartSearch(this.transform.position, this.searchForPlayers);
        Debug.Log((object) string.Format("Crawler: Started new search; is searching?: {0}", (object) this.searchForPlayers.inProgress));
        break;
      case 1:
        this.CheckForVeryClosePlayer();
        if (this.lostPlayerInChase)
        {
          this.movingTowardsTargetPlayer = false;
          if (this.searchForPlayers.inProgress)
            break;
          this.searchForPlayers.searchWidth = 30f;
          this.StartSearch(this.lastPositionOfSeenPlayer, this.searchForPlayers);
          Debug.Log((object) "Crawler: Lost player in chase; beginning search where the player was last seen");
          break;
        }
        if (!this.searchForPlayers.inProgress)
          break;
        this.StopSearch(this.searchForPlayers);
        this.movingTowardsTargetPlayer = true;
        Debug.Log((object) "Crawler: Found player during chase; stopping search coroutine and moving after target player");
        break;
    }
  }

  public override void FinishedCurrentSearchRoutine()
  {
    base.FinishedCurrentSearchRoutine();
    this.searchForPlayers.searchWidth = Mathf.Clamp(this.searchForPlayers.searchWidth + 10f, 1f, this.maxSearchAndRoamRadius);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    if (!this.IsOwner)
      this.inSpecialAnimation = false;
    this.CalculateAgentSpeed();
    this.timeSinceHittingPlayer += Time.deltaTime;
    if (GameNetworkManager.Instance.localPlayerController.HasLineOfSightToPosition(this.transform.position + Vector3.up * 0.25f, 80f, 25, 5f))
    {
      if (this.currentBehaviourStateIndex == 1)
        GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f);
      else
        GameNetworkManager.Instance.localPlayerController.IncreaseFearLevelOverTime(0.8f, 0.5f);
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.hasEnteredChaseMode)
        {
          this.hasEnteredChaseMode = false;
          this.searchForPlayers.searchWidth = 25f;
          this.beginningChasingThisClient = false;
          this.noticePlayerTimer = 0.0f;
          this.useSecondaryAudiosOnAnimatedObjects = false;
          this.openDoorSpeedMultiplier = 0.6f;
          this.agent.stoppingDistance = 0.0f;
          this.agent.speed = 7f;
        }
        if ((double) this.checkLineOfSightInterval <= 0.05000000074505806)
        {
          this.checkLineOfSightInterval += Time.deltaTime;
          break;
        }
        this.checkLineOfSightInterval = 0.0f;
        PlayerControllerB playerControllerB1;
        if ((UnityEngine.Object) this.stunnedByPlayer != (UnityEngine.Object) null)
        {
          playerControllerB1 = this.stunnedByPlayer;
          this.noticePlayerTimer = 1f;
        }
        else
          playerControllerB1 = this.CheckLineOfSightForPlayer(55f);
        if ((UnityEngine.Object) playerControllerB1 == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController)
        {
          this.noticePlayerTimer = Mathf.Clamp(this.noticePlayerTimer + 0.05f, 0.0f, 10f);
          if ((double) this.noticePlayerTimer <= 0.20000000298023224 || this.beginningChasingThisClient)
            break;
          this.beginningChasingThisClient = true;
          this.BeginChasingPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
          this.ChangeOwnershipOfEnemy(playerControllerB1.actualClientId);
          Debug.Log((object) "Begin chasing on local client");
          break;
        }
        this.noticePlayerTimer -= Time.deltaTime;
        break;
      case 1:
        if (!this.hasEnteredChaseMode)
        {
          this.hasEnteredChaseMode = true;
          this.lostPlayerInChase = false;
          this.checkLineOfSightInterval = 0.0f;
          this.noticePlayerTimer = 0.0f;
          this.beginningChasingThisClient = false;
          this.useSecondaryAudiosOnAnimatedObjects = true;
          this.openDoorSpeedMultiplier = 1.5f;
          this.agent.stoppingDistance = 0.5f;
          this.agent.speed = 0.0f;
        }
        if (!this.IsOwner || (double) this.stunNormalizedTimer > 0.0)
          break;
        if ((double) this.checkLineOfSightInterval <= 0.075000002980232239)
        {
          this.checkLineOfSightInterval += Time.deltaTime;
          break;
        }
        this.checkLineOfSightInterval = 0.0f;
        if (!this.ateTargetPlayerBody && (UnityEngine.Object) this.targetPlayer != (UnityEngine.Object) null && (UnityEngine.Object) this.targetPlayer.deadBody != (UnityEngine.Object) null && (UnityEngine.Object) this.targetPlayer.deadBody.grabBodyObject != (UnityEngine.Object) null && this.targetPlayer.deadBody.grabBodyObject.grabbableToEnemies && this.eatPlayerBodyCoroutine == null && (double) Vector3.Distance(this.transform.position, this.targetPlayer.deadBody.bodyParts[0].transform.position) < 3.2999999523162842)
        {
          Debug.Log((object) "Crawler: Eat player body start");
          this.ateTargetPlayerBody = true;
          this.inSpecialAnimation = true;
          this.eatPlayerBodyCoroutine = this.StartCoroutine(this.EatPlayerBodyAnimation((int) this.targetPlayer.playerClientId));
          this.EatPlayerBodyServerRpc((int) this.targetPlayer.playerClientId);
        }
        if (this.inSpecialAnimation)
          break;
        if (this.lostPlayerInChase)
        {
          PlayerControllerB playerScript = this.CheckLineOfSightForPlayer(55f);
          if ((bool) (UnityEngine.Object) playerScript)
          {
            this.noticePlayerTimer = 0.0f;
            this.lostPlayerInChase = false;
            this.MakeScreechNoiseServerRpc();
            if (!((UnityEngine.Object) playerScript != (UnityEngine.Object) this.targetPlayer))
              break;
            this.SetMovingTowardsTargetPlayer(playerScript);
            this.ateTargetPlayerBody = false;
            this.ChangeOwnershipOfEnemy(playerScript.actualClientId);
            break;
          }
          this.noticePlayerTimer -= 0.075f;
          if ((double) this.noticePlayerTimer >= -15.0)
            break;
          this.SwitchToBehaviourState(0);
          break;
        }
        PlayerControllerB playerControllerB2 = this.CheckLineOfSightForPlayer(65f, 80);
        if ((UnityEngine.Object) playerControllerB2 != (UnityEngine.Object) null)
        {
          this.noticePlayerTimer = 0.0f;
          this.lastPositionOfSeenPlayer = playerControllerB2.transform.position;
          if (!((UnityEngine.Object) playerControllerB2 != (UnityEngine.Object) this.targetPlayer))
            break;
          this.targetPlayer = playerControllerB2;
          this.ateTargetPlayerBody = false;
          this.ChangeOwnershipOfEnemy(this.targetPlayer.actualClientId);
          break;
        }
        this.noticePlayerTimer += 0.075f;
        if ((double) this.noticePlayerTimer <= 1.7999999523162842)
          break;
        this.lostPlayerInChase = true;
        break;
    }
  }

  private void CalculateAgentSpeed()
  {
    if ((double) this.stunNormalizedTimer >= 0.0)
    {
      this.agent.speed = 0.1f;
      this.agent.acceleration = 200f;
      this.creatureAnimator.SetBool("stunned", true);
    }
    else
    {
      this.creatureAnimator.SetBool("stunned", false);
      this.creatureAnimator.SetFloat("speedMultiplier", Mathf.Clamp((float) ((double) this.averageVelocity / 12.0 * 2.5), 0.1f, 6f));
      float num = (this.transform.position - this.previousPosition).magnitude / (Time.deltaTime / 1.4f);
      if ((double) this.velocityInterval <= 0.0)
      {
        this.previousVelocity = this.averageVelocity;
        this.velocityInterval = 0.05f;
        ++this.velocityAverageCount;
        if ((double) this.velocityAverageCount > 5.0)
        {
          this.averageVelocity += (float) (((double) num - (double) this.averageVelocity) / 3.0);
        }
        else
        {
          this.averageVelocity += num;
          if ((double) this.velocityAverageCount == 2.0)
            this.averageVelocity /= this.velocityAverageCount;
        }
      }
      else
        this.velocityInterval -= Time.deltaTime;
      if (this.IsOwner && (double) this.averageVelocity - (double) num > (double) Mathf.Clamp(num * 0.17f, 2f, 100f) && (double) num > 3.0 && this.currentBehaviourStateIndex == 1)
      {
        if ((double) this.wallCollisionSFXDebounce > 0.5)
        {
          if (this.IsServer)
            this.CollideWithWallServerRpc();
          else
            this.CollideWithWallClientRpc();
        }
        this.agentSpeedWithNegative *= 0.2f;
        this.wallCollisionSFXDebounce = 0.0f;
      }
      this.wallCollisionSFXDebounce += Time.deltaTime;
      this.previousPosition = this.transform.position;
      if (this.currentBehaviourStateIndex == 0)
      {
        this.agent.speed = 8f;
        this.agent.acceleration = 26f;
      }
      else
      {
        if (this.currentBehaviourStateIndex != 1)
          return;
        float speedIncreaseRate = this.SpeedIncreaseRate;
        if ((double) Time.realtimeSinceStartup - (double) this.lastTimeHit < 1.0)
          speedIncreaseRate += 4.25f;
        this.agentSpeedWithNegative += Time.deltaTime * speedIncreaseRate;
        this.agent.speed = Mathf.Clamp(this.agentSpeedWithNegative, -3f, 16f);
        this.agent.acceleration = Mathf.Clamp(this.BaseAcceleration - this.averageVelocity * this.SpeedAccelerationEffect, 4f, 40f);
        if ((double) this.agent.acceleration > 22.0)
        {
          this.agent.angularSpeed = 800f;
          this.agent.acceleration += 20f;
        }
        else
          this.agent.angularSpeed = 230f;
      }
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void CollideWithWallServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3661877694U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3661877694U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.CollideWithWallClientRpc();
  }

  [ClientRpc]
  public void CollideWithWallClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(461029090U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 461029090U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    RoundManager.PlayRandomClip(this.creatureSFX, this.hitWallSFX);
    float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position);
    if ((double) num < 15.0)
    {
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    }
    else
    {
      if ((double) num >= 24.0)
        return;
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
    }
  }

  private void CheckForVeryClosePlayer()
  {
    if (Physics.OverlapSphereNonAlloc(this.transform.position, 1.5f, this.nearPlayerColliders, 8, QueryTriggerInteraction.Ignore) <= 0)
      return;
    PlayerControllerB component = this.nearPlayerColliders[0].transform.GetComponent<PlayerControllerB>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null) || !((UnityEngine.Object) component != (UnityEngine.Object) this.targetPlayer) || Physics.Linecast(this.transform.position + Vector3.up * 0.3f, component.transform.position, StartOfRound.Instance.collidersAndRoomMask))
      return;
    this.targetPlayer = component;
  }

  [ServerRpc(RequireOwnership = false)]
  public void BeginChasingPlayerServerRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(869452445U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendServerRpc(ref bufferWriter, 869452445U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.BeginChasingPlayerClientRpc(playerObjectId);
  }

  [ClientRpc]
  public void BeginChasingPlayerClientRpc(int playerObjectId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1964892800U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerObjectId);
      this.__endSendClientRpc(ref bufferWriter, 1964892800U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.MakeScreech();
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.SetMovingTowardsTargetPlayer(StartOfRound.Instance.allPlayerScripts[playerObjectId]);
  }

  [ServerRpc(RequireOwnership = false)]
  public void MakeScreechNoiseServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(2716706397U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 2716706397U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.MakeScreechNoiseClientRpc();
  }

  [ClientRpc]
  public void MakeScreechNoiseClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3572529702U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3572529702U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.MakeScreech();
  }

  private void MakeScreech()
  {
    int index = UnityEngine.Random.Range(0, this.longRoarSFX.Length);
    this.creatureVoice.PlayOneShot(this.longRoarSFX[index]);
    WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.longRoarSFX[index]);
    if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.transform.position) >= 15.0)
      return;
    GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(0.75f);
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.timeSinceHittingPlayer < 0.64999997615814209)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.timeSinceHittingPlayer = 0.0f;
    playerControllerB.DamagePlayer(40, causeOfDeath: CauseOfDeath.Mauling);
    this.agent.speed = 0.0f;
    this.HitPlayerServerRpc((int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    GameNetworkManager.Instance.localPlayerController.JumpToFearLevel(1f);
  }

  [ServerRpc(RequireOwnership = false)]
  public void HitPlayerServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3352518565U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3352518565U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.HitPlayerClientRpc(playerId);
  }

  [ClientRpc]
  public void HitPlayerClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(880045462U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 880045462U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (!this.inSpecialAnimation)
      this.creatureAnimator.SetTrigger("HitPlayer");
    this.creatureVoice.PlayOneShot(this.bitePlayerSFX);
    this.agentSpeedWithNegative = UnityEngine.Random.Range(-2f, 0.25f);
  }

  [ServerRpc(RequireOwnership = false)]
  public void EatPlayerBodyServerRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3781293737U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendServerRpc(ref bufferWriter, 3781293737U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EatPlayerBodyClientRpc(playerId);
  }

  [ClientRpc]
  public void EatPlayerBodyClientRpc(int playerId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2460625110U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerId);
      this.__endSendClientRpc(ref bufferWriter, 2460625110U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsOwner || this.eatPlayerBodyCoroutine != null)
      return;
    this.StartCoroutine(this.EatPlayerBodyAnimation(playerId));
  }

  private IEnumerator EatPlayerBodyAnimation(int playerId)
  {
    CrawlerAI crawlerAi = this;
    PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[playerId];
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) playerScript.deadBody != (UnityEngine.Object) null && (UnityEngine.Object) playerScript.deadBody.grabBodyObject != (UnityEngine.Object) null || (double) Time.realtimeSinceStartup - (double) startTime > 2.0));
    DeadBodyInfo deadBody = (DeadBodyInfo) null;
    if ((UnityEngine.Object) StartOfRound.Instance.allPlayerScripts[playerId].deadBody != (UnityEngine.Object) null)
    {
      if (crawlerAi.debugEnemyAI)
        Debug.Log((object) "Thumper: Body is not null!");
      deadBody = StartOfRound.Instance.allPlayerScripts[playerId].deadBody;
    }
    yield return (object) null;
    if (crawlerAi.debugEnemyAI)
      Debug.Log((object) string.Format("{0}; {1}; {2}; {3}; {4}", (object) ((UnityEngine.Object) deadBody != (UnityEngine.Object) null), (object) ((UnityEngine.Object) deadBody.grabBodyObject != (UnityEngine.Object) null), (object) !deadBody.isInShip, (object) !deadBody.grabBodyObject.isHeld, (object) Vector3.Distance(crawlerAi.transform.position, deadBody.bodyParts[0].transform.position)));
    if ((UnityEngine.Object) deadBody != (UnityEngine.Object) null && (UnityEngine.Object) deadBody.grabBodyObject != (UnityEngine.Object) null && !deadBody.isInShip && !deadBody.grabBodyObject.isHeld && !crawlerAi.isEnemyDead && (double) Vector3.Distance(crawlerAi.transform.position, deadBody.bodyParts[0].transform.position) < 6.6999998092651367)
    {
      crawlerAi.creatureAnimator.SetTrigger("EatPlayer");
      crawlerAi.creatureVoice.pitch = UnityEngine.Random.Range(0.85f, 1.1f);
      crawlerAi.creatureVoice.PlayOneShot(crawlerAi.eatPlayerSFX);
      deadBody.canBeGrabbedBackByPlayers = false;
      crawlerAi.currentlyHeldBody = deadBody;
      crawlerAi.pullingSecondLimb = (UnityEngine.Object) deadBody.attachedTo != (UnityEngine.Object) null;
      if (crawlerAi.pullingSecondLimb)
      {
        deadBody.secondaryAttachedLimb = deadBody.bodyParts[3];
        deadBody.secondaryAttachedTo = crawlerAi.mouthTarget;
      }
      else
      {
        deadBody.attachedLimb = deadBody.bodyParts[0];
        deadBody.attachedTo = crawlerAi.mouthTarget;
      }
      yield return (object) new WaitForSeconds(2.75f);
    }
    Debug.Log((object) "Crawler: leaving special animation");
    crawlerAi.inSpecialAnimation = false;
    crawlerAi.DropPlayerBody();
    crawlerAi.eatPlayerBodyCoroutine = (Coroutine) null;
  }

  private void DropPlayerBody()
  {
    if (!((UnityEngine.Object) this.currentlyHeldBody != (UnityEngine.Object) null))
      return;
    if (this.pullingSecondLimb)
    {
      this.currentlyHeldBody.secondaryAttachedLimb = (Rigidbody) null;
      this.currentlyHeldBody.secondaryAttachedTo = (Transform) null;
    }
    else
    {
      this.currentlyHeldBody.attachedLimb = (Rigidbody) null;
      this.currentlyHeldBody.attachedTo = (Transform) null;
    }
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy();
    if (this.eatPlayerBodyCoroutine != null)
      this.StopCoroutine(this.eatPlayerBodyCoroutine);
    this.DropPlayerBody();
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    if (this.isEnemyDead)
      return;
    this.agent.speed = 2f;
    if (!this.inSpecialAnimation)
      this.creatureAnimator.SetTrigger("HurtEnemy");
    this.enemyHP -= force;
    this.agentSpeedWithNegative = UnityEngine.Random.Range(-2.8f, -2f);
    this.lastTimeHit = Time.realtimeSinceStartup;
    this.averageVelocity = 0.0f;
    RoundManager.PlayRandomClip(this.creatureVoice, this.hitCrawlerSFX);
    if (this.enemyHP > 0 || !this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_CrawlerAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3661877694U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_3661877694)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(461029090U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_461029090)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(869452445U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_869452445)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1964892800U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_1964892800)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2716706397U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_2716706397)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3572529702U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_3572529702)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3352518565U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_3352518565)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(880045462U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_880045462)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3781293737U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_3781293737)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2460625110U, new NetworkManager.RpcReceiveHandler(CrawlerAI.__rpc_handler_2460625110)));
  }

  private static void __rpc_handler_3661877694(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CrawlerAI) target).CollideWithWallServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_461029090(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CrawlerAI) target).CollideWithWallClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_869452445(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerObjectId;
    ByteUnpacker.ReadValueBitPacked(reader, out playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CrawlerAI) target).BeginChasingPlayerServerRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1964892800(
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
    ((CrawlerAI) target).BeginChasingPlayerClientRpc(playerObjectId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2716706397(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((CrawlerAI) target).MakeScreechNoiseServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3572529702(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((CrawlerAI) target).MakeScreechNoiseClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3352518565(
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
    ((CrawlerAI) target).HitPlayerServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_880045462(
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
    ((CrawlerAI) target).HitPlayerClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3781293737(
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
    ((CrawlerAI) target).EatPlayerBodyServerRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2460625110(
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
    ((CrawlerAI) target).EatPlayerBodyClientRpc(playerId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (CrawlerAI);
}
