// Decompiled with JetBrains decompiler
// Type: DoublewingAI
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class DoublewingAI : EnemyAI
{
  public Animator bodyAnimator;
  private int behaviourStateLastFrame = -1;
  public AudioSource flappingAudio;
  public AudioClip[] birdScreechSFX;
  public AudioClip birdHitGroundSFX;
  public AISearchRoutine roamGlide;
  private bool alertingBird;
  private float glideTime = 10f;
  private float currentGlideTime;
  private RaycastHit hit;
  private bool flyingToOtherBirdLanding;
  private float avoidingPlayer;
  public Transform Body;
  private Vector3 previousPosition;
  private float flyLayerWeight;
  [Space(5f)]
  public float maxSpeed;
  [Space(5f)]
  public float speedElevationMultiplier;
  private float randomYRot;
  private int velocityAverageCount;
  private float averageVelocity;
  private float lerpedElevation;
  private float timeSinceEnteringFlight;
  private float randomHeightOffset;
  private bool birdStunned;
  private bool oddInterval;
  private int birdNoisiness = 5;
  private float timeSinceSquawking;
  private float velocityInterval;
  public Rigidbody birdRigidbody;
  private int timesSyncingPosition;

  public override void Start()
  {
    base.Start();
    this.creatureAnimator.SetInteger("idleType", UnityEngine.Random.Range(0, 2));
    this.creatureAnimator.SetFloat("speedMultiplier", UnityEngine.Random.Range(0.73f, 1.3f));
    this.bodyAnimator.SetFloat("speedMultiplier", UnityEngine.Random.Range(0.8f, 1.2f));
    this.randomHeightOffset = (float) new System.Random(StartOfRound.Instance.randomMapSeed / (int) ((long) this.NetworkObjectId + 1L)).NextDouble();
  }

  public override void DaytimeEnemyLeave()
  {
    base.DaytimeEnemyLeave();
    if ((double) this.stunNormalizedTimer < 0.0 && !this.isEnemyDead)
    {
      this.bodyAnimator.SetBool("flying", true);
      this.creatureAnimator.SetBool("gliding", true);
      this.bodyAnimator.SetTrigger("Leave");
    }
    this.StartCoroutine(this.flyAwayThenDespawn());
  }

  private IEnumerator flyAwayThenDespawn()
  {
    DoublewingAI doublewingAi = this;
    yield return (object) new WaitForSeconds(7f);
    if (doublewingAi.IsOwner)
      doublewingAi.KillEnemyOnOwnerClient(true);
  }

  public override void DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    base.DetectNoise(noisePosition, noiseLoudness, timesPlayedInOneSpot, noiseID);
    if (noiseID == 911 || this.isEnemyDead || (double) this.stunNormalizedTimer > 0.0)
      return;
    float num1 = Vector3.Distance(noisePosition, this.transform.position + Vector3.up * 0.5f);
    if (Physics.Linecast(this.transform.position, noisePosition, 256))
      noiseLoudness /= 2f;
    float num2 = 0.01f;
    if ((double) noiseLoudness / (double) num1 <= (double) num2 || this.currentBehaviourStateIndex != 0 || this.alertingBird)
      return;
    this.alertingBird = true;
    this.AlertBirdServerRpc();
  }

  public void StunBird()
  {
    if (this.birdStunned)
      return;
    this.birdStunned = true;
    this.agent.speed = 0.0f;
    DoublewingAI[] objectsByType = UnityEngine.Object.FindObjectsByType<DoublewingAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    for (int index = 0; index < objectsByType.Length; ++index)
    {
      if (!((UnityEngine.Object) objectsByType[index] == (UnityEngine.Object) this) && (double) Vector3.Distance(objectsByType[index].transform.position, this.transform.position) < 8.0)
        objectsByType[index].AlertBirdByOther();
    }
    this.flappingAudio.Stop();
    this.creatureAnimator.SetBool("stunned", true);
    this.bodyAnimator.SetBool("stunned", true);
  }

  public void UnstunBird()
  {
    if (!this.birdStunned)
      return;
    this.birdStunned = false;
    this.creatureAnimator.SetBool("stunned", false);
    this.bodyAnimator.SetBool("stunned", false);
    if (this.currentBehaviourStateIndex != 0)
      return;
    this.SwitchToBehaviourStateOnLocalClient(1);
  }

  public override void DoAIInterval()
  {
    base.DoAIInterval();
    if (this.daytimeEnemyLeaving || this.isEnemyDead || StartOfRound.Instance.allPlayersDead || (double) this.stunNormalizedTimer > 0.0)
      return;
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        this.oddInterval = !this.oddInterval;
        if (!this.oddInterval || this.alertingBird || !(bool) (UnityEngine.Object) this.CheckLineOfSightForPlayer(80f, 8, 4))
          break;
        this.alertingBird = true;
        this.AlertBirdServerRpc();
        break;
      case 1:
        this.behaviourStateLastFrame = 1;
        this.creatureAnimator.SetBool("gliding", true);
        this.bodyAnimator.SetBool("flying", true);
        this.agent.speed = Mathf.Clamp(this.agent.speed + this.AIIntervalTime * 4f, 5f, 19f);
        if (!this.flyingToOtherBirdLanding && (double) this.avoidingPlayer <= 0.0 && !this.roamGlide.inProgress)
          this.StartSearch(this.transform.position, this.roamGlide);
        if ((double) this.avoidingPlayer > 0.0)
        {
          this.avoidingPlayer -= this.AIIntervalTime;
          if ((double) Vector3.Distance(this.transform.position, this.destination) >= 3.0)
            break;
          this.avoidingPlayer = 0.0f;
          break;
        }
        PlayerControllerB playerControllerB = this.CheckLineOfSightForPlayer(80f, 10, 8);
        if (this.oddInterval && (bool) (UnityEngine.Object) playerControllerB && this.SetDestinationToPosition(this.ChooseFarthestNodeFromPosition(playerControllerB.transform.position, offset: UnityEngine.Random.Range(0, this.allAINodes.Length / 2)).position))
        {
          this.avoidingPlayer = (float) UnityEngine.Random.Range(10, 20);
          this.StopSearch(this.roamGlide);
        }
        this.currentGlideTime += this.AIIntervalTime;
        if ((double) this.currentGlideTime <= (double) this.glideTime)
          break;
        this.currentGlideTime = 0.0f;
        if (this.flyingToOtherBirdLanding)
        {
          if (!this.SetDestinationToPosition(this.destination, true))
          {
            this.flyingToOtherBirdLanding = false;
            this.glideTime = 5f;
            break;
          }
          if ((double) Vector3.Distance(this.transform.position, this.destination) < 3.0)
          {
            if (!this.TryLanding())
            {
              this.flyingToOtherBirdLanding = false;
              this.glideTime = 5f;
              break;
            }
            this.SwitchToBehaviourState(0);
            break;
          }
        }
        for (int index = 0; index < RoundManager.Instance.SpawnedEnemies.Count; ++index)
        {
          if ((UnityEngine.Object) RoundManager.Instance.SpawnedEnemies[index].enemyType == (UnityEngine.Object) this.enemyType && RoundManager.Instance.SpawnedEnemies[index].currentBehaviourStateIndex == 0 && (double) Vector3.Distance(this.transform.position, RoundManager.Instance.SpawnedEnemies[index].transform.position) < 100.0)
          {
            if (this.SetDestinationToPosition(RoundManager.Instance.GetRandomNavMeshPositionInRadius(RoundManager.Instance.SpawnedEnemies[index].transform.position), true))
            {
              this.StopSearch(this.roamGlide);
              this.flyingToOtherBirdLanding = true;
              this.glideTime = 2f;
              break;
            }
            break;
          }
        }
        if (this.flyingToOtherBirdLanding)
          break;
        if (this.TryLanding())
        {
          this.SwitchToBehaviourState(0);
          break;
        }
        this.glideTime = 10f;
        break;
    }
  }

  public bool TryLanding()
  {
    if (!Physics.Raycast(new Vector3(this.transform.position.x, this.eye.position.y, this.transform.position.z), Vector3.down, out this.hit, 60f, StartOfRound.Instance.collidersAndRoomMaskAndDefault) || Physics.CheckSphere(this.hit.point, 16f, StartOfRound.Instance.playersMask, QueryTriggerInteraction.Ignore))
      return false;
    if ((double) Vector3.Distance(this.hit.point, this.transform.position) <= 1.0)
      return true;
    if (!this.SetDestinationToPosition(this.hit.point, true))
      return false;
    this.agent.Warp(this.destination);
    return true;
  }

  [ServerRpc(RequireOwnership = false)]
  public void AlertBirdServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(838150599U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 838150599U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.AlertBirdClientRpc();
  }

  [ClientRpc]
  public void AlertBirdClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3264241129U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3264241129U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.AlertBird();
  }

  public void AlertBird()
  {
    DoublewingAI[] objectsByType = UnityEngine.Object.FindObjectsByType<DoublewingAI>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
    for (int index = 0; index < objectsByType.Length; ++index)
    {
      if (!((UnityEngine.Object) objectsByType[index] == (UnityEngine.Object) this) && (double) Vector3.Distance(objectsByType[index].transform.position, this.transform.position) < 8.0)
        objectsByType[index].AlertBirdByOther();
    }
    this.SwitchToBehaviourStateOnLocalClient(1);
    this.alertingBird = false;
  }

  public void AlertBirdByOther()
  {
    if (this.daytimeEnemyLeaving)
      return;
    if (this.IsServer)
      this.SwitchToBehaviourState(1);
    else
      this.SwitchToBehaviourStateOnLocalClient(1);
  }

  public override void Update()
  {
    base.Update();
    if (this.isEnemyDead || StartOfRound.Instance.allPlayersDead)
      return;
    this.SetFlyDirection();
    if (this.daytimeEnemyLeaving)
      return;
    this.timeSinceSquawking += Time.deltaTime;
    if ((double) this.stunNormalizedTimer > 0.0)
      this.StunBird();
    else
      this.UnstunBird();
    switch (this.currentBehaviourStateIndex)
    {
      case 0:
        if (this.behaviourStateLastFrame != 0)
        {
          this.behaviourStateLastFrame = 0;
          this.randomYRot = UnityEngine.Random.Range(0.0f, 360f);
          this.agent.speed = 0.0f;
          this.creatureAnimator.SetBool("gliding", false);
          this.bodyAnimator.SetBool("flying", false);
          this.flyingToOtherBirdLanding = false;
          this.timeSinceEnteringFlight = 0.0f;
        }
        this.flyLayerWeight = Mathf.Max(0.0f, this.flyLayerWeight - Time.deltaTime * 0.28f);
        this.timeSinceEnteringFlight += Time.deltaTime;
        break;
      case 1:
        if (this.behaviourStateLastFrame != 1)
        {
          this.behaviourStateLastFrame = 1;
          this.timeSinceEnteringFlight = 0.0f;
          this.creatureAnimator.SetBool("gliding", true);
          this.bodyAnimator.SetBool("flying", true);
          WalkieTalkie.TransmitOneShotAudio(this.creatureSFX, this.enemyType.audioClips[RoundManager.PlayRandomClip(this.creatureSFX, this.enemyType.audioClips)], 0.7f);
          RoundManager.Instance.PlayAudibleNoise(this.transform.position, 12f, 0.6f, noiseID: 911);
          this.glideTime = UnityEngine.Random.Range(8f, 20f);
        }
        this.timeSinceEnteringFlight += Time.deltaTime;
        this.flyLayerWeight = Mathf.Min(1f, this.flyLayerWeight + Time.deltaTime * 0.33f);
        break;
    }
  }

  private void BirdScreech()
  {
    RoundManager.PlayRandomClip(this.creatureVoice, this.birdScreechSFX);
    WalkieTalkie.TransmitOneShotAudio(this.creatureVoice, this.birdScreechSFX[UnityEngine.Random.Range(0, this.birdScreechSFX.Length)]);
    RoundManager.Instance.PlayAudibleNoise(this.transform.position, 12f, 0.6f, noiseID: 911);
  }

  public void SetFlyDirection()
  {
    if (this.birdStunned)
    {
      this.Body.localEulerAngles = this.Body.localEulerAngles with
      {
        x = 0.0f,
        z = 0.0f
      };
    }
    else
    {
      bool flag = (double) this.averageVelocity * (double) this.speedElevationMultiplier < 12.0;
      float num = (this.Body.position - this.previousPosition).magnitude / Time.deltaTime;
      if (this.daytimeEnemyLeaving)
        this.Body.rotation = Quaternion.Lerp(this.Body.rotation, Quaternion.LookRotation(this.Body.position - this.previousPosition, Vector3.up), 5f * Time.deltaTime);
      else if (this.currentBehaviourStateIndex == 0 || (double) this.timeSinceEnteringFlight < 1.0)
      {
        flag = false;
        this.Body.rotation = Quaternion.Lerp(this.Body.rotation, Quaternion.Euler(new Vector3(0.0f, this.randomYRot, 0.0f)), 10f * Time.deltaTime);
      }
      else if ((double) this.averageVelocity * (double) this.speedElevationMultiplier > 0.0)
        this.Body.rotation = Quaternion.Lerp(this.Body.rotation, Quaternion.LookRotation(this.Body.position - this.previousPosition, Vector3.up), 5f * Time.deltaTime);
      if ((double) this.velocityInterval <= 0.0)
      {
        this.velocityInterval = 0.1f;
        ++this.velocityAverageCount;
        if (this.velocityAverageCount > 5)
        {
          this.averageVelocity += (float) (((double) num - (double) this.averageVelocity) / 6.0);
        }
        else
        {
          this.averageVelocity += num;
          if (this.velocityAverageCount == 5)
            this.averageVelocity /= (float) this.velocityAverageCount;
        }
      }
      else
        this.velocityInterval -= Time.deltaTime;
      this.creatureAnimator.SetBool("flapping", flag);
      if (flag)
      {
        if ((double) this.flappingAudio.volume <= 0.99000000953674316)
          this.flappingAudio.volume = Mathf.Min(this.flappingAudio.volume + Time.deltaTime, 1f);
        if (!this.flappingAudio.isPlaying)
          this.flappingAudio.Play();
      }
      else if ((double) this.flappingAudio.volume >= 0.05000000074505806)
        this.flappingAudio.volume = Mathf.Max(this.flappingAudio.volume - Time.deltaTime, 0.0f);
      else
        this.flappingAudio.Stop();
      this.lerpedElevation = Mathf.Lerp(this.lerpedElevation, this.averageVelocity * this.speedElevationMultiplier / this.maxSpeed, Time.deltaTime * 0.5f);
      this.bodyAnimator.SetFloat("elevation", Mathf.Clamp(this.lerpedElevation * this.randomHeightOffset, 0.02f, 0.98f));
      this.previousPosition = this.Body.position;
    }
  }

  public override void HitEnemy(int force = 1, PlayerControllerB playerWhoHit = null, bool playHitSFX = false)
  {
    base.HitEnemy(force, playerWhoHit, playHitSFX);
    if (!this.IsOwner)
      return;
    this.KillEnemyOnOwnerClient();
  }

  public override void KillEnemy(bool destroy = false)
  {
    base.KillEnemy(destroy);
    this.bodyAnimator.SetBool("stunned", true);
    this.creatureAnimator.SetBool("dead", true);
  }

  public override void AnimationEventA()
  {
    base.AnimationEventA();
    if (!this.IsServer || (double) this.timeSinceSquawking <= 0.699999988079071 || UnityEngine.Random.Range(0, 100) >= this.birdNoisiness)
      return;
    this.timeSinceSquawking = 0.0f;
    this.BirdScreechClientRpc();
  }

  [ClientRpc(Delivery = RpcDelivery.Unreliable)]
  public void BirdScreechClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2325720037U, clientRpcParams, RpcDelivery.Unreliable);
      this.__endSendClientRpc(ref bufferWriter, 2325720037U, clientRpcParams, RpcDelivery.Unreliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.BirdScreech();
  }

  public override void AnimationEventB()
  {
    base.AnimationEventB();
    this.creatureSFX.PlayOneShot(this.birdHitGroundSFX);
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_DoublewingAI()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(838150599U, new NetworkManager.RpcReceiveHandler(DoublewingAI.__rpc_handler_838150599)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3264241129U, new NetworkManager.RpcReceiveHandler(DoublewingAI.__rpc_handler_3264241129)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2325720037U, new NetworkManager.RpcReceiveHandler(DoublewingAI.__rpc_handler_2325720037)));
  }

  private static void __rpc_handler_838150599(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DoublewingAI) target).AlertBirdServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3264241129(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoublewingAI) target).AlertBirdClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2325720037(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DoublewingAI) target).BirdScreechClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (DoublewingAI);
}
