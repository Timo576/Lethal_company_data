// Decompiled with JetBrains decompiler
// Type: BlobAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class BlobAI : EnemyAI
{
  [Header("Fluid simulation")]
  public Transform centerPoint;
  public Transform[] SlimeRaycastTargets;
  public Rigidbody[] SlimeBones;
  private Vector3[] SlimeBonePositions = new Vector3[8];
  public float slimeRange = 8f;
  public float currentSlimeRange;
  private float[] maxDistanceForSlimeRays = new float[8];
  private float[] distanceOfRaysLastFrame = new float[8];
  private int partsMovingUpSlope;
  private Ray slimeRay;
  private RaycastHit slimeRayHit;
  private RaycastHit slimePlayerRayHit;
  private float timeSinceHittingLocalPlayer;
  [Header("Behaviors")]
  public AISearchRoutine searchForPlayers;
  private float tamedTimer;
  private float angeredTimer;
  private Material thisSlimeMaterial;
  private float slimeJiggleAmplitude;
  private float slimeJiggleDensity;
  [Header("SFX")]
  public AudioSource movableAudioSource;
  public AudioClip agitatedSFX;
  public AudioClip jiggleSFX;
  public AudioClip hitSlimeSFX;
  public AudioClip killPlayerSFX;
  public AudioClip idleSFX;
  private Collider[] ragdollColliders;
  private Coroutine eatPlayerBodyCoroutine;
  private DeadBodyInfo bodyBeingCarried;
  private int slimeMask = 268470529;
  public Mesh emptySuitMesh;

  public override void Start()
  {
    this.ragdollColliders = new Collider[4];
    base.Start();
    this.thisSlimeMaterial = this.skinnedMeshRenderers[0].material;
    for (int index = 0; index < this.maxDistanceForSlimeRays.Length; ++index)
    {
      this.maxDistanceForSlimeRays[index] = 3.7f;
      this.SlimeBonePositions[index] = this.SlimeBones[index].transform.position;
    }
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    if (this.TargetClosestPlayer(4f))
    {
      this.StopSearch(this.searchForPlayers);
      this.movingTowardsTargetPlayer = true;
    }
    else
    {
      this.movingTowardsTargetPlayer = false;
      this.StartSearch(this.transform.position, this.searchForPlayers);
    }
  }

  private void SimulateSurfaceTensionInRaycasts(int i)
  {
    float num1 = this.distanceOfRaysLastFrame[(i + 1) % this.SlimeRaycastTargets.Length];
    float num2 = Mathf.Clamp((float) (((i != 0 ? (double) this.distanceOfRaysLastFrame[i - 1] : (double) this.distanceOfRaysLastFrame[this.SlimeRaycastTargets.Length - 1]) + (double) num1) / 2.0), 0.5f, 200f);
    float num3 = 1f;
    if ((double) num2 < 2.0)
      num3 = 2f;
    this.maxDistanceForSlimeRays[i] = Mathf.Clamp(num2 * 2f * num3, 0.0f, this.currentSlimeRange);
  }

  private void FixedUpdate()
  {
    if (!this.ventAnimationFinished)
      return;
    for (int index = 0; index < this.SlimeBonePositions.Length; ++index)
    {
      if ((double) Vector3.Distance(this.centerPoint.position, this.SlimeBonePositions[index]) > (double) this.distanceOfRaysLastFrame[index])
        this.SlimeBones[index].MovePosition(Vector3.Lerp(this.SlimeBones[index].position, this.SlimeBonePositions[index], 10f * Time.deltaTime));
      else
        this.SlimeBones[index].MovePosition(Vector3.Lerp(this.SlimeBones[index].position, this.SlimeBonePositions[index], 5f * Time.deltaTime));
    }
  }

  public override void Update()
  {
    base.Update();
    if (!this.ventAnimationFinished || !((UnityEngine.Object) this.creatureAnimator != (UnityEngine.Object) null))
      return;
    this.creatureAnimator.enabled = false;
    if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    this.timeSinceHittingLocalPlayer += Time.deltaTime;
    this.partsMovingUpSlope = 0;
    Vector3 serverPosition = this.serverPosition;
    for (int i = 0; i < this.SlimeRaycastTargets.Length; ++i)
    {
      Vector3 direction = this.SlimeRaycastTargets[i].position - this.centerPoint.position;
      this.slimeRay = new Ray(serverPosition, direction);
      this.RaycastCollisionWithPlayers(Vector3.Distance(serverPosition, this.SlimeBones[i].transform.position));
      if (Physics.Raycast(this.slimeRay, out this.slimeRayHit, this.maxDistanceForSlimeRays[i], this.slimeMask, QueryTriggerInteraction.Ignore))
      {
        this.MoveSlimeBoneToRaycastHit(0.0f, i);
      }
      else
      {
        Vector3 navMeshPosition = RoundManager.Instance.GetNavMeshPosition(this.slimeRay.GetPoint(this.maxDistanceForSlimeRays[i]));
        this.SlimeBonePositions[i] = Vector3.Lerp(this.SlimeBonePositions[i], navMeshPosition, 1f * Time.deltaTime);
        this.distanceOfRaysLastFrame[i] = this.maxDistanceForSlimeRays[i];
      }
    }
    if ((double) this.stunNormalizedTimer > 0.0)
    {
      this.thisSlimeMaterial.SetFloat("_Frequency", 4f);
      this.slimeJiggleDensity = Mathf.Lerp(this.slimeJiggleDensity, 1f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Ripple_Density", this.slimeJiggleDensity);
      this.slimeJiggleAmplitude = Mathf.Lerp(this.slimeJiggleAmplitude, 0.17f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Amplitude", this.slimeJiggleAmplitude);
      this.agent.speed = 0.0f;
      this.currentSlimeRange = Mathf.Lerp(this.currentSlimeRange, 2f, Time.deltaTime * 4f);
      this.angeredTimer = 7f;
    }
    else if ((double) this.angeredTimer > 0.0)
    {
      this.angeredTimer -= Time.deltaTime;
      this.currentSlimeRange = Mathf.Lerp(this.currentSlimeRange, this.slimeRange + 6f, Time.deltaTime * 3f);
      this.thisSlimeMaterial.SetFloat("_Frequency", 3f);
      this.slimeJiggleDensity = Mathf.Lerp(this.slimeJiggleDensity, 1f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Ripple_Density", this.slimeJiggleDensity);
      this.slimeJiggleAmplitude = Mathf.Lerp(this.slimeJiggleAmplitude, 0.14f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Amplitude", this.slimeJiggleAmplitude);
      if ((UnityEngine.Object) this.creatureSFX.clip != (UnityEngine.Object) this.agitatedSFX)
      {
        this.creatureSFX.clip = this.agitatedSFX;
        this.creatureSFX.Play();
      }
      if (!this.IsOwner)
        return;
      this.agent.stoppingDistance = 0.1f;
      this.agent.speed = 0.6f;
    }
    else if ((double) this.tamedTimer > 0.0)
    {
      this.tamedTimer -= Time.deltaTime;
      this.currentSlimeRange = 1.5f;
      this.thisSlimeMaterial.SetFloat("_Frequency", 4.3f);
      this.slimeJiggleDensity = Mathf.Lerp(this.slimeJiggleDensity, 1.3f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Ripple_Density", this.slimeJiggleDensity);
      this.slimeJiggleAmplitude = Mathf.Lerp(this.slimeJiggleAmplitude, 0.2f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Amplitude", this.slimeJiggleAmplitude);
      if ((UnityEngine.Object) this.creatureSFX.clip != (UnityEngine.Object) this.jiggleSFX)
      {
        this.creatureSFX.clip = this.jiggleSFX;
        this.creatureSFX.Play();
      }
      if (!this.IsOwner)
        return;
      this.agent.stoppingDistance = 5f;
      this.agent.speed = Mathf.Lerp(this.agent.speed, 3f, 0.7f * Time.deltaTime);
    }
    else
    {
      this.currentSlimeRange = this.partsMovingUpSlope < 2 ? this.slimeRange : Mathf.Clamp(this.slimeRange / 2f, 1.5f, 100f);
      this.thisSlimeMaterial.SetFloat("_Frequency", 2f);
      this.slimeJiggleDensity = Mathf.Lerp(this.slimeJiggleDensity, 0.6f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Ripple_Density", this.slimeJiggleDensity);
      this.slimeJiggleAmplitude = Mathf.Lerp(this.slimeJiggleAmplitude, 0.15f, 10f * Time.deltaTime);
      this.thisSlimeMaterial.SetFloat("_Amplitude", this.slimeJiggleAmplitude);
      if ((UnityEngine.Object) this.creatureSFX.clip != (UnityEngine.Object) this.idleSFX)
      {
        this.creatureSFX.clip = this.idleSFX;
        this.creatureSFX.Play();
      }
      if (!this.IsOwner)
        return;
      this.agent.stoppingDistance = 0.1f;
      this.agent.speed = 0.5f;
    }
  }

  private void MoveSlimeBoneToRaycastHit(float currentRangeOfRaycast, int i)
  {
    float num = 1.8f;
    if ((double) this.slimeRayHit.distance + (double) currentRangeOfRaycast < (double) this.distanceOfRaysLastFrame[i])
      num = 5f;
    this.SlimeBonePositions[i] = Vector3.Lerp(this.SlimeBonePositions[i], this.slimeRay.GetPoint(this.slimeRayHit.distance), num * Time.deltaTime);
    this.distanceOfRaysLastFrame[i] = this.slimeRayHit.distance + currentRangeOfRaycast;
  }

  private void RaycastCollisionWithPlayers(float maxDistance)
  {
    maxDistance -= 1.55f;
    if (!Physics.SphereCast(this.slimeRay, 0.7f, out this.slimePlayerRayHit, maxDistance, 2312) || !this.slimePlayerRayHit.collider.gameObject.CompareTag("Player"))
      return;
    this.OnCollideWithPlayer(this.slimePlayerRayHit.collider);
  }

  public override void OnCollideWithPlayer(Collider other)
  {
    base.OnCollideWithPlayer(other);
    if ((double) this.timeSinceHittingLocalPlayer < 0.25 || (double) this.tamedTimer > 0.0 && (double) this.angeredTimer < 0.0)
      return;
    PlayerControllerB playerControllerB = this.MeetsStandardPlayerCollisionConditions(other);
    if (!((UnityEngine.Object) playerControllerB != (UnityEngine.Object) null))
      return;
    this.timeSinceHittingLocalPlayer = 0.0f;
    playerControllerB.DamagePlayer(35);
    if (!playerControllerB.isPlayerDead)
      return;
    this.SlimeKillPlayerEffectServerRpc((int) playerControllerB.playerClientId);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SlimeKillPlayerEffectServerRpc(int playerKilled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3848306567U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerKilled);
      this.__endSendServerRpc(ref bufferWriter, 3848306567U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SlimeKillPlayerEffectClientRpc(playerKilled);
  }

  [ClientRpc]
  public void SlimeKillPlayerEffectClientRpc(int playerKilled)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1531516867U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, playerKilled);
      this.__endSendClientRpc(ref bufferWriter, 1531516867U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.creatureSFX.PlayOneShot(this.killPlayerSFX);
    this.angeredTimer = 0.0f;
    if (this.eatPlayerBodyCoroutine != null)
      return;
    this.eatPlayerBodyCoroutine = this.StartCoroutine(this.eatPlayerBody(playerKilled));
  }

  private IEnumerator eatPlayerBody(int playerKilled)
  {
    yield return (object) null;
    PlayerControllerB playerScript = StartOfRound.Instance.allPlayerScripts[playerKilled];
    float startTime = Time.realtimeSinceStartup;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) playerScript.deadBody != (UnityEngine.Object) null || (double) Time.realtimeSinceStartup - (double) startTime > 2.0));
    if ((UnityEngine.Object) playerScript.deadBody == (UnityEngine.Object) null)
    {
      Debug.Log((object) "Blob: Player body was not spawned or found within 2 seconds.");
    }
    else
    {
      playerScript.deadBody.attachedLimb = playerScript.deadBody.bodyParts[6];
      playerScript.deadBody.attachedTo = this.centerPoint;
      playerScript.deadBody.matchPositionExactly = false;
      yield return (object) new WaitForSeconds(2f);
      playerScript.deadBody.attachedTo = (Transform) null;
      playerScript.deadBody.ChangeMesh(this.emptySuitMesh);
    }
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit);
    this.angeredTimer = 18f;
    this.movableAudioSource.transform.position = playerWhoHit.gameplayCamera.transform.position + playerWhoHit.gameplayCamera.transform.forward * 1.5f;
    this.movableAudioSource.PlayOneShot(this.hitSlimeSFX);
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    if (noiseID != 5 || Physics.Linecast(this.transform.position, noisePosition, StartOfRound.Instance.collidersAndRoomMask) || (double) Vector3.Distance(this.transform.position, noisePosition) >= 12.0)
      return;
    this.tamedTimer = 2f;
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_BlobAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3848306567U, new NetworkManager.RpcReceiveHandler(BlobAI.__rpc_handler_3848306567)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1531516867U, new NetworkManager.RpcReceiveHandler(BlobAI.__rpc_handler_1531516867)));
  }

  private static void __rpc_handler_3848306567(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerKilled;
    ByteUnpacker.ReadValueBitPacked(reader, out playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((BlobAI) target).SlimeKillPlayerEffectServerRpc(playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1531516867(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int playerKilled;
    ByteUnpacker.ReadValueBitPacked(reader, out playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((BlobAI) target).SlimeKillPlayerEffectClientRpc(playerKilled);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (BlobAI);
}
