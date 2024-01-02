// Decompiled with JetBrains decompiler
// Type: SandWormAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;

#nullable disable
public class SandWormAI : EnemyAI
{
  public AudioSource groundAudio;
  public ParticleSystem emergeFromGroundParticle1;
  public ParticleSystem emergeFromGroundParticle2;
  public ParticleSystem hitGroundParticle;
  public AudioClip[] groundRumbleSFX;
  public AudioClip[] ambientRumbleSFX;
  public AudioClip hitGroundSFX;
  public AudioClip emergeFromGroundSFX;
  public AudioClip[] roarSFX;
  public bool inEmergingState;
  public bool emerged;
  private int timesEmerging;
  public bool hitGroundInAnimation;
  public Transform endingPosition;
  public Transform[] airPathNodes;
  public Vector3 endOfFlightPathPosition;
  private Coroutine emergingFromGroundCoroutine;
  public AISearchRoutine roamMap;
  public float chaseTimer;
  private int stateLastFrame;
  private NavMeshHit navHit;
  private System.Random sandWormRandom;

  public override void Start()
  {
    base.Start();
    this.sandWormRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 15 + this.thisEnemyIndex);
    this.roamMap.randomized = true;
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (StartOfRound.Instance.livingPlayers == 0 || this.isEnemyDead)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.emerged || this.inEmergingState)
          break;
        if (!this.roamMap.inProgress)
          this.StartSearch(this.transform.position, this.roamMap);
        this.agent.speed = 4f;
        PlayerControllerB closestPlayer = this.GetClosestPlayer(cannotBeInShip: true, cannotBeNearShip: true);
        if (!((UnityEngine.Object) closestPlayer != (UnityEngine.Object) null) || (double) this.mostOptimalDistance >= 15.0)
          break;
        this.SetMovingTowardsTargetPlayer(closestPlayer);
        this.SwitchToBehaviourState(1);
        this.chaseTimer = 0.0f;
        break;
      case 1:
        if (this.roamMap.inProgress)
          this.StopSearch(this.roamMap);
        this.targetPlayer = this.GetClosestPlayer(cannotBeInShip: true, cannotBeNearShip: true);
        if ((double) this.mostOptimalDistance > 19.0)
          this.targetPlayer = (PlayerControllerB) null;
        if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null)
        {
          this.SwitchToBehaviourState(0);
          break;
        }
        this.SetMovingTowardsTargetPlayer(this.targetPlayer);
        if ((double) this.chaseTimer >= 1.5 || (double) Vector3.Distance(this.transform.position, this.targetPlayer.transform.position) >= 4.0 || (double) Vector3.Distance(StartOfRound.Instance.shipInnerRoomBounds.ClosestPoint(this.transform.position), this.transform.position) < 9.0 || UnityEngine.Random.Range(0, 100) >= 17)
          break;
        this.StartEmergeAnimation();
        break;
    }
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead)
      return;
    if (this.stateLastFrame != this.currentBehaviourStateIndex)
    {
      this.stateLastFrame = this.currentBehaviourStateIndex;
      this.chaseTimer = 0.0f;
    }
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (!this.creatureSFX.isPlaying)
          break;
        this.creatureSFX.Stop();
        break;
      case 1:
        if (!this.creatureSFX.isPlaying && !this.inEmergingState && !this.emerged)
        {
          this.creatureSFX.clip = this.ambientRumbleSFX[UnityEngine.Random.Range(0, this.ambientRumbleSFX.Length)];
          this.creatureSFX.Play();
        }
        if (!this.IsOwner)
          break;
        if ((UnityEngine.Object) this.targetPlayer == (UnityEngine.Object) null)
        {
          this.SwitchToBehaviourState(0);
          break;
        }
        if (!this.PlayerIsTargetable(this.targetPlayer, true) || (double) Vector3.Distance(this.targetPlayer.transform.position, this.transform.position) > 22.0)
          this.chaseTimer += Time.deltaTime;
        else
          this.chaseTimer = 0.0f;
        if ((double) this.chaseTimer <= 6.0)
          break;
        this.SwitchToBehaviourState(0);
        break;
    }
  }

  public void StartEmergeAnimation()
  {
    if (!this.IsServer)
      return;
    this.inEmergingState = true;
    float num = RoundManager.Instance.YRotationThatFacesTheFarthestFromPosition(this.transform.position + Vector3.up * 1.5f, 30f) + UnityEngine.Random.Range(-45f, 45f);
    this.agent.enabled = false;
    this.inSpecialAnimation = true;
    this.transform.eulerAngles = new Vector3(0.0f, num, 0.0f);
    bool flag = false;
    for (int index1 = 0; index1 < 6; ++index1)
    {
      RaycastHit hitInfo;
      for (int index2 = 0; index2 < this.airPathNodes.Length - 1; ++index2)
      {
        Vector3 direction = this.airPathNodes[index2 + 1].position - this.airPathNodes[index2].position;
        if (Physics.SphereCast(this.airPathNodes[index2].position, 5f, direction, out hitInfo, direction.magnitude, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
        {
          flag = false;
          for (int index3 = 0; index3 < StartOfRound.Instance.naturalSurfaceTags.Length; ++index3)
          {
            if (hitInfo.collider.CompareTag(StartOfRound.Instance.naturalSurfaceTags[index3]))
              flag = true;
          }
          if (!flag)
            break;
        }
      }
      if (!flag)
      {
        num += 60f;
        this.transform.eulerAngles = new Vector3(0.0f, num, 0.0f);
      }
      else if (Physics.Raycast(this.endingPosition.position + Vector3.up * 50f, Vector3.down, out hitInfo, 100f, StartOfRound.Instance.collidersAndRoomMaskAndDefault, QueryTriggerInteraction.Ignore))
      {
        this.endOfFlightPathPosition = RoundManager.Instance.GetNavMeshPosition(hitInfo.point, this.navHit, 8f, this.agent.areaMask);
        if (!RoundManager.Instance.GotNavMeshPositionResult)
        {
          this.endOfFlightPathPosition = RoundManager.Instance.GetClosestNode(hitInfo.point).position;
          break;
        }
        break;
      }
    }
    if (!flag)
    {
      this.inSpecialAnimation = false;
      this.agent.enabled = true;
      this.inEmergingState = false;
    }
    else
      this.EmergeServerRpc((int) num);
  }

  [ServerRpc]
  public void EmergeServerRpc(int yRot)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1498805140U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, yRot);
      this.__endSendServerRpc(ref bufferWriter, 1498805140U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.EmergeClientRpc(yRot);
  }

  [ClientRpc]
  public void EmergeClientRpc(int yRot)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1497638036U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, yRot);
      this.__endSendClientRpc(ref bufferWriter, 1497638036U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.inSpecialAnimation = true;
    this.inEmergingState = true;
    this.hitGroundInAnimation = false;
    this.agent.enabled = false;
    this.transform.position = this.serverPosition;
    this.transform.eulerAngles = new Vector3(0.0f, (float) yRot, 0.0f);
    ++this.timesEmerging;
    this.creatureSFX.Stop();
    if (this.emergingFromGroundCoroutine != null)
      this.StopCoroutine(this.emergingFromGroundCoroutine);
    this.emergingFromGroundCoroutine = this.StartCoroutine(this.EmergeFromGround(yRot));
  }

  private IEnumerator EmergeFromGround(int rot)
  {
    SandWormAI sandWormAi = this;
    RoundManager.PlayRandomClip(sandWormAi.creatureSFX, sandWormAi.groundRumbleSFX);
    sandWormAi.emergeFromGroundParticle1.Play(true);
    yield return (object) new WaitForSeconds((float) sandWormAi.sandWormRandom.Next(1, 7) / 3f);
    sandWormAi.creatureAnimator.SetBool("emerge", true);
    sandWormAi.inEmergingState = false;
    sandWormAi.emerged = true;
    yield return (object) new WaitForSeconds(0.1f);
    sandWormAi.creatureSFX.PlayOneShot(sandWormAi.emergeFromGroundSFX);
    sandWormAi.emergeFromGroundParticle2.Play();
    sandWormAi.ShakePlayerCameraInProximity(sandWormAi.transform.position);
    yield return (object) new WaitForSeconds((float) sandWormAi.sandWormRandom.Next(2, 5) / 3f);
    sandWormAi.creatureVoice.PlayOneShot(sandWormAi.roarSFX[sandWormAi.sandWormRandom.Next(0, sandWormAi.roarSFX.Length)]);
    Debug.Log((object) "Playing sandworm roar!");
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(sandWormAi.\u003CEmergeFromGround\u003Eb__28_0));
    sandWormAi.hitGroundParticle.Play(true);
    sandWormAi.groundAudio.PlayOneShot(sandWormAi.hitGroundSFX);
    sandWormAi.ShakePlayerCameraInProximity(sandWormAi.groundAudio.transform.position);
    yield return (object) new WaitForSeconds(10f);
    sandWormAi.SetInGround();
  }

  private void ShakePlayerCameraInProximity(Vector3 pos)
  {
    if (GameNetworkManager.Instance.localPlayerController.isInsideFactory)
      return;
    float num = Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, pos);
    if ((double) num < 27.0)
    {
      Debug.Log((object) "Shaking camera strong");
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
    }
    else if ((double) num < 50.0)
    {
      Debug.Log((object) "Shaking camera strong");
      HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
    }
    else if ((double) num < 90.0)
    {
      Debug.Log((object) "Shaking camera long");
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Long);
    }
    else
    {
      if ((double) num >= 120.0)
        return;
      Debug.Log((object) "Shaking camera small");
      HUDManager.Instance.ShakeCamera(ScreenShakeType.Small);
    }
  }

  public void HitGroundInAnimation() => this.hitGroundInAnimation = true;

  public void SetInGround()
  {
    this.transform.position = this.endOfFlightPathPosition;
    this.inSpecialAnimation = false;
    this.emerged = false;
    this.inEmergingState = false;
    this.creatureAnimator.SetBool("emerge", false);
    if (!this.IsOwner)
      return;
    this.agent.enabled = true;
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if (this.isEnemyDead || !this.emerged)
      return;
    PlayerControllerB component = other.gameObject.GetComponent<PlayerControllerB>();
    if (!((UnityEngine.Object) component != (UnityEngine.Object) null) || !((UnityEngine.Object) component.inAnimationWithEnemy == (UnityEngine.Object) null) || !((UnityEngine.Object) component == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      return;
    this.EatPlayer(component);
  }

  public void EatPlayer(PlayerControllerB playerScript)
  {
    if (playerScript.inSpecialInteractAnimation && (UnityEngine.Object) playerScript.currentTriggerInAnimationWith != (UnityEngine.Object) null)
      playerScript.currentTriggerInAnimationWith.CancelAnimationExternally();
    playerScript.inAnimationWithEnemy = (EnemyAI) null;
    playerScript.inSpecialInteractAnimation = false;
    Debug.Log((object) "KILL player called");
    playerScript.KillPlayer(Vector3.zero, false);
  }

  public override void OnCollideWithEnemy(Collider other, EnemyAI enemyScript = null)
  {
    base.OnCollideWithEnemy(other);
    if (!this.IsServer || !this.emerged)
      return;
    enemyScript.KillEnemyOnOwnerClient(true);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_SandWormAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1498805140U, new NetworkManager.RpcReceiveHandler(SandWormAI.__rpc_handler_1498805140)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1497638036U, new NetworkManager.RpcReceiveHandler(SandWormAI.__rpc_handler_1497638036)));
  }

  private static void __rpc_handler_1498805140(
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
      int yRot;
      ByteUnpacker.ReadValueBitPacked(reader, out yRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((SandWormAI) target).EmergeServerRpc(yRot);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_1497638036(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int yRot;
    ByteUnpacker.ReadValueBitPacked(reader, out yRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((SandWormAI) target).EmergeClientRpc(yRot);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (SandWormAI);
}
