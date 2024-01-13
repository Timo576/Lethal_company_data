// Decompiled with JetBrains decompiler
// Type: DepositItemsDesk
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using GameNetcodeStuff;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

#nullable disable
public class DepositItemsDesk : NetworkBehaviour, INoiseListener
{
  public bool inGrabbingObjectsAnimation = true;
  public bool attacking;
  public bool doorOpen;
  private float noiseBehindWallVolume = 1f;
  [Space(3f)]
  public CompanyMood[] allMoodPresets;
  public CompanyMood currentMood;
  public float patienceLevel;
  public float timesHearingNoise;
  [Space(3f)]
  public float grabObjectsWaitTime = 10f;
  private float grabObjectsTimer = 10f;
  [Space(5f)]
  public NetworkObject deskObjectsContainer;
  public BoxCollider triggerCollider;
  public InteractTrigger triggerScript;
  public List<GrabbableObject> itemsOnCounter = new List<GrabbableObject>();
  public List<NetworkObject> itemsOnCounterNetworkObjects = new List<NetworkObject>();
  public int itemsOnCounterAmount;
  public Animator depositDeskAnimator;
  private NetworkObject lastObjectAddedToDesk;
  private Coroutine acceptItemsCoroutine;
  private int angerSFXindex;
  private int clientsRecievedSellItemsRPC;
  private float updateInterval;
  private System.Random CompanyLevelRandom;
  [Header("AUDIOS")]
  public AudioSource deskAudio;
  [Header("AUDIOS")]
  public AudioSource wallAudio;
  [Header("AUDIOS")]
  public AudioSource constantWallAudio;
  [Header("AUDIOS")]
  public AudioSource doorWindowAudio;
  public AudioClip[] microphoneAudios;
  public AudioClip[] rareMicrophoneAudios;
  public AudioClip doorOpenSFX;
  public AudioClip doorShutSFX;
  public AudioClip rumbleSFX;
  public AudioClip rewardGood;
  public AudioClip rewardBad;
  public AudioSource rewardsMusic;
  public AudioSource speakerAudio;
  [Header("Attack animations")]
  public MonsterAnimation[] monsterAnimations;
  public float killAnimationTimer;
  public float timeSinceAttacking;
  public int playersKilled;
  private float timeSinceMakingWarningNoise;
  private float waitingWithDoorOpenTimer;
  private float timeSinceLoweringPatience;
  private bool inSellingItemsAnimation;

  private void Start()
  {
    this.grabObjectsTimer = this.grabObjectsWaitTime;
    this.CompanyLevelRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 39);
    this.SetCompanyMood(TimeOfDay.Instance.currentCompanyMood);
  }

  private void SetCompanyMood(CompanyMood mood)
  {
    this.currentMood = mood;
    this.doorWindowAudio.clip = mood.insideWindowSFX;
    this.doorWindowAudio.Play();
    this.patienceLevel = mood.startingPatience;
    this.StartCoroutine(this.waitForRoundToStart(mood));
  }

  private IEnumerator waitForRoundToStart(CompanyMood mood)
  {
    yield return (object) new WaitUntil((Func<bool>) (() => StartOfRound.Instance.shipDoorsEnabled));
    yield return (object) null;
    if ((UnityEngine.Object) mood.behindWallSFX != (UnityEngine.Object) null)
    {
      this.constantWallAudio.clip = mood.behindWallSFX;
      this.constantWallAudio.Play();
    }
  }

  public void PlaceItemOnCounter(PlayerControllerB playerWhoTriggered)
  {
    if (this.deskObjectsContainer.GetComponentsInChildren<GrabbableObject>().Length >= 12 || this.inGrabbingObjectsAnimation || !((UnityEngine.Object) GameNetworkManager.Instance != (UnityEngine.Object) null) || !((UnityEngine.Object) playerWhoTriggered == (UnityEngine.Object) GameNetworkManager.Instance.localPlayerController))
      return;
    Vector3 position = RoundManager.RandomPointInBounds(this.triggerCollider.bounds) with
    {
      y = this.triggerCollider.bounds.min.y
    };
    RaycastHit hitInfo;
    if (Physics.Raycast(new Ray(position + Vector3.up * 3f, Vector3.down), out hitInfo, 8f, 1048640, QueryTriggerInteraction.Collide))
      position = hitInfo.point;
    position.y += playerWhoTriggered.currentlyHeldObjectServer.itemProperties.verticalOffset;
    Vector3 placePosition = this.deskObjectsContainer.transform.InverseTransformPoint(position);
    this.AddObjectToDeskServerRpc((NetworkObjectReference) playerWhoTriggered.currentlyHeldObjectServer.gameObject.GetComponent<NetworkObject>());
    playerWhoTriggered.DiscardHeldObject(true, this.deskObjectsContainer, placePosition, false);
    Debug.Log((object) "discard held object called from deposit items desk");
  }

  [ServerRpc(RequireOwnership = false)]
  public void AddObjectToDeskServerRpc(NetworkObjectReference grabbableObjectNetObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(4150038830U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbableObjectNetObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 4150038830U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    if (grabbableObjectNetObject.TryGet(out this.lastObjectAddedToDesk))
    {
      if (this.itemsOnCounter.Contains(this.lastObjectAddedToDesk.GetComponentInChildren<GrabbableObject>()))
        return;
      this.itemsOnCounterNetworkObjects.Add(this.lastObjectAddedToDesk);
      this.itemsOnCounter.Add(this.lastObjectAddedToDesk.GetComponentInChildren<GrabbableObject>());
      this.AddObjectToDeskClientRpc(grabbableObjectNetObject);
      this.grabObjectsTimer = Mathf.Clamp(this.grabObjectsTimer + 6f, 0.0f, 10f);
      if (this.doorOpen || this.currentMood.mustBeWokenUp && (double) this.timesHearingNoise < 5.0)
        return;
      this.OpenShutDoorClientRpc();
    }
    else
      Debug.LogError((object) "ServerRpc: Could not find networkobject in the object that was placed on desk.");
  }

  [ClientRpc]
  public void AddObjectToDeskClientRpc(NetworkObjectReference grabbableObjectNetObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3889142070U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in grabbableObjectNetObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendClientRpc(ref bufferWriter, 3889142070U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    if (grabbableObjectNetObject.TryGet(out this.lastObjectAddedToDesk))
      this.lastObjectAddedToDesk.gameObject.GetComponentInChildren<GrabbableObject>().EnablePhysics(false);
    else
      Debug.LogError((object) "ClientRpc: Could not find networkobject in the object that was placed on desk.");
  }

  private void Update()
  {
    if ((UnityEngine.Object) NetworkManager.Singleton == (UnityEngine.Object) null)
      return;
    this.UpdateEffects();
    if (this.attacking)
    {
      if ((double) this.killAnimationTimer <= 0.0)
      {
        this.FinishKillAnimation();
      }
      else
      {
        TimeOfDay.Instance.TimeOfDayMusic.volume = Mathf.Lerp(TimeOfDay.Instance.TimeOfDayMusic.volume, 0.0f, 10f * Time.deltaTime);
        this.killAnimationTimer -= Time.deltaTime;
      }
    }
    this.triggerScript.interactable = GameNetworkManager.Instance.localPlayerController.isHoldingObject;
    GrabbableObject[] componentsInChildren = this.gameObject.GetComponentsInChildren<GrabbableObject>();
    for (int index = 0; index < componentsInChildren.Length; ++index)
    {
      if (componentsInChildren[index].grabbable)
        componentsInChildren[index].grabbable = false;
    }
    if (!this.IsServer)
      return;
    this.timeSinceAttacking += Time.deltaTime;
    if (this.itemsOnCounter.Count > 0 && !this.inGrabbingObjectsAnimation && !this.attacking)
    {
      if (!this.doorOpen)
        return;
      if ((double) this.grabObjectsTimer >= 0.0)
      {
        Debug.Log((object) string.Format("Desk: Waiting to grab the items on the desk; {0}", (object) this.grabObjectsTimer));
        this.grabObjectsTimer -= Time.deltaTime;
      }
      else
      {
        this.grabObjectsTimer = this.grabObjectsWaitTime;
        this.TakeItemsOffCounterOnServer();
      }
    }
    else
    {
      if ((double) this.timeSinceAttacking <= 25.0 || this.attacking || !this.doorOpen || this.itemsOnCounter.Count > 0)
        return;
      this.waitingWithDoorOpenTimer += Time.deltaTime;
      Debug.Log((object) string.Format("Desk: no objects on counter, waiting with door open; {0}", (object) this.waitingWithDoorOpenTimer));
      if ((double) this.waitingWithDoorOpenTimer <= 8.0 / (double) this.currentMood.irritability)
        return;
      this.waitingWithDoorOpenTimer = 0.0f;
      double patienceLevel = (double) this.patienceLevel;
      this.SetPatienceServerRpc(-1f * this.currentMood.irritability);
      double irritability = (double) this.currentMood.irritability;
      if (patienceLevel - irritability <= 0.0)
        return;
      this.OpenShutDoorClientRpc(false);
    }
  }

  private void UpdateEffects()
  {
    this.timeSinceLoweringPatience += Time.deltaTime;
    this.timeSinceMakingWarningNoise += Time.deltaTime;
    this.doorWindowAudio.volume = !this.doorOpen ? Mathf.Lerp(this.doorWindowAudio.volume, 0.0f, 10f * Time.deltaTime) : Mathf.Lerp(this.doorWindowAudio.volume, 1f * this.noiseBehindWallVolume, 3f * Time.deltaTime);
    if (this.attacking || this.currentMood.stopWallSFXWhenOpening && this.doorOpen)
      this.constantWallAudio.volume = Mathf.Lerp(this.constantWallAudio.volume, 0.0f, 15f * Time.deltaTime);
    else
      this.constantWallAudio.volume = Mathf.Lerp(this.constantWallAudio.volume, 1f, Time.deltaTime);
  }

  private void TakeItemsOffCounterOnServer()
  {
    this.inGrabbingObjectsAnimation = true;
    this.TakeObjectsClientRpc();
  }

  [ClientRpc]
  public void TakeObjectsClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3132539150U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3132539150U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.inGrabbingObjectsAnimation = true;
    this.depositDeskAnimator.SetBool("GrabbingItems", true);
    this.deskAudio.PlayOneShot(this.currentMood.grabItemsSFX[UnityEngine.Random.Range(0, this.currentMood.grabItemsSFX.Length)]);
  }

  public void SellItemsOnServer()
  {
    if (!this.IsServer)
      return;
    this.inSellingItemsAnimation = true;
    int num1 = 0;
    for (int index = 0; index < this.itemsOnCounter.Count; ++index)
    {
      if (!this.itemsOnCounter[index].itemProperties.isScrap)
      {
        if (this.itemsOnCounter[index].itemUsedUp)
          ;
      }
      else
        num1 += this.itemsOnCounter[index].scrapValue;
    }
    int num2 = (int) ((double) num1 * (double) StartOfRound.Instance.companyBuyingRate);
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    objectOfType.groupCredits += num2;
    this.SellItemsClientRpc(num2, objectOfType.groupCredits, this.itemsOnCounterAmount, StartOfRound.Instance.companyBuyingRate);
    this.SellAndDisplayItemProfits(num2, objectOfType.groupCredits);
  }

  [ClientRpc]
  public void SellItemsClientRpc(
    int itemProfit,
    int newGroupCredits,
    int itemsSold,
    float buyingRate)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3628265478U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, itemProfit);
      BytePacker.WriteValueBitPacked(bufferWriter, newGroupCredits);
      BytePacker.WriteValueBitPacked(bufferWriter, itemsSold);
      bufferWriter.WriteValueSafe<float>(in buyingRate, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3628265478U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || this.IsServer)
      return;
    this.itemsOnCounterAmount = itemsSold;
    StartOfRound.Instance.companyBuyingRate = buyingRate;
    this.SellAndDisplayItemProfits(itemProfit, newGroupCredits);
  }

  private void SellAndDisplayItemProfits(int profit, int newGroupCredits)
  {
    UnityEngine.Object.FindObjectOfType<Terminal>().groupCredits = newGroupCredits;
    StartOfRound.Instance.gameStats.scrapValueCollected += profit;
    TimeOfDay.Instance.quotaFulfilled += profit;
    GrabbableObject[] componentsInChildren = this.deskObjectsContainer.GetComponentsInChildren<GrabbableObject>();
    if (this.acceptItemsCoroutine != null)
      this.StopCoroutine(this.acceptItemsCoroutine);
    this.acceptItemsCoroutine = this.StartCoroutine(this.delayedAcceptanceOfItems(profit, componentsInChildren, newGroupCredits));
    this.CheckAllPlayersSoldItemsServerRpc();
  }

  [ServerRpc(RequireOwnership = false)]
  public void CheckAllPlayersSoldItemsServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1114072420U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 1114072420U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    ++this.clientsRecievedSellItemsRPC;
    if (this.clientsRecievedSellItemsRPC < GameNetworkManager.Instance.connectedPlayers)
      return;
    this.clientsRecievedSellItemsRPC = 0;
    for (int index = 0; index < this.itemsOnCounterNetworkObjects.Count; ++index)
    {
      if (this.itemsOnCounterNetworkObjects[index].IsSpawned)
        this.itemsOnCounterNetworkObjects[index].Despawn();
    }
    this.itemsOnCounterNetworkObjects.Clear();
    this.itemsOnCounter.Clear();
    this.FinishSellingItemsClientRpc();
  }

  [ClientRpc]
  public void FinishSellingItemsClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2469293577U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2469293577U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.depositDeskAnimator.SetBool("GrabbingItems", false);
    this.inGrabbingObjectsAnimation = false;
  }

  private IEnumerator delayedAcceptanceOfItems(
    int profit,
    GrabbableObject[] objectsOnDesk,
    int newGroupCredits)
  {
    DepositItemsDesk depositItemsDesk = this;
    // ISSUE: reference to a compiler-generated method
    yield return (object) new WaitUntil(new Func<bool>(depositItemsDesk.\u003CdelayedAcceptanceOfItems\u003Eb__59_0));
    depositItemsDesk.noiseBehindWallVolume = 0.3f;
    yield return (object) new WaitForSeconds(depositItemsDesk.currentMood.judgementSpeed);
    if ((double) (profit / Mathf.Max(objectsOnDesk.Length, 1)) <= 3.0 && (double) depositItemsDesk.patienceLevel <= 2.0)
    {
      System.Random random = new System.Random(objectsOnDesk.Length + newGroupCredits);
      if (!depositItemsDesk.attacking && random.Next(0, 100) < 30)
      {
        depositItemsDesk.Attack();
        // ISSUE: reference to a compiler-generated method
        yield return (object) new WaitUntil(new Func<bool>(depositItemsDesk.\u003CdelayedAcceptanceOfItems\u003Eb__59_1));
        yield return (object) new WaitForSeconds(2f);
      }
    }
    else
      depositItemsDesk.patienceLevel += 3f;
    depositItemsDesk.OpenShutDoor(false);
    yield return (object) new WaitForSeconds(0.5f);
    depositItemsDesk.noiseBehindWallVolume = 1f;
    HUDManager.Instance.DisplayCreditsEarning(profit, objectsOnDesk, newGroupCredits);
    depositItemsDesk.PlayRewardEffects(profit);
    yield return (object) new WaitForSeconds(1.25f);
    depositItemsDesk.MicrophoneSpeak();
    depositItemsDesk.inSellingItemsAnimation = false;
    depositItemsDesk.itemsOnCounterAmount = 0;
  }

  private void PlayRewardEffects(int profit)
  {
    Terminal objectOfType = UnityEngine.Object.FindObjectOfType<Terminal>();
    TimeOfDay.Instance.UpdateProfitQuotaCurrentTime();
    if ((double) profit < (double) objectOfType.groupCredits / 4.0)
      this.rewardsMusic.PlayOneShot(this.rewardBad);
    else
      this.rewardsMusic.PlayOneShot(this.rewardGood);
  }

  private void MicrophoneSpeak()
  {
    this.speakerAudio.PlayOneShot(this.CompanyLevelRandom.NextDouble() >= 0.029999999329447746 ? this.microphoneAudios[this.CompanyLevelRandom.Next(0, this.microphoneAudios.Length)] : this.rareMicrophoneAudios[this.CompanyLevelRandom.Next(0, this.rareMicrophoneAudios.Length)], 1f);
  }

  [ServerRpc(RequireOwnership = false)]
  public void AttackPlayersServerRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3230280218U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 3230280218U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.attacking || this.inGrabbingObjectsAnimation)
      return;
    this.attacking = true;
    this.AttackPlayersClientRpc();
  }

  [ClientRpc]
  public void AttackPlayersClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3277367259U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 3277367259U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.Attack();
  }

  public void Attack()
  {
    this.attacking = true;
    this.timeSinceAttacking = 0.0f;
    this.patienceLevel += 6f;
    if (!this.doorOpen)
      this.OpenShutDoor(true);
    for (int index = 0; index < this.monsterAnimations.Length; ++index)
    {
      if (this.currentMood.enableMonsterAnimationIndex == null)
      {
        Debug.Log((object) "Current company monster mood has no monster animations to enable.");
        this.attacking = false;
        return;
      }
      if (index == this.currentMood.enableMonsterAnimationIndex[index])
        this.monsterAnimations[index].monsterAnimator.SetBool("visible", true);
    }
    switch (this.currentMood.manifestation)
    {
      case CompanyMonster.GiantHand:
        Debug.Log((object) "Giant hand appears and searches");
        this.killAnimationTimer = 3f;
        break;
      case CompanyMonster.Tentacles:
        Debug.Log((object) "Tentacles appear");
        this.killAnimationTimer = 3f;
        break;
      case CompanyMonster.Tongue:
        Debug.Log((object) "Giant tongue appears");
        this.killAnimationTimer = 2f;
        break;
    }
    this.MakeLoudNoise(2);
  }

  public void CollisionDetect(int monsterAnimationID)
  {
    if (!this.attacking || this.monsterAnimations[monsterAnimationID].animatorCollidedOnClient)
      return;
    this.monsterAnimations[monsterAnimationID].animatorCollidedOnClient = true;
    if (this.IsServer)
      this.ConfirmAnimationGrabPlayerClientRpc(monsterAnimationID, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    else
      this.CheckAnimationGrabPlayerServerRpc(monsterAnimationID, (int) GameNetworkManager.Instance.localPlayerController.playerClientId);
    switch (this.currentMood.manifestation)
    {
      case CompanyMonster.GiantHand:
        Debug.Log((object) "Hand collision");
        break;
      case CompanyMonster.Tentacles:
        Debug.Log((object) "Tentacle collision");
        break;
      case CompanyMonster.Tongue:
        Debug.Log((object) "Tongue collision");
        break;
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void CheckAnimationGrabPlayerServerRpc(int monsterAnimationID, int playerID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1392297385U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, monsterAnimationID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerID);
      this.__endSendServerRpc(ref bufferWriter, 1392297385U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.monsterAnimations[monsterAnimationID].animatorCollidedOnClient)
      return;
    this.monsterAnimations[monsterAnimationID].animatorCollidedOnClient = true;
    this.ConfirmAnimationGrabPlayerClientRpc(monsterAnimationID, playerID);
  }

  [ClientRpc]
  public void ConfirmAnimationGrabPlayerClientRpc(int monsterAnimationID, int playerID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3034180067U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, monsterAnimationID);
      BytePacker.WriteValueBitPacked(bufferWriter, playerID);
      this.__endSendClientRpc(ref bufferWriter, 3034180067U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.monsterAnimations[monsterAnimationID].animatorCollidedOnClient = true;
    this.StartCoroutine(this.AnimationGrabPlayer(monsterAnimationID, playerID));
  }

  private IEnumerator AnimationGrabPlayer(int monsterAnimationID, int playerID)
  {
    Animator monsterAnimator = this.monsterAnimations[monsterAnimationID].monsterAnimator;
    Transform animatorGrabTarget = this.monsterAnimations[monsterAnimationID].monsterAnimatorGrabTarget;
    PlayerControllerB playerDying = StartOfRound.Instance.allPlayerScripts[playerID];
    monsterAnimator.SetBool("grabbingPlayer", true);
    Vector3 position = playerDying.transform.position;
    animatorGrabTarget.position = position;
    yield return (object) new WaitForSeconds(0.05f);
    if (playerDying.IsOwner)
      playerDying.KillPlayer(Vector3.zero);
    float startTime = Time.timeSinceLevelLoad;
    yield return (object) new WaitUntil((Func<bool>) (() => (UnityEngine.Object) playerDying.deadBody != (UnityEngine.Object) null || (double) Time.timeSinceLevelLoad - (double) startTime > 4.0));
    if ((UnityEngine.Object) playerDying.deadBody != (UnityEngine.Object) null)
    {
      playerDying.deadBody.attachedTo = this.monsterAnimations[monsterAnimationID].monsterAnimatorGrabPoint;
      playerDying.deadBody.attachedLimb = playerDying.deadBody.bodyParts[6];
      playerDying.deadBody.matchPositionExactly = true;
    }
    else
      Debug.Log((object) "Player body was not spawned in time for animation.");
    monsterAnimator.SetBool("grabbingPlayer", false);
    yield return (object) new WaitForSeconds(this.currentMood.grabPlayerAnimationTime);
    if ((UnityEngine.Object) playerDying.deadBody != (UnityEngine.Object) null)
    {
      playerDying.deadBody.attachedTo = (Transform) null;
      playerDying.deadBody.attachedLimb = (Rigidbody) null;
      playerDying.deadBody.matchPositionExactly = false;
      playerDying.deadBody.gameObject.SetActive(false);
    }
    ++this.playersKilled;
    if (this.playersKilled >= this.currentMood.maxPlayersToKillBeforeSatisfied)
      this.FinishKillAnimation();
  }

  public void FinishKillAnimation()
  {
    this.attacking = false;
    for (int index = 0; index < this.monsterAnimations.Length; ++index)
    {
      this.monsterAnimations[index].animatorCollidedOnClient = false;
      this.monsterAnimations[index].monsterAnimator.SetBool("visible", false);
    }
    switch (this.currentMood.manifestation)
    {
      case CompanyMonster.GiantHand:
        Debug.Log((object) "Hand finishing animation");
        break;
      case CompanyMonster.Tentacles:
        Debug.Log((object) "Tentacles finishing animation");
        break;
      case CompanyMonster.Tongue:
        Debug.Log((object) "Tongue finishing animation");
        break;
    }
    this.StartCoroutine(this.closeDoorAfterDelay());
  }

  private IEnumerator closeDoorAfterDelay()
  {
    yield return (object) new WaitForSeconds(1f);
    this.OpenShutDoor(false);
  }

  void INoiseListener.DetectNoise(
    Vector3 noisePosition,
    float noiseLoudness = 0.5f,
    int timesPlayedInOneSpot = 0,
    int noiseID = 0)
  {
    if (noiseID == 941 || (double) Vector3.Distance(this.triggerCollider.transform.position, noisePosition) > 9.0 || (double) noiseLoudness <= 0.40000000596046448)
      return;
    if (this.currentMood.mustBeWokenUp && !this.doorOpen)
    {
      this.SetTimesHeardNoiseServerRpc(this.currentMood.sensitivity * (noiseLoudness + 0.3f) / (float) (StartOfRound.Instance.connectedPlayersAmount + 1));
    }
    else
    {
      if (!this.currentMood.desiresSilence || (double) this.timeSinceLoweringPatience <= 1.0)
        return;
      this.SetPatienceServerRpc((float) (-1.0 * ((double) this.currentMood.irritability / 2.0)) * noiseLoudness);
    }
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetPatienceServerRpc(float valueChange)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(892728304U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in valueChange, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 892728304U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost || this.inSellingItemsAnimation)
      return;
    this.patienceLevel += valueChange;
    if ((double) this.patienceLevel <= 0.0)
    {
      if (this.attacking || this.inGrabbingObjectsAnimation)
        return;
      if (UnityEngine.Random.Range(0, 100) < 50)
      {
        this.attacking = true;
        this.AttackPlayersClientRpc();
      }
      else
      {
        this.patienceLevel += 3f;
        if (this.itemsOnCounter.Count > 0 || (double) this.timeSinceLoweringPatience <= 2.0)
          return;
        this.OpenShutDoorClientRpc(false);
      }
    }
    else
    {
      if ((double) valueChange >= 0.0 || (double) this.patienceLevel >= 1.0 || (double) this.timeSinceMakingWarningNoise <= 1.0)
        return;
      this.MakeWarningNoiseClientRpc();
    }
  }

  [ClientRpc]
  public void MakeWarningNoiseClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(103348088U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 103348088U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.timeSinceMakingWarningNoise = 0.0f;
    this.MakeLoudNoise(1);
  }

  [ServerRpc(RequireOwnership = false)]
  public void SetTimesHeardNoiseServerRpc(float valueChange)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(745684781U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<float>(in valueChange, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 745684781U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    Debug.Log((object) "NOISE D");
    this.timesHearingNoise += valueChange;
    if ((double) this.timesHearingNoise < 5.0 || this.doorOpen)
      return;
    this.timesHearingNoise = 0.0f;
    this.doorOpen = true;
    this.OpenShutDoorClientRpc();
    this.timeSinceLoweringPatience = 2.6f;
  }

  [ClientRpc]
  public void OpenShutDoorClientRpc(bool open = true)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1191125720U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<bool>(in open, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1191125720U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.OpenShutDoor(open);
  }

  public void OpenShutDoor(bool open)
  {
    this.doorOpen = open;
    this.depositDeskAnimator.SetBool("doorOpen", open);
    if (open)
      this.deskAudio.PlayOneShot(this.doorOpenSFX);
    else
      this.deskAudio.PlayOneShot(this.doorShutSFX);
  }

  public void MakeLoudNoise(int noise)
  {
    switch (noise)
    {
      case 1:
        this.wallAudio.PlayOneShot(this.rumbleSFX);
        if (this.doorOpen)
        {
          this.deskAudio.PlayOneShot(this.currentMood.angerSFX[this.angerSFXindex]);
          this.angerSFXindex = (this.angerSFXindex + 1) % this.currentMood.angerSFX.Length;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        break;
      case 2:
        int index = UnityEngine.Random.Range(0, this.currentMood.attackSFX.Length);
        this.deskAudio.PlayOneShot(this.currentMood.attackSFX[index]);
        WalkieTalkie.TransmitOneShotAudio(this.deskAudio, this.currentMood.attackSFX[index]);
        this.wallAudio.PlayOneShot(this.currentMood.wallAttackSFX);
        if ((double) Vector3.Distance(GameNetworkManager.Instance.localPlayerController.transform.position, this.deskAudio.transform.position) < 12.0)
        {
          HUDManager.Instance.ShakeCamera(ScreenShakeType.VeryStrong);
          break;
        }
        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        break;
      default:
        this.wallAudio.PlayOneShot(this.currentMood.noiseBehindWallSFX);
        HUDManager.Instance.ShakeCamera(ScreenShakeType.Big);
        break;
    }
  }

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_DepositItemsDesk()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(4150038830U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_4150038830)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3889142070U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3889142070)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3132539150U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3132539150)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3628265478U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3628265478)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1114072420U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_1114072420)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2469293577U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_2469293577)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3230280218U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3230280218)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3277367259U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3277367259)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1392297385U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_1392297385)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3034180067U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_3034180067)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(892728304U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_892728304)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(103348088U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_103348088)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(745684781U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_745684781)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1191125720U, new NetworkManager.RpcReceiveHandler(DepositItemsDesk.__rpc_handler_1191125720)));
  }

  private static void __rpc_handler_4150038830(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference grabbableObjectNetObject;
    reader.ReadValueSafe<NetworkObjectReference>(out grabbableObjectNetObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).AddObjectToDeskServerRpc(grabbableObjectNetObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3889142070(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference grabbableObjectNetObject;
    reader.ReadValueSafe<NetworkObjectReference>(out grabbableObjectNetObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).AddObjectToDeskClientRpc(grabbableObjectNetObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3132539150(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).TakeObjectsClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3628265478(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int itemProfit;
    ByteUnpacker.ReadValueBitPacked(reader, out itemProfit);
    int newGroupCredits;
    ByteUnpacker.ReadValueBitPacked(reader, out newGroupCredits);
    int itemsSold;
    ByteUnpacker.ReadValueBitPacked(reader, out itemsSold);
    float buyingRate;
    reader.ReadValueSafe<float>(out buyingRate, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).SellItemsClientRpc(itemProfit, newGroupCredits, itemsSold, buyingRate);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1114072420(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).CheckAllPlayersSoldItemsServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_2469293577(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).FinishSellingItemsClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3230280218(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).AttackPlayersServerRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3277367259(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).AttackPlayersClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1392297385(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int monsterAnimationID;
    ByteUnpacker.ReadValueBitPacked(reader, out monsterAnimationID);
    int playerID;
    ByteUnpacker.ReadValueBitPacked(reader, out playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).CheckAnimationGrabPlayerServerRpc(monsterAnimationID, playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_3034180067(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int monsterAnimationID;
    ByteUnpacker.ReadValueBitPacked(reader, out monsterAnimationID);
    int playerID;
    ByteUnpacker.ReadValueBitPacked(reader, out playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).ConfirmAnimationGrabPlayerClientRpc(monsterAnimationID, playerID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_892728304(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float valueChange;
    reader.ReadValueSafe<float>(out valueChange, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).SetPatienceServerRpc(valueChange);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_103348088(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).MakeWarningNoiseClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_745684781(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    float valueChange;
    reader.ReadValueSafe<float>(out valueChange, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((DepositItemsDesk) target).SetTimesHeardNoiseServerRpc(valueChange);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1191125720(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool open;
    reader.ReadValueSafe<bool>(out open, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((DepositItemsDesk) target).OpenShutDoorClientRpc(open);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (DepositItemsDesk);
}
