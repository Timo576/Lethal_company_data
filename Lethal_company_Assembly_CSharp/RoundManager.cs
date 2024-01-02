// Decompiled with JetBrains decompiler
// Type: RoundManager
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen;
using DunGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.AI.Navigation;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.SceneManagement;

#nullable disable
public class RoundManager : NetworkBehaviour
{
  public StartOfRound playersManager;
  public Transform syncedPropsContainer;
  public Transform itemPooledObjectsContainer;
  [Header("Global Game Variables / Balancing")]
  public float scrapValueMultiplier = 1f;
  public float scrapAmountMultiplier = 1f;
  public float mapSizeMultiplier = 1f;
  [Space(5f)]
  [Space(5f)]
  public int currentEnemyPower;
  public int currentOutsideEnemyPower;
  public int currentDaytimeEnemyPower;
  public TimeOfDay timeScript;
  private int currentHour;
  public float currentHourTime;
  [Header("Gameplay events")]
  public List<int> enemySpawnTimes = new List<int>();
  public int currentEnemySpawnIndex;
  public bool isSpawningEnemies;
  public bool begunSpawningEnemies;
  [Header("Elevator Properties")]
  public bool ElevatorCharging;
  public float elevatorCharge;
  public bool ElevatorPowered;
  public bool elevatorUp;
  public bool ElevatorLowering;
  public bool ElevatorRunning;
  public bool ReturnToSurface;
  [Header("Elevator Variables")]
  public Animator ElevatorAnimator;
  public Animator ElevatorLightAnimator;
  public AudioSource elevatorMotorAudio;
  public AudioClip startMotor;
  public Animator PanelButtons;
  public Animator PanelLights;
  public AudioSource elevatorButtonsAudio;
  public AudioClip PressButtonSFX1;
  public AudioClip PressButtonSFX2;
  public TextMeshProUGUI PanelScreenText;
  public Canvas PanelScreen;
  public NetworkObject lungPlacePosition;
  public InteractTrigger elevatorSocketTrigger;
  private Coroutine loadLevelCoroutine;
  private Coroutine flickerLightsCoroutine;
  private Coroutine powerLightsCoroutine;
  [Header("Enemies")]
  public EnemyVent[] allEnemyVents;
  public List<Anomaly> SpawnedAnomalies = new List<Anomaly>();
  public List<EnemyAI> SpawnedEnemies = new List<EnemyAI>();
  private List<int> SpawnProbabilities = new List<int>();
  public int hourTimeBetweenEnemySpawnBatches = 2;
  public int numberOfEnemiesInScene;
  public int minEnemiesToSpawn;
  public int minOutsideEnemiesToSpawn;
  [Header("Hazards")]
  public SpawnableMapObject[] spawnableMapObjects;
  public GameObject mapPropsContainer;
  public Transform[] shipSpawnPathPoints;
  public GameObject[] spawnDenialPoints;
  public string[] possibleCodesForBigDoors;
  public GameObject quicksandPrefab;
  public GameObject keyPrefab;
  [Space(5f)]
  public GameObject[] outsideAINodes;
  public GameObject[] insideAINodes;
  [Header("Dungeon generation")]
  public DungeonFlow[] dungeonFlowTypes;
  public RuntimeDungeon dungeonGenerator;
  public bool dungeonCompletedGenerating;
  public bool dungeonFinishedGeneratingForAllPlayers;
  public AudioClip[] firstTimeDungeonAudios;
  [Header("Scrap-collection")]
  public Transform spawnedScrapContainer;
  public int scrapCollectedInLevel;
  public float totalScrapValueInLevel;
  public int valueOfFoundScrapItems;
  public List<GrabbableObject> scrapCollectedThisRound = new List<GrabbableObject>();
  public SelectableLevel currentLevel;
  public System.Random LevelRandom;
  public System.Random AnomalyRandom;
  public System.Random AnomalyValuesRandom;
  public System.Random BreakerBoxRandom;
  public System.Random ScrapValuesRandom;
  public bool powerOffPermanently;
  public bool hasInitializedLevelRandomSeed;
  public List<ulong> playersFinishedGeneratingFloor = new List<ulong>(4);
  public PowerSwitchEvent onPowerSwitch = new PowerSwitchEvent();
  public List<Animator> allPoweredLightsAnimators = new List<Animator>();
  public List<Light> allPoweredLights = new List<Light>();
  public List<GameObject> spawnedSyncedObjects = new List<GameObject>();
  public float stabilityMeter;
  private Coroutine elevatorRunningCoroutine;
  public int collisionsMask = 2305;
  public bool cannotSpawnMoreInsideEnemies;
  public Collider[] tempColliderResults = new Collider[20];
  public Transform tempTransform;
  public bool GotNavMeshPositionResult;
  public NavMeshHit navHit;
  private bool firstTimeSpawningEnemies;
  private bool firstTimeSpawningOutsideEnemies;
  private bool firstTimeSpawningDaytimeEnemies;

  public static RoundManager Instance { get; private set; }

  private void Awake()
  {
    if ((UnityEngine.Object) RoundManager.Instance == (UnityEngine.Object) null)
      RoundManager.Instance = this;
    else
      UnityEngine.Object.Destroy((UnityEngine.Object) RoundManager.Instance.gameObject);
  }

  public void SpawnScrapInLevel()
  {
    List<Item> ScrapToSpawn = new List<Item>();
    List<int> intList1 = new List<int>();
    int num1 = 0;
    List<int> intList2 = new List<int>(this.currentLevel.spawnableScrap.Count);
    for (int index = 0; index < this.currentLevel.spawnableScrap.Count; ++index)
      intList2.Add(this.currentLevel.spawnableScrap[index].rarity);
    int[] array = intList2.ToArray();
    int num2 = (int) ((double) this.AnomalyRandom.Next(this.currentLevel.minScrap, this.currentLevel.maxScrap) * (double) this.scrapAmountMultiplier);
    for (int index = 0; index < num2; ++index)
      ScrapToSpawn.Add(this.currentLevel.spawnableScrap[this.GetRandomWeightedIndex(array)].spawnableItem);
    Debug.Log((object) string.Format("Number of scrap to spawn: {0}. minTotalScrapValue: {1}. Total value of items: {2}.", (object) ScrapToSpawn.Count, (object) this.currentLevel.minTotalScrapValue, (object) num1));
    RandomScrapSpawn randomScrapSpawn = (RandomScrapSpawn) null;
    RandomScrapSpawn[] objectsOfType = UnityEngine.Object.FindObjectsOfType<RandomScrapSpawn>();
    List<NetworkObjectReference> networkObjectReferenceList = new List<NetworkObjectReference>();
    List<RandomScrapSpawn> usedSpawns = new List<RandomScrapSpawn>();
    for (int i = 0; i < ScrapToSpawn.Count; i++)
    {
      if ((UnityEngine.Object) ScrapToSpawn[i] == (UnityEngine.Object) null)
      {
        Debug.Log((object) "Error!!!!! Found null element in list ScrapToSpawn. Skipping it.");
      }
      else
      {
        List<RandomScrapSpawn> randomScrapSpawnList = ScrapToSpawn[i].spawnPositionTypes == null || ScrapToSpawn[i].spawnPositionTypes.Count == 0 ? ((IEnumerable<RandomScrapSpawn>) objectsOfType).ToList<RandomScrapSpawn>() : ((IEnumerable<RandomScrapSpawn>) objectsOfType).Where<RandomScrapSpawn>((Func<RandomScrapSpawn, bool>) (x => ScrapToSpawn[i].spawnPositionTypes.Contains(x.spawnableItems) && !x.spawnUsed)).ToList<RandomScrapSpawn>();
        if (randomScrapSpawnList.Count <= 0)
        {
          Debug.Log((object) ("No tiles containing a scrap spawn with item type: " + ScrapToSpawn[i].itemName));
        }
        else
        {
          if (usedSpawns.Count > 0 && randomScrapSpawnList.Contains(randomScrapSpawn))
          {
            randomScrapSpawnList.RemoveAll((Predicate<RandomScrapSpawn>) (x => usedSpawns.Contains(x)));
            if (randomScrapSpawnList.Count <= 0)
            {
              usedSpawns.Clear();
              i--;
              continue;
            }
          }
          randomScrapSpawn = randomScrapSpawnList[this.AnomalyRandom.Next(0, randomScrapSpawnList.Count)];
          usedSpawns.Add(randomScrapSpawn);
          Vector3 position;
          if (randomScrapSpawn.spawnedItemsCopyPosition)
          {
            randomScrapSpawn.spawnUsed = true;
            position = randomScrapSpawn.transform.position;
          }
          else
            position = this.GetRandomNavMeshPositionInRadiusSpherical(randomScrapSpawn.transform.position, randomScrapSpawn.itemSpawnRange, this.navHit) + Vector3.up * ScrapToSpawn[i].verticalOffset;
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(ScrapToSpawn[i].spawnPrefab, position, Quaternion.identity, this.spawnedScrapContainer);
          GrabbableObject component1 = gameObject.GetComponent<GrabbableObject>();
          component1.transform.rotation = Quaternion.Euler(component1.itemProperties.restingRotation);
          component1.fallTime = 0.0f;
          intList1.Add((int) ((double) this.AnomalyRandom.Next(ScrapToSpawn[i].minValue, ScrapToSpawn[i].maxValue) * (double) this.scrapValueMultiplier));
          num1 += intList1[intList1.Count - 1];
          component1.scrapValue = intList1[intList1.Count - 1];
          NetworkObject component2 = gameObject.GetComponent<NetworkObject>();
          component2.Spawn();
          networkObjectReferenceList.Add((NetworkObjectReference) component2);
        }
      }
    }
    this.StartCoroutine(this.waitForScrapToSpawnToSync(networkObjectReferenceList.ToArray(), intList1.ToArray()));
  }

  private IEnumerator waitForScrapToSpawnToSync(
    NetworkObjectReference[] spawnedScrap,
    int[] scrapValues)
  {
    yield return (object) new WaitForSeconds(11f);
    this.SyncScrapValuesClientRpc(spawnedScrap, scrapValues);
  }

  [ClientRpc]
  public void SyncScrapValuesClientRpc(NetworkObjectReference[] spawnedScrap, int[] allScrapValue)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1659269112U, clientRpcParams, RpcDelivery.Reliable);
      bool flag1 = spawnedScrap != null;
      bufferWriter.WriteValueSafe<bool>(in flag1, new FastBufferWriter.ForPrimitives());
      if (flag1)
        bufferWriter.WriteValueSafe<NetworkObjectReference>(spawnedScrap, new FastBufferWriter.ForNetworkSerializable());
      bool flag2 = allScrapValue != null;
      bufferWriter.WriteValueSafe<bool>(in flag2, new FastBufferWriter.ForPrimitives());
      if (flag2)
        bufferWriter.WriteValueSafe<int>(allScrapValue, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 1659269112U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) string.Format("clientRPC scrap values length: {0}", (object) allScrapValue.Length));
    this.ScrapValuesRandom = new System.Random(StartOfRound.Instance.randomMapSeed + 210);
    int num = 0;
    for (int index = 0; index < spawnedScrap.Length; ++index)
    {
      NetworkObject networkObject;
      if (spawnedScrap[index].TryGet(out networkObject))
      {
        GrabbableObject component = networkObject.GetComponent<GrabbableObject>();
        if ((UnityEngine.Object) component != (UnityEngine.Object) null)
        {
          if (index >= allScrapValue.Length)
          {
            Debug.LogError((object) string.Format("spawnedScrap amount exceeded allScrapValue!: {0}", (object) spawnedScrap.Length));
            break;
          }
          component.SetScrapValue(allScrapValue[index]);
          num += allScrapValue[index];
          if (component.itemProperties.meshVariants.Length != 0)
            component.gameObject.GetComponent<MeshFilter>().mesh = component.itemProperties.meshVariants[this.ScrapValuesRandom.Next(0, component.itemProperties.meshVariants.Length)];
          try
          {
            if (component.itemProperties.materialVariants.Length != 0)
              component.gameObject.GetComponent<MeshRenderer>().sharedMaterial = component.itemProperties.materialVariants[this.ScrapValuesRandom.Next(0, component.itemProperties.materialVariants.Length)];
          }
          catch (Exception ex)
          {
            Debug.Log((object) string.Format("Item name: {0}; {1}", (object) component.gameObject.name, (object) ex));
          }
        }
        else
          Debug.LogError((object) ("Scrap networkobject object did not contain grabbable object!: " + networkObject.gameObject.name));
      }
      else
        Debug.LogError((object) string.Format("Failed to get networkobject reference for scrap. id: {0}", (object) spawnedScrap[index].NetworkObjectId));
    }
    this.totalScrapValueInLevel = (float) num;
    this.scrapCollectedInLevel = 0;
    this.valueOfFoundScrapItems = 0;
  }

  public void SpawnSyncedProps()
  {
    try
    {
      this.spawnedSyncedObjects.Clear();
      SpawnSyncedObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<SpawnSyncedObject>();
      if (objectsOfType == null)
        return;
      this.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
      Debug.Log((object) string.Format("Spawning synced props on server. Length: {0}", (object) objectsOfType.Length));
      for (int index = 0; index < objectsOfType.Length; ++index)
      {
        GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(objectsOfType[index].spawnPrefab, objectsOfType[index].transform.position, objectsOfType[index].transform.rotation, this.mapPropsContainer.transform);
        if ((UnityEngine.Object) gameObject != (UnityEngine.Object) null)
        {
          gameObject.GetComponent<NetworkObject>().Spawn(true);
          this.spawnedSyncedObjects.Add(gameObject);
        }
      }
    }
    catch (Exception ex)
    {
      Debug.Log((object) string.Format("Exception! Unable to sync spawned objects on host; {0}", (object) ex));
    }
  }

  public void SpawnMapObjects()
  {
    if (this.currentLevel.spawnableMapObjects.Length == 0)
      return;
    this.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
    RandomMapObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<RandomMapObject>();
    List<RandomMapObject> randomMapObjectList = new List<RandomMapObject>();
    for (int index1 = 0; index1 < this.currentLevel.spawnableMapObjects.Length; ++index1)
    {
      int num = (int) this.currentLevel.spawnableMapObjects[index1].numberToSpawn.Evaluate((float) this.AnomalyRandom.NextDouble());
      if (num > 0)
      {
        for (int index2 = 0; index2 < objectsOfType.Length; ++index2)
        {
          if (objectsOfType[index2].spawnablePrefabs.Contains(this.currentLevel.spawnableMapObjects[index1].prefabToSpawn))
            randomMapObjectList.Add(objectsOfType[index2]);
        }
        for (int index3 = 0; index3 < num; ++index3)
        {
          RandomMapObject randomMapObject = randomMapObjectList[this.AnomalyRandom.Next(0, randomMapObjectList.Count)];
          Vector3 positionInRadius = this.GetRandomNavMeshPositionInRadius(randomMapObject.transform.position, randomMapObject.spawnRange);
          GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.spawnableMapObjects[index1].prefabToSpawn, positionInRadius, Quaternion.identity, this.mapPropsContainer.transform);
          gameObject.transform.eulerAngles = !this.currentLevel.spawnableMapObjects[index1].spawnFacingAwayFromWall ? new Vector3(gameObject.transform.eulerAngles.x, (float) this.AnomalyRandom.Next(0, 360), gameObject.transform.eulerAngles.z) : new Vector3(0.0f, this.YRotationThatFacesTheFarthestFromPosition(positionInRadius + Vector3.up * 0.2f), 0.0f);
          gameObject.GetComponent<NetworkObject>().Spawn(true);
        }
      }
    }
    for (int index = 0; index < objectsOfType.Length; ++index)
      UnityEngine.Object.Destroy((UnityEngine.Object) objectsOfType[index].gameObject);
  }

  public float YRotationThatFacesTheFarthestFromPosition(
    Vector3 pos,
    float maxDistance = 25f,
    int resolution = 6)
  {
    int num1 = 0;
    float num2 = 0.0f;
    for (int y = 0; y < 360; y += 360 / resolution)
    {
      this.tempTransform.eulerAngles = new Vector3(0.0f, (float) y, 0.0f);
      RaycastHit hitInfo;
      if (Physics.Raycast(pos, this.tempTransform.forward, out hitInfo, maxDistance, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      {
        if ((double) hitInfo.distance > (double) num2)
        {
          num2 = hitInfo.distance;
          num1 = y;
        }
      }
      else
      {
        num1 = y;
        break;
      }
    }
    return !this.hasInitializedLevelRandomSeed ? (float) UnityEngine.Random.Range(num1 - 15, num1 + 15) : (float) this.AnomalyRandom.Next(num1 - 15, num1 + 15);
  }

  public float YRotationThatFacesTheNearestFromPosition(
    Vector3 pos,
    float maxDistance = 25f,
    int resolution = 6)
  {
    int num1 = 0;
    float num2 = 100f;
    bool flag = false;
    for (int y = 0; y < 360; y += 360 / resolution)
    {
      this.tempTransform.eulerAngles = new Vector3(0.0f, (float) y, 0.0f);
      RaycastHit hitInfo;
      if (Physics.Raycast(pos, this.tempTransform.forward, out hitInfo, maxDistance, StartOfRound.Instance.collidersAndRoomMask, QueryTriggerInteraction.Ignore))
      {
        flag = true;
        if ((double) hitInfo.distance < (double) num2)
        {
          num2 = hitInfo.distance;
          num1 = y;
        }
      }
      else
        break;
    }
    if (!flag)
      return -777f;
    return !this.hasInitializedLevelRandomSeed ? (float) UnityEngine.Random.Range(num1 - 15, num1 + 15) : (float) this.AnomalyRandom.Next(num1 - 15, num1 + 15);
  }

  public void GenerateNewFloor()
  {
    if (!this.hasInitializedLevelRandomSeed)
    {
      this.hasInitializedLevelRandomSeed = true;
      this.InitializeRandomNumberGenerators();
    }
    if (this.currentLevel.dungeonFlowTypes != null && this.currentLevel.dungeonFlowTypes.Length != 0)
    {
      List<int> intList = new List<int>();
      for (int index = 0; index < this.currentLevel.dungeonFlowTypes.Length; ++index)
        intList.Add(this.currentLevel.dungeonFlowTypes[index].rarity);
      int id = this.currentLevel.dungeonFlowTypes[this.GetRandomWeightedIndex(intList.ToArray(), this.LevelRandom)].id;
      this.dungeonGenerator.Generator.DungeonFlow = this.dungeonFlowTypes[id];
      if (id < this.firstTimeDungeonAudios.Length && (UnityEngine.Object) this.firstTimeDungeonAudios[id] != (UnityEngine.Object) null)
      {
        EntranceTeleport[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>();
        if (objectsOfType != null && objectsOfType.Length != 0)
        {
          for (int index = 0; index < objectsOfType.Length; ++index)
          {
            if (objectsOfType[index].isEntranceToBuilding)
            {
              objectsOfType[index].firstTimeAudio = this.firstTimeDungeonAudios[id];
              objectsOfType[index].dungeonFlowId = id;
            }
          }
        }
      }
    }
    this.dungeonGenerator.Generator.ShouldRandomizeSeed = false;
    this.dungeonGenerator.Generator.Seed = this.LevelRandom.Next();
    Debug.Log((object) string.Format("GenerateNewFloor(). Map generator's random seed: {0}", (object) this.dungeonGenerator.Generator.Seed));
    this.dungeonGenerator.Generator.LengthMultiplier = this.currentLevel.factorySizeMultiplier * this.mapSizeMultiplier;
    this.dungeonGenerator.Generate();
  }

  public void GeneratedFloorPostProcessing()
  {
    if (!this.IsServer)
      return;
    this.SpawnScrapInLevel();
    this.SpawnMapObjects();
  }

  public void TurnBreakerSwitchesOff()
  {
    BreakerBox objectOfType = UnityEngine.Object.FindObjectOfType<BreakerBox>();
    if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
      return;
    Debug.Log((object) "Switching breaker switches off");
    objectOfType.SetSwitchesOff();
    this.SwitchPower(false);
    this.onPowerSwitch.Invoke(false);
  }

  public void LoadNewLevel(int randomSeed, SelectableLevel newLevel)
  {
    if (!this.IsServer)
      return;
    this.currentLevel = newLevel;
    this.dungeonFinishedGeneratingForAllPlayers = false;
    this.playersManager.fullyLoadedPlayers.Clear();
    if ((UnityEngine.Object) this.dungeonGenerator != (UnityEngine.Object) null)
      this.dungeonGenerator.Generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.Generator_OnGenerationStatusChanged);
    if (this.loadLevelCoroutine != null)
      this.loadLevelCoroutine = (Coroutine) null;
    this.loadLevelCoroutine = this.StartCoroutine(this.LoadNewLevelWait(randomSeed));
  }

  private IEnumerator LoadNewLevelWait(int randomSeed)
  {
    RoundManager roundManager = this;
    yield return (object) null;
    yield return (object) null;
    roundManager.playersFinishedGeneratingFloor.Clear();
    roundManager.GenerateNewLevelClientRpc(randomSeed, roundManager.currentLevel.levelID);
    if (roundManager.currentLevel.spawnEnemiesAndScrap)
    {
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(roundManager.\u003CLoadNewLevelWait\u003Eb__106_0));
      yield return (object) null;
      // ISSUE: reference to a compiler-generated method
      yield return (object) new WaitUntil(new Func<bool>(roundManager.\u003CLoadNewLevelWait\u003Eb__106_1));
      Debug.Log((object) "Players finished generating the new floor");
    }
    yield return (object) new WaitForSeconds(0.3f);
    roundManager.SpawnSyncedProps();
    if (roundManager.currentLevel.spawnEnemiesAndScrap)
      roundManager.GeneratedFloorPostProcessing();
    yield return (object) null;
    roundManager.playersFinishedGeneratingFloor.Clear();
    roundManager.dungeonFinishedGeneratingForAllPlayers = true;
    roundManager.RefreshEnemyVents();
    roundManager.FinishGeneratingNewLevelClientRpc();
  }

  [ClientRpc]
  public void GenerateNewLevelClientRpc(int randomSeed, int levelID)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1193916134U, clientRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, randomSeed);
      BytePacker.WriteValueBitPacked(bufferWriter, levelID);
      this.__endSendClientRpc(ref bufferWriter, 1193916134U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.playersManager.randomMapSeed = randomSeed;
    this.currentLevel = this.playersManager.levels[levelID];
    this.hasInitializedLevelRandomSeed = false;
    HUDManager.Instance.loadingText.text = string.Format("Random seed: {0}", (object) randomSeed);
    HUDManager.Instance.loadingDarkenScreen.enabled = true;
    this.dungeonCompletedGenerating = false;
    this.mapPropsContainer = GameObject.FindGameObjectWithTag("MapPropsContainer");
    if (!this.currentLevel.spawnEnemiesAndScrap)
      return;
    this.dungeonGenerator = UnityEngine.Object.FindObjectOfType<RuntimeDungeon>(false);
    if ((UnityEngine.Object) this.dungeonGenerator != (UnityEngine.Object) null)
    {
      this.GenerateNewFloor();
      if (this.dungeonGenerator.Generator.Status == GenerationStatus.Complete)
      {
        this.FinishGeneratingLevel();
        Debug.Log((object) "Dungeon finished generating in one frame.");
      }
      else
      {
        this.dungeonGenerator.Generator.OnGenerationStatusChanged += new GenerationStatusDelegate(this.Generator_OnGenerationStatusChanged);
        Debug.Log((object) "Now listening to dungeon generator status.");
      }
    }
    else
      Debug.LogError((object) string.Format("This client could not find dungeon generator! scene count: {0}", (object) SceneManager.sceneCount));
  }

  private void FinishGeneratingLevel()
  {
    this.insideAINodes = GameObject.FindGameObjectsWithTag("AINode");
    this.outsideAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
    this.dungeonCompletedGenerating = true;
    this.SetToCurrentLevelWeather();
    this.SpawnOutsideHazards();
    this.FinishedGeneratingLevelServerRpc(NetworkManager.Singleton.LocalClientId);
  }

  private void Generator_OnGenerationStatusChanged(
    DungeonGenerator generator,
    GenerationStatus status)
  {
    if (status == GenerationStatus.Complete && !this.dungeonCompletedGenerating)
    {
      this.FinishGeneratingLevel();
      Debug.Log((object) "Dungeon has finished generating on this client after multiple frames");
    }
    this.dungeonGenerator.Generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.Generator_OnGenerationStatusChanged);
  }

  [ServerRpc(RequireOwnership = false)]
  public void FinishedGeneratingLevelServerRpc(ulong clientId)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(192551691U, serverRpcParams, RpcDelivery.Reliable);
      BytePacker.WriteValueBitPacked(bufferWriter, clientId);
      this.__endSendServerRpc(ref bufferWriter, 192551691U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.playersFinishedGeneratingFloor.Add(clientId);
  }

  public void DespawnPropsAtEndOfRound(bool despawnAllItems = false)
  {
    if (!this.IsServer)
      return;
    GrabbableObject[] objectsOfType = UnityEngine.Object.FindObjectsOfType<GrabbableObject>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (despawnAllItems || !objectsOfType[index].isHeld && !objectsOfType[index].isInShipRoom || objectsOfType[index].deactivated || StartOfRound.Instance.allPlayersDead && objectsOfType[index].itemProperties.isScrap)
      {
        if (objectsOfType[index].isHeld && (UnityEngine.Object) objectsOfType[index].playerHeldBy != (UnityEngine.Object) null)
          objectsOfType[index].playerHeldBy.DropAllHeldItems();
        objectsOfType[index].gameObject.GetComponent<NetworkObject>().Despawn();
      }
      else
        objectsOfType[index].scrapPersistedThroughRounds = true;
      if (this.spawnedSyncedObjects.Contains(objectsOfType[index].gameObject))
        this.spawnedSyncedObjects.Remove(objectsOfType[index].gameObject);
    }
    foreach (UnityEngine.Object @object in GameObject.FindGameObjectsWithTag("TemporaryEffect"))
      UnityEngine.Object.Destroy(@object);
  }

  public void UnloadSceneObjectsEarly()
  {
    if (!this.IsServer)
      return;
    Debug.Log((object) "Despawning props and enemies #3");
    this.isSpawningEnemies = false;
    EnemyAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
    Debug.Log((object) string.Format("Enemies on map: {0}", (object) objectsOfType.Length));
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].thisNetworkObject.IsSpawned)
      {
        Debug.Log((object) ("Despawning enemy: " + objectsOfType[index].gameObject.name));
        objectsOfType[index].thisNetworkObject.Despawn();
      }
      else
        Debug.Log((object) string.Format("{0} was not spawned on network, so it could not be removed.", (object) objectsOfType[index].thisNetworkObject));
    }
    this.SpawnedEnemies.Clear();
    this.currentEnemyPower = 0;
    this.currentDaytimeEnemyPower = 0;
    this.currentOutsideEnemyPower = 0;
  }

  public override void OnDestroy()
  {
    if ((UnityEngine.Object) this.dungeonGenerator != (UnityEngine.Object) null)
      this.dungeonGenerator.Generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.Generator_OnGenerationStatusChanged);
    base.OnDestroy();
  }

  [ServerRpc]
  public void FinishGeneratingNewLevelServerRpc()
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(710372063U, serverRpcParams, RpcDelivery.Reliable);
      this.__endSendServerRpc(ref bufferWriter, 710372063U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.FinishGeneratingNewLevelClientRpc();
  }

  private void SetToCurrentLevelWeather()
  {
    TimeOfDay.Instance.currentLevelWeather = this.currentLevel.currentWeather;
    if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.None || this.currentLevel.randomWeathers == null)
      return;
    for (int index = 0; index < this.currentLevel.randomWeathers.Length; ++index)
    {
      if (this.currentLevel.randomWeathers[index].weatherType == this.currentLevel.currentWeather)
      {
        TimeOfDay.Instance.currentWeatherVariable = this.currentLevel.randomWeathers[index].weatherVariable;
        TimeOfDay.Instance.currentWeatherVariable2 = this.currentLevel.randomWeathers[index].weatherVariable2;
      }
    }
  }

  [ClientRpc]
  public void FinishGeneratingNewLevelClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(2729232387U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 2729232387U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    HUDManager.Instance.loadingText.enabled = false;
    HUDManager.Instance.loadingDarkenScreen.enabled = false;
    this.RefreshLightsList();
    StartOfRound.Instance.StartCoroutine(this.playersManager.openingDoorsSequence());
    if (this.currentLevel.spawnEnemiesAndScrap)
      this.SetLevelObjectVariables();
    this.ResetEnemySpawningVariables();
    this.ResetEnemyTypesSpawnedCounts();
    this.playersManager.newGameIsLoading = false;
    FlashlightItem.globalFlashlightInterferenceLevel = 0;
    this.powerOffPermanently = false;
    this.RefreshEnemiesList();
    if (!StartOfRound.Instance.currentLevel.levelIncludesSnowFootprints)
      return;
    StartOfRound.Instance.InstantiateFootprintsPooledObjects();
  }

  private void ResetEnemySpawningVariables()
  {
    this.begunSpawningEnemies = false;
    this.currentHour = 0;
    this.cannotSpawnMoreInsideEnemies = false;
    this.minEnemiesToSpawn = 0;
    this.minOutsideEnemiesToSpawn = 0;
  }

  public void ResetEnemyVariables()
  {
    HoarderBugAI.grabbableObjectsInMap.Clear();
    HoarderBugAI.HoarderBugItems.Clear();
    BaboonBirdAI.baboonCampPosition = Vector3.zero;
  }

  public void CollectNewScrapForThisRound(GrabbableObject scrapObject)
  {
    if (!scrapObject.itemProperties.isScrap || this.scrapCollectedThisRound.Contains(scrapObject) || scrapObject.scrapPersistedThroughRounds)
      return;
    this.scrapCollectedThisRound.Add(scrapObject);
    HUDManager.Instance.AddNewScrapFoundToDisplay(scrapObject);
  }

  public void DetectElevatorIsRunning()
  {
    if (!this.IsServer)
      return;
    Debug.Log((object) "Ship is leaving. Despawning props and enemies.");
    if (this.elevatorRunningCoroutine != null)
      this.StopCoroutine(this.elevatorRunningCoroutine);
    this.elevatorRunningCoroutine = this.StartCoroutine(this.DetectElevatorRunning());
  }

  private IEnumerator DetectElevatorRunning()
  {
    this.isSpawningEnemies = false;
    yield return (object) new WaitForSeconds(1.5f);
    Debug.Log((object) "Despawning props and enemies #2");
    this.UnloadSceneObjectsEarly();
  }

  public void BeginEnemySpawning()
  {
    if (!this.IsServer)
      return;
    if (this.allEnemyVents.Length != 0 && this.currentLevel.maxEnemyPowerCount > 0)
    {
      this.currentEnemySpawnIndex = 0;
      this.PlotOutEnemiesForNextHour();
      this.isSpawningEnemies = true;
    }
    else
      Debug.Log((object) "Not able to spawn enemies on map; no vents were detected or maxEnemyPowerCount is 0.");
  }

  public void SpawnEnemiesOutside()
  {
    if (this.currentOutsideEnemyPower > this.currentLevel.maxOutsideEnemyPowerCount)
      return;
    float timeUpToCurrentHour = this.timeScript.lengthOfHours * (float) this.currentHour;
    float num1 = this.currentLevel.outsideEnemySpawnChanceThroughDay.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime);
    int num2 = Mathf.Clamp(this.AnomalyRandom.Next((int) ((double) (num1 + (float) Mathf.Abs(TimeOfDay.Instance.daysUntilDeadline - 3) / 1.6f) - 3.0), (int) ((double) num1 + 3.0)), this.minOutsideEnemiesToSpawn, 20);
    GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("OutsideAINode");
    for (int index = 0; index < num2; ++index)
    {
      GameObject gameObject = this.SpawnRandomOutsideEnemy(gameObjectsWithTag, timeUpToCurrentHour);
      if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
        break;
      this.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
      ++gameObject.GetComponent<EnemyAI>().enemyType.numberSpawned;
    }
  }

  public void SpawnDaytimeEnemiesOutside()
  {
    if (this.currentLevel.DaytimeEnemies == null || this.currentLevel.DaytimeEnemies.Count <= 0 || this.currentDaytimeEnemyPower > this.currentLevel.maxDaytimeEnemyPowerCount)
      return;
    float timeUpToCurrentHour = this.timeScript.lengthOfHours * (float) this.currentHour;
    float num1 = this.currentLevel.daytimeEnemySpawnChanceThroughDay.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime);
    Debug.Log((object) string.Format("base chance daytime: {0}", (object) this.currentLevel.daytimeEnemySpawnChanceThroughDay.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime)));
    Debug.Log((object) string.Format("timeuptocurrenthour: {0}; totalTime: {1}", (object) timeUpToCurrentHour, (object) this.timeScript.totalTime));
    int num2 = Mathf.Clamp(this.AnomalyRandom.Next((int) ((double) num1 - (double) this.currentLevel.daytimeEnemiesProbabilityRange), (int) ((double) num1 + (double) this.currentLevel.daytimeEnemiesProbabilityRange)), 0, 20);
    Debug.Log((object) string.Format("enemies to spawn daytime: {0}", (object) num2));
    GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("OutsideAINode");
    for (int index = 0; index < num2; ++index)
    {
      GameObject gameObject = this.SpawnRandomDaytimeEnemy(gameObjectsWithTag, timeUpToCurrentHour);
      if (!((UnityEngine.Object) gameObject != (UnityEngine.Object) null))
        break;
      this.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
      ++gameObject.GetComponent<EnemyAI>().enemyType.numberSpawned;
    }
  }

  private GameObject SpawnRandomDaytimeEnemy(GameObject[] spawnPoints, float timeUpToCurrentHour)
  {
    this.SpawnProbabilities.Clear();
    int num1 = 0;
    for (int index = 0; index < this.currentLevel.DaytimeEnemies.Count; ++index)
    {
      EnemyType enemyType = this.currentLevel.DaytimeEnemies[index].enemyType;
      if (this.firstTimeSpawningDaytimeEnemies)
        enemyType.numberSpawned = 0;
      Debug.Log((object) string.Format("dayimte enemy chance: {0}; {1}; {2}", (object) (enemyType.PowerLevel > this.currentLevel.maxDaytimeEnemyPowerCount - this.currentDaytimeEnemyPower), (object) (enemyType.numberSpawned >= this.currentLevel.DaytimeEnemies[index].enemyType.MaxCount), (object) ((double) enemyType.normalizedTimeInDayToLeave < (double) TimeOfDay.Instance.normalizedTimeOfDay)));
      if (enemyType.PowerLevel > this.currentLevel.maxDaytimeEnemyPowerCount - this.currentDaytimeEnemyPower || enemyType.numberSpawned >= this.currentLevel.DaytimeEnemies[index].enemyType.MaxCount || (double) enemyType.normalizedTimeInDayToLeave < (double) TimeOfDay.Instance.normalizedTimeOfDay)
      {
        this.SpawnProbabilities.Add(0);
      }
      else
      {
        int num2 = (int) ((double) this.currentLevel.DaytimeEnemies[index].rarity * (double) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime));
        Debug.Log((object) string.Format("Enemy available, probability: {0}; {1} * {2}", (object) num2, (object) this.currentLevel.DaytimeEnemies[index].rarity, (object) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime)));
        Debug.Log((object) string.Format("time up to current hour: {0}", (object) timeUpToCurrentHour));
        this.SpawnProbabilities.Add(num2);
        num1 += num2;
      }
    }
    this.firstTimeSpawningDaytimeEnemies = false;
    if (num1 <= 0)
    {
      int daytimeEnemyPower = this.currentDaytimeEnemyPower;
      int daytimeEnemyPowerCount = this.currentLevel.maxDaytimeEnemyPowerCount;
      return (GameObject) null;
    }
    int randomWeightedIndex = this.GetRandomWeightedIndex(this.SpawnProbabilities.ToArray());
    this.currentDaytimeEnemyPower += this.currentLevel.DaytimeEnemies[randomWeightedIndex].enemyType.PowerLevel;
    Vector3 positionInRadius = this.GetRandomNavMeshPositionInRadius(spawnPoints[this.AnomalyRandom.Next(0, spawnPoints.Length)].transform.position, 4f);
    int index1 = 0;
    bool flag = false;
    for (int index2 = 0; index2 < spawnPoints.Length - 1; ++index2)
    {
      for (int index3 = 0; index3 < this.spawnDenialPoints.Length; ++index3)
      {
        flag = true;
        if ((double) Vector3.Distance(positionInRadius, this.spawnDenialPoints[index3].transform.position) < 8.0)
        {
          index1 = (index1 + 1) % spawnPoints.Length;
          positionInRadius = this.GetRandomNavMeshPositionInRadius(spawnPoints[index1].transform.position, 4f);
          flag = false;
          break;
        }
      }
      if (flag)
        break;
    }
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.DaytimeEnemies[randomWeightedIndex].enemyType.enemyPrefab, positionInRadius, Quaternion.Euler(Vector3.zero));
    gameObject.gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
    return gameObject;
  }

  private GameObject SpawnRandomOutsideEnemy(GameObject[] spawnPoints, float timeUpToCurrentHour)
  {
    this.SpawnProbabilities.Clear();
    int num1 = 0;
    for (int index = 0; index < this.currentLevel.OutsideEnemies.Count; ++index)
    {
      EnemyType enemyType = this.currentLevel.OutsideEnemies[index].enemyType;
      if (this.firstTimeSpawningOutsideEnemies)
        enemyType.numberSpawned = 0;
      if (enemyType.PowerLevel > this.currentLevel.maxOutsideEnemyPowerCount - this.currentOutsideEnemyPower || enemyType.numberSpawned >= enemyType.MaxCount)
      {
        this.SpawnProbabilities.Add(0);
      }
      else
      {
        int num2;
        if (enemyType.useNumberSpawnedFalloff)
        {
          num2 = (int) ((double) this.currentLevel.OutsideEnemies[index].rarity * ((double) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime) * (double) enemyType.numberSpawnedFalloff.Evaluate((float) enemyType.numberSpawned / 10f)));
          Debug.Log((object) string.Format("Enemy '{0}' rarity: {1}; time multiplier: {2}", (object) this.currentLevel.OutsideEnemies[index].enemyType.enemyName, (object) this.currentLevel.OutsideEnemies[index].rarity, (object) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime)));
          Debug.Log((object) string.Format("Enemy probability without number falloff: {0}", (object) (int) ((double) this.currentLevel.OutsideEnemies[index].rarity * (double) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime))));
          Debug.Log((object) string.Format("Enemy number falloff probability: {0}; number falloff multiplier y: {1}; x : {2}", (object) num2, (object) enemyType.numberSpawnedFalloff.Evaluate((float) enemyType.numberSpawned / 10f), (object) (float) ((double) enemyType.numberSpawned / 10.0)));
        }
        else
          num2 = (int) ((double) this.currentLevel.OutsideEnemies[index].rarity * (double) enemyType.probabilityCurve.Evaluate(timeUpToCurrentHour / this.timeScript.totalTime));
        this.SpawnProbabilities.Add(num2);
        num1 += num2;
      }
    }
    this.firstTimeSpawningOutsideEnemies = false;
    if (num1 <= 0)
    {
      int outsideEnemyPower = this.currentOutsideEnemyPower;
      int outsideEnemyPowerCount = this.currentLevel.maxOutsideEnemyPowerCount;
      return (GameObject) null;
    }
    int randomWeightedIndex = this.GetRandomWeightedIndex(this.SpawnProbabilities.ToArray());
    this.currentOutsideEnemyPower += this.currentLevel.OutsideEnemies[randomWeightedIndex].enemyType.PowerLevel;
    Vector3 positionInRadius = this.GetRandomNavMeshPositionInRadius(spawnPoints[this.AnomalyRandom.Next(0, spawnPoints.Length)].transform.position, 4f);
    int index1 = 0;
    bool flag = false;
    for (int index2 = 0; index2 < spawnPoints.Length - 1; ++index2)
    {
      for (int index3 = 0; index3 < this.spawnDenialPoints.Length; ++index3)
      {
        flag = true;
        if ((double) Vector3.Distance(positionInRadius, this.spawnDenialPoints[index3].transform.position) < 16.0)
        {
          index1 = (index1 + 1) % spawnPoints.Length;
          positionInRadius = this.GetRandomNavMeshPositionInRadius(spawnPoints[index1].transform.position, 4f);
          flag = false;
          break;
        }
      }
      if (flag)
        break;
    }
    GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.OutsideEnemies[randomWeightedIndex].enemyType.enemyPrefab, positionInRadius, Quaternion.Euler(Vector3.zero));
    gameObject.gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
    return gameObject;
  }

  public void PlotOutEnemiesForNextHour()
  {
    if (!this.IsServer)
      return;
    List<EnemyVent> enemyVentList = new List<EnemyVent>();
    for (int index = 0; index < this.allEnemyVents.Length; ++index)
    {
      if (!this.allEnemyVents[index].occupied)
        enemyVentList.Add(this.allEnemyVents[index]);
    }
    this.enemySpawnTimes.Clear();
    float num1 = this.currentLevel.enemySpawnChanceThroughoutDay.Evaluate(this.timeScript.currentDayTime / this.timeScript.totalTime);
    int num2 = Mathf.Clamp(this.AnomalyRandom.Next((int) ((double) (num1 + (float) Mathf.Abs(TimeOfDay.Instance.daysUntilDeadline - 3) / 1.6f) - (double) this.currentLevel.spawnProbabilityRange), (int) ((double) num1 + (double) this.currentLevel.spawnProbabilityRange)), this.minEnemiesToSpawn, 20);
    Debug.Log((object) string.Format("spawn chance range min/max : {0}, {1}; time of day percent: {2}", (object) (int) ((double) num1 - (double) this.currentLevel.spawnProbabilityRange), (object) (int) ((double) num1 + (double) this.currentLevel.spawnProbabilityRange), (object) (float) ((double) this.timeScript.currentDayTime / (double) this.timeScript.totalTime)));
    Debug.Log((object) string.Format("spawn chance range min/max (non integer) : {0}, {1}", (object) (float) ((double) num1 - (double) this.currentLevel.spawnProbabilityRange), (object) (float) ((double) num1 + (double) this.currentLevel.spawnProbabilityRange)));
    int num3 = Mathf.Clamp(num2, 0, enemyVentList.Count);
    Debug.Log((object) string.Format("enemies to spawn: {0}", (object) num3));
    if (this.currentEnemyPower >= this.currentLevel.maxEnemyPowerCount)
    {
      this.cannotSpawnMoreInsideEnemies = true;
    }
    else
    {
      float num4 = this.timeScript.lengthOfHours * (float) this.currentHour;
      for (int index = 0; index < num3; ++index)
      {
        int spawnTime = this.AnomalyRandom.Next((int) (10.0 + (double) num4), (int) ((double) this.timeScript.lengthOfHours * (double) this.hourTimeBetweenEnemySpawnBatches + (double) num4));
        if (this.AssignRandomEnemyToVent(enemyVentList[this.AnomalyRandom.Next(enemyVentList.Count)], (float) spawnTime))
          this.enemySpawnTimes.Add(spawnTime);
        else
          break;
      }
      this.enemySpawnTimes.Sort();
    }
  }

  public void LogEnemySpawnTimes(bool couldNotFinish)
  {
    if (couldNotFinish)
      Debug.Log((object) "Stopped assigning enemies to vents early as there was no enemy with a power count low enough to fit.");
    Debug.Log((object) "Enemy spawn times:");
    for (int index = 0; index < this.enemySpawnTimes.Count; ++index)
      Debug.Log((object) string.Format("time {0}: {1}", (object) index, (object) this.enemySpawnTimes[index]));
  }

  private bool AssignRandomEnemyToVent(EnemyVent vent, float spawnTime)
  {
    this.SpawnProbabilities.Clear();
    int num1 = 0;
    for (int index = 0; index < this.currentLevel.Enemies.Count; ++index)
    {
      EnemyType enemyType = this.currentLevel.Enemies[index].enemyType;
      if (this.firstTimeSpawningEnemies)
      {
        Debug.Log((object) ("Setting enemy numberspawned to 0 to start: " + enemyType.enemyName));
        enemyType.numberSpawned = 0;
      }
      if (this.EnemyCannotBeSpawned(index))
      {
        this.SpawnProbabilities.Add(0);
        Debug.Log((object) string.Format("enemy #{0} probability - {1}", (object) index, (object) 0));
        Debug.Log((object) string.Format("Enemy {0} could not be spawned. current power count is {1}; max is {2}.", (object) index, (object) this.currentEnemyPower, (object) this.currentLevel.maxEnemyPowerCount));
      }
      else
      {
        int num2 = !enemyType.useNumberSpawnedFalloff ? (int) ((double) this.currentLevel.Enemies[index].rarity * (double) enemyType.probabilityCurve.Evaluate(this.timeScript.normalizedTimeOfDay)) : (int) ((double) this.currentLevel.Enemies[index].rarity * ((double) enemyType.probabilityCurve.Evaluate(this.timeScript.normalizedTimeOfDay) * (double) enemyType.numberSpawnedFalloff.Evaluate((float) enemyType.numberSpawned / 10f)));
        this.SpawnProbabilities.Add(num2);
        Debug.Log((object) string.Format("enemy #{0} probability - {1}", (object) index, (object) num2));
        num1 += num2;
      }
    }
    this.firstTimeSpawningEnemies = false;
    if (num1 <= 0)
    {
      if (this.currentEnemyPower >= this.currentLevel.maxEnemyPowerCount)
      {
        Debug.Log((object) string.Format("Round manager: No more spawnable enemies. Power count: {0} Max: {1}", (object) this.currentLevel.maxEnemyPowerCount, (object) this.currentLevel.maxEnemyPowerCount));
        this.cannotSpawnMoreInsideEnemies = true;
      }
      return false;
    }
    int randomWeightedIndex = this.GetRandomWeightedIndex(this.SpawnProbabilities.ToArray());
    Debug.Log((object) string.Format("ADDING ENEMY #{0}: {1}", (object) randomWeightedIndex, (object) this.currentLevel.Enemies[randomWeightedIndex].enemyType.enemyName));
    Debug.Log((object) string.Format("Adding {0} to power level, enemy: {1}", (object) this.currentLevel.Enemies[randomWeightedIndex].enemyType.PowerLevel, (object) this.currentLevel.Enemies[randomWeightedIndex].enemyType.enemyName));
    this.currentEnemyPower += this.currentLevel.Enemies[randomWeightedIndex].enemyType.PowerLevel;
    vent.enemyType = this.currentLevel.Enemies[randomWeightedIndex].enemyType;
    vent.enemyTypeIndex = randomWeightedIndex;
    vent.occupied = true;
    vent.spawnTime = spawnTime;
    if (this.timeScript.hour - this.currentHour > 0)
      Debug.Log((object) "RoundManager is catching up to current time! Not syncing vent SFX with clients since enemy will spawn from vent almost immediately.");
    else
      vent.SyncVentSpawnTimeClientRpc((int) spawnTime, randomWeightedIndex);
    ++this.currentLevel.Enemies[randomWeightedIndex].enemyType.numberSpawned;
    return true;
  }

  private bool EnemyCannotBeSpawned(int enemyIndex)
  {
    return this.currentLevel.Enemies[enemyIndex].enemyType.PowerLevel > this.currentLevel.maxEnemyPowerCount - this.currentEnemyPower || this.currentLevel.Enemies[enemyIndex].enemyType.numberSpawned >= this.currentLevel.Enemies[enemyIndex].enemyType.MaxCount;
  }

  public void InitializeRandomNumberGenerators()
  {
    SoundManager.Instance.InitializeRandom();
    this.LevelRandom = new System.Random(this.playersManager.randomMapSeed);
    this.AnomalyRandom = new System.Random(this.playersManager.randomMapSeed + 5);
    this.AnomalyValuesRandom = new System.Random(this.playersManager.randomMapSeed - 40);
    this.BreakerBoxRandom = new System.Random(this.playersManager.randomMapSeed - 20);
  }

  public void SpawnEnemyFromVent(EnemyVent vent)
  {
    this.SpawnEnemyOnServer(vent.floorNode.position, vent.floorNode.eulerAngles.y, vent.enemyTypeIndex);
    Debug.Log((object) "Spawned enemy from vent");
    vent.OpenVentClientRpc();
    vent.occupied = false;
  }

  public void SpawnEnemyOnServer(Vector3 spawnPosition, float yRot, int enemyNumber = -1)
  {
    if (!this.IsServer)
      this.SpawnEnemyServerRpc(spawnPosition, yRot, enemyNumber);
    else
      this.SpawnEnemyGameObject(spawnPosition, yRot, enemyNumber);
  }

  [ServerRpc]
  public void SpawnEnemyServerRpc(Vector3 spawnPosition, float yRot, int enemyNumber)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(46494176U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in spawnPosition);
      bufferWriter.WriteValueSafe<float>(in yRot, new FastBufferWriter.ForPrimitives());
      BytePacker.WriteValueBitPacked(bufferWriter, enemyNumber);
      this.__endSendServerRpc(ref bufferWriter, 46494176U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.SpawnEnemyGameObject(spawnPosition, yRot, enemyNumber);
  }

  public NetworkObjectReference SpawnEnemyGameObject(
    Vector3 spawnPosition,
    float yRot,
    int enemyNumber,
    EnemyType enemyType = null)
  {
    if ((UnityEngine.Object) enemyType != (UnityEngine.Object) null)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0.0f, yRot, 0.0f)));
      gameObject.GetComponentInChildren<NetworkObject>().Spawn(true);
      this.SpawnedEnemies.Add(gameObject.GetComponent<EnemyAI>());
      return (NetworkObjectReference) gameObject.GetComponentInChildren<NetworkObject>();
    }
    int index = enemyNumber;
    switch (enemyNumber)
    {
      case -3:
        GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.OutsideEnemies[UnityEngine.Random.Range(0, this.currentLevel.OutsideEnemies.Count)].enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0.0f, yRot, 0.0f)));
        gameObject1.GetComponentInChildren<NetworkObject>().Spawn(true);
        this.SpawnedEnemies.Add(gameObject1.GetComponent<EnemyAI>());
        return (NetworkObjectReference) gameObject1.GetComponentInChildren<NetworkObject>();
      case -2:
        GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.DaytimeEnemies[UnityEngine.Random.Range(0, this.currentLevel.DaytimeEnemies.Count)].enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0.0f, yRot, 0.0f)));
        gameObject2.GetComponentInChildren<NetworkObject>().Spawn(true);
        this.SpawnedEnemies.Add(gameObject2.GetComponent<EnemyAI>());
        return (NetworkObjectReference) gameObject2.GetComponentInChildren<NetworkObject>();
      case -1:
        index = UnityEngine.Random.Range(0, this.currentLevel.Enemies.Count);
        break;
    }
    GameObject gameObject3 = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.Enemies[index].enemyType.enemyPrefab, spawnPosition, Quaternion.Euler(new Vector3(0.0f, yRot, 0.0f)));
    gameObject3.GetComponentInChildren<NetworkObject>().Spawn(true);
    this.SpawnedEnemies.Add(gameObject3.GetComponent<EnemyAI>());
    return (NetworkObjectReference) gameObject3.GetComponentInChildren<NetworkObject>();
  }

  public void DespawnEnemyOnServer(NetworkObject enemyNetworkObject)
  {
    if (!this.IsServer)
      this.DespawnEnemyServerRpc((NetworkObjectReference) enemyNetworkObject);
    else
      this.DespawnEnemyGameObject((NetworkObjectReference) enemyNetworkObject);
  }

  [ServerRpc(RequireOwnership = false)]
  public void DespawnEnemyServerRpc(NetworkObjectReference enemyNetworkObject)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server && (networkManager.IsClient || networkManager.IsHost))
    {
      ServerRpcParams serverRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(3840785488U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in enemyNetworkObject, new FastBufferWriter.ForNetworkSerializable());
      this.__endSendServerRpc(ref bufferWriter, 3840785488U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.DespawnEnemyGameObject(enemyNetworkObject);
  }

  private void DespawnEnemyGameObject(NetworkObjectReference enemyNetworkObject)
  {
    NetworkObject networkObject;
    if (enemyNetworkObject.TryGet(out networkObject))
    {
      EnemyAI component = networkObject.gameObject.GetComponent<EnemyAI>();
      this.SpawnedEnemies.Remove(component);
      if (component.enemyType.isOutsideEnemy)
        this.currentOutsideEnemyPower -= component.enemyType.PowerLevel;
      else if (component.enemyType.isDaytimeEnemy)
        this.currentDaytimeEnemyPower -= component.enemyType.PowerLevel;
      else
        this.currentEnemyPower -= component.enemyType.PowerLevel;
      this.cannotSpawnMoreInsideEnemies = false;
      component.gameObject.GetComponent<NetworkObject>().Despawn();
    }
    else
      Debug.LogError((object) "Round manager despawn enemy gameobject: Could not get network object from reference!");
  }

  public void SwitchPower(bool on)
  {
    if (!this.IsServer)
      return;
    if (on)
    {
      if (this.powerOffPermanently)
        return;
      this.PowerSwitchOnClientRpc();
    }
    else
      this.PowerSwitchOffClientRpc();
  }

  [ClientRpc]
  public void PowerSwitchOnClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1061166170U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1061166170U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    this.onPowerSwitch.Invoke(true);
    this.TurnOnAllLights(true);
  }

  [ClientRpc]
  public void PowerSwitchOffClientRpc()
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(1586488299U, clientRpcParams, RpcDelivery.Reliable);
      this.__endSendClientRpc(ref bufferWriter, 1586488299U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    Debug.Log((object) "Calling power switch off event from roundmanager");
    this.onPowerSwitch.Invoke(false);
    this.TurnOnAllLights(false);
  }

  public void TurnOnAllLights(bool on)
  {
    if (this.powerLightsCoroutine != null)
      this.StopCoroutine(this.powerLightsCoroutine);
    this.powerLightsCoroutine = this.StartCoroutine(this.turnOnLights(on));
  }

  private IEnumerator turnOnLights(bool turnOn)
  {
    yield return (object) null;
    BreakerBox objectOfType = UnityEngine.Object.FindObjectOfType<BreakerBox>();
    if ((UnityEngine.Object) objectOfType != (UnityEngine.Object) null)
    {
      objectOfType.thisAudioSource.PlayOneShot(objectOfType.switchPowerSFX);
      objectOfType.isPowerOn = turnOn;
    }
    for (int b = 4; b > 0; --b)
    {
      if (b == 0)
      {
        Debug.Log((object) "b is 0 in loop");
        break;
      }
      for (int index = 0; index < this.allPoweredLightsAnimators.Count / b; ++index)
        this.allPoweredLightsAnimators[index].SetBool("on", turnOn);
      yield return (object) new WaitForSeconds(0.03f);
    }
  }

  public void FlickerLights(bool flickerFlashlights = false, bool disableFlashlights = false)
  {
    if (this.flickerLightsCoroutine != null)
      this.StopCoroutine(this.flickerLightsCoroutine);
    this.flickerLightsCoroutine = this.StartCoroutine(this.FlickerPoweredLights(flickerFlashlights, disableFlashlights));
  }

  private IEnumerator FlickerPoweredLights(bool flickerFlashlights = false, bool disableFlashlights = false)
  {
    Debug.Log((object) "Flickering lights");
    if (flickerFlashlights)
    {
      Debug.Log((object) "Flickering flashlights");
      FlashlightItem.globalFlashlightInterferenceLevel = 1;
      FlashlightItem[] objectsOfType = UnityEngine.Object.FindObjectsOfType<FlashlightItem>();
      if (objectsOfType != null)
      {
        for (int index = 0; index < objectsOfType.Length; ++index)
        {
          objectsOfType[index].flashlightAudio.PlayOneShot(objectsOfType[index].flashlightFlicker);
          WalkieTalkie.TransmitOneShotAudio(objectsOfType[index].flashlightAudio, objectsOfType[index].flashlightFlicker, 0.8f);
          if (disableFlashlights && (UnityEngine.Object) objectsOfType[index].playerHeldBy != (UnityEngine.Object) null && objectsOfType[index].playerHeldBy.isInsideFactory)
            objectsOfType[index].flashlightInterferenceLevel = 2;
        }
      }
    }
    if (this.allPoweredLightsAnimators.Count > 0 && (UnityEngine.Object) this.allPoweredLightsAnimators[0] != (UnityEngine.Object) null)
    {
      int loopCount = 0;
      for (int b = 4; b > 0 && b != 0; --b)
      {
        for (int index = loopCount; index < this.allPoweredLightsAnimators.Count / b; ++index)
        {
          ++loopCount;
          this.allPoweredLightsAnimators[index].SetTrigger("Flicker");
        }
        yield return (object) new WaitForSeconds(0.05f);
      }
    }
    if (flickerFlashlights)
    {
      yield return (object) new WaitForSeconds(0.3f);
      FlashlightItem[] objectsOfType = UnityEngine.Object.FindObjectsOfType<FlashlightItem>();
      if (objectsOfType != null)
      {
        for (int index = 0; index < objectsOfType.Length; ++index)
          objectsOfType[index].flashlightInterferenceLevel = 0;
      }
      FlashlightItem.globalFlashlightInterferenceLevel = 0;
    }
  }

  private void Start()
  {
    this.RefreshLightsList();
    this.RefreshEnemyVents();
    this.timeScript = UnityEngine.Object.FindObjectOfType<TimeOfDay>();
    FlashlightItem.globalFlashlightInterferenceLevel = 0;
    this.navHit = new NavMeshHit();
    if (!((UnityEngine.Object) StartOfRound.Instance.testRoom != (UnityEngine.Object) null))
      return;
    this.outsideAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
  }

  private void ResetEnemyTypesSpawnedCounts()
  {
    EnemyAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
    for (int index1 = 0; index1 < this.currentLevel.Enemies.Count; ++index1)
    {
      this.currentLevel.Enemies[index1].enemyType.numberSpawned = 0;
      for (int index2 = 0; index2 < objectsOfType.Length; ++index2)
      {
        if ((UnityEngine.Object) objectsOfType[index2].enemyType == (UnityEngine.Object) this.currentLevel.Enemies[index1].enemyType)
          ++this.currentLevel.Enemies[index1].enemyType.numberSpawned;
      }
    }
    for (int index3 = 0; index3 < this.currentLevel.OutsideEnemies.Count; ++index3)
    {
      this.currentLevel.OutsideEnemies[index3].enemyType.numberSpawned = 0;
      for (int index4 = 0; index4 < objectsOfType.Length; ++index4)
      {
        if ((UnityEngine.Object) objectsOfType[index4].enemyType == (UnityEngine.Object) this.currentLevel.OutsideEnemies[index3].enemyType)
          ++this.currentLevel.OutsideEnemies[index3].enemyType.numberSpawned;
      }
    }
  }

  private void RefreshEnemiesList()
  {
    this.SpawnedEnemies.Clear();
    EnemyAI[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EnemyAI>();
    this.SpawnedEnemies.AddRange((IEnumerable<EnemyAI>) objectsOfType);
    this.numberOfEnemiesInScene = objectsOfType.Length;
    this.firstTimeSpawningEnemies = true;
    this.firstTimeSpawningOutsideEnemies = true;
    this.firstTimeSpawningDaytimeEnemies = true;
  }

  private void Update()
  {
    if (!this.IsServer || !this.dungeonFinishedGeneratingForAllPlayers)
      return;
    if (this.isSpawningEnemies)
    {
      this.SpawnInsideEnemiesFromVentsIfReady();
      if (this.timeScript.hour <= this.currentHour || this.currentEnemySpawnIndex < this.enemySpawnTimes.Count)
        return;
      this.AdvanceHourAndSpawnNewBatchOfEnemies();
    }
    else
    {
      if ((double) this.timeScript.currentDayTime <= 85.0 || this.begunSpawningEnemies)
        return;
      this.begunSpawningEnemies = true;
      this.BeginEnemySpawning();
    }
  }

  private void SpawnInsideEnemiesFromVentsIfReady()
  {
    if (this.enemySpawnTimes.Count <= this.currentEnemySpawnIndex || (double) this.timeScript.currentDayTime <= (double) this.enemySpawnTimes[this.currentEnemySpawnIndex])
      return;
    for (int index = 0; index < this.allEnemyVents.Length; ++index)
    {
      if (this.allEnemyVents[index].occupied && (double) this.timeScript.currentDayTime > (double) this.allEnemyVents[index].spawnTime)
      {
        Debug.Log((object) ("Found enemy vent which has its time up: " + this.allEnemyVents[index].gameObject.name + ". Spawning " + this.allEnemyVents[index].enemyType.enemyName + " from vent."));
        this.SpawnEnemyFromVent(this.allEnemyVents[index]);
      }
    }
    ++this.currentEnemySpawnIndex;
  }

  private void AdvanceHourAndSpawnNewBatchOfEnemies()
  {
    this.currentHour += this.hourTimeBetweenEnemySpawnBatches;
    this.SpawnDaytimeEnemiesOutside();
    this.SpawnEnemiesOutside();
    Debug.Log((object) "Advance hour");
    if (this.allEnemyVents.Length != 0 && !this.cannotSpawnMoreInsideEnemies)
    {
      this.currentEnemySpawnIndex = 0;
      if (StartOfRound.Instance.connectedPlayersAmount + 1 > 0 && TimeOfDay.Instance.daysUntilDeadline <= 2 && ((double) (this.valueOfFoundScrapItems / TimeOfDay.Instance.profitQuota) > 0.800000011920929 && (double) TimeOfDay.Instance.normalizedTimeOfDay > 0.30000001192092896 || (double) this.valueOfFoundScrapItems / (double) this.totalScrapValueInLevel > 0.64999997615814209 || StartOfRound.Instance.daysPlayersSurvivedInARow >= 5) && this.minEnemiesToSpawn == 0)
      {
        Debug.Log((object) "Min enemy spawn chance per hour set to 1!!!");
        this.minEnemiesToSpawn = 1;
      }
      this.PlotOutEnemiesForNextHour();
    }
    else
      Debug.Log((object) string.Format("Could not spawn more enemies; vents #: {0}. CannotSpawnMoreInsideEnemies: {1}", (object) this.allEnemyVents.Length, (object) this.cannotSpawnMoreInsideEnemies));
  }

  public void RefreshLightsList()
  {
    this.allPoweredLights.Clear();
    this.allPoweredLightsAnimators.Clear();
    GameObject[] gameObjectsWithTag = GameObject.FindGameObjectsWithTag("PoweredLight");
    if (gameObjectsWithTag == null)
      return;
    for (int index = 0; index < gameObjectsWithTag.Length; ++index)
    {
      Animator componentInChildren = gameObjectsWithTag[index].GetComponentInChildren<Animator>();
      if (!((UnityEngine.Object) componentInChildren == (UnityEngine.Object) null))
      {
        this.allPoweredLightsAnimators.Add(componentInChildren);
        this.allPoweredLights.Add(gameObjectsWithTag[index].GetComponentInChildren<Light>(true));
      }
    }
    for (int index = 0; index < this.allPoweredLightsAnimators.Count; ++index)
      this.allPoweredLightsAnimators[index].SetFloat("flickerSpeed", UnityEngine.Random.Range(0.6f, 1.4f));
    Debug.Log((object) string.Format("# powered lights: {0}", (object) this.allPoweredLights.Count));
  }

  public void RefreshEnemyVents() => this.allEnemyVents = UnityEngine.Object.FindObjectsOfType<EnemyVent>();

  private void SpawnOutsideHazards()
  {
    System.Random randomSeed = new System.Random(StartOfRound.Instance.randomMapSeed + 2);
    this.outsideAINodes = ((IEnumerable<GameObject>) GameObject.FindGameObjectsWithTag("OutsideAINode")).OrderBy<GameObject, float>((Func<GameObject, float>) (x => Vector3.Distance(x.transform.position, Vector3.zero))).ToArray<GameObject>();
    NavMeshHit hit = new NavMeshHit();
    if (TimeOfDay.Instance.currentLevelWeather == LevelWeatherType.Rainy)
    {
      int num = randomSeed.Next(5, 15);
      if (randomSeed.Next(0, 100) < 7)
        num = randomSeed.Next(5, 30);
      for (int index = 0; index < num; ++index)
        UnityEngine.Object.Instantiate<GameObject>(this.quicksandPrefab, this.GetRandomNavMeshPositionInBoxPredictable(this.outsideAINodes[randomSeed.Next(0, this.outsideAINodes.Length)].transform.position, 30f, hit, randomSeed) + Vector3.up, Quaternion.identity, this.mapPropsContainer.transform);
    }
    int num1 = 0;
    List<Vector3> vector3List = new List<Vector3>();
    this.spawnDenialPoints = GameObject.FindGameObjectsWithTag("SpawnDenialPoint");
    if (this.currentLevel.spawnableOutsideObjects != null)
    {
      for (int index1 = 0; index1 < this.currentLevel.spawnableOutsideObjects.Length; ++index1)
      {
        int num2 = (int) this.currentLevel.spawnableOutsideObjects[index1].randomAmount.Evaluate((float) randomSeed.NextDouble());
        if (randomSeed.Next(0, 100) < 20)
          num2 *= 2;
        for (int index2 = 0; index2 < num2; ++index2)
        {
          Vector3 vector3 = this.GetRandomNavMeshPositionInBoxPredictable(this.outsideAINodes[randomSeed.Next(0, this.outsideAINodes.Length)].transform.position, 30f, hit, randomSeed);
          if (this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.spawnableFloorTags != null)
          {
            bool flag = false;
            RaycastHit hitInfo;
            if (Physics.Raycast(vector3 + Vector3.up, Vector3.down, out hitInfo, 5f, StartOfRound.Instance.collidersAndRoomMaskAndDefault))
            {
              for (int index3 = 0; index3 < this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.spawnableFloorTags.Length; ++index3)
              {
                if (hitInfo.collider.transform.CompareTag(this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.spawnableFloorTags[index3]))
                {
                  flag = true;
                  break;
                }
              }
            }
            if (!flag)
              continue;
          }
          if (NavMesh.FindClosestEdge(vector3, out hit, -1) && (double) hit.distance < (double) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth)
          {
            Vector3 position = hit.position;
            if (NavMesh.SamplePosition(new Ray(position, vector3 - position).GetPoint((float) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth + 0.5f), out hit, 10f, -1))
              vector3 = hit.position;
            else
              continue;
          }
          bool flag1 = false;
          for (int index4 = 0; index4 < this.shipSpawnPathPoints.Length; ++index4)
          {
            if ((double) Vector3.Distance(this.shipSpawnPathPoints[index4].transform.position, vector3) < (double) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth + 6.0)
            {
              flag1 = true;
              break;
            }
          }
          for (int index5 = 0; index5 < this.spawnDenialPoints.Length; ++index5)
          {
            if ((double) Vector3.Distance(this.spawnDenialPoints[index5].transform.position, vector3) < (double) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth + 6.0)
            {
              flag1 = true;
              break;
            }
          }
          if ((double) Vector3.Distance(GameObject.FindGameObjectWithTag("ItemShipLandingNode").transform.position, vector3) < (double) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth + 4.0)
            break;
          if (!flag1)
          {
            if (this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth > 4)
            {
              bool flag2 = false;
              for (int index6 = 0; index6 < vector3List.Count; ++index6)
              {
                if ((double) Vector3.Distance(vector3, vector3List[index6]) < (double) this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.objectWidth)
                {
                  flag2 = true;
                  break;
                }
              }
              if (flag2)
                continue;
            }
            vector3List.Add(vector3);
            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.prefabToSpawn, vector3 - Vector3.up * 0.7f, Quaternion.identity, this.mapPropsContainer.transform);
            ++num1;
            gameObject.transform.eulerAngles = !this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.spawnFacingAwayFromWall ? new Vector3(gameObject.transform.eulerAngles.x, (float) randomSeed.Next(0, 360), gameObject.transform.eulerAngles.z) : new Vector3(0.0f, this.YRotationThatFacesTheFarthestFromPosition(vector3 + Vector3.up * 0.2f), 0.0f);
            gameObject.transform.localEulerAngles = new Vector3(gameObject.transform.localEulerAngles.x + this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.rotationOffset.x, gameObject.transform.localEulerAngles.y + this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.rotationOffset.y, gameObject.transform.localEulerAngles.z + this.currentLevel.spawnableOutsideObjects[index1].spawnableObject.rotationOffset.z);
          }
        }
      }
    }
    if (num1 <= 0)
      return;
    GameObject gameObjectWithTag = GameObject.FindGameObjectWithTag("OutsideLevelNavMesh");
    if (!((UnityEngine.Object) gameObjectWithTag != (UnityEngine.Object) null))
      return;
    gameObjectWithTag.GetComponent<NavMeshSurface>().BuildNavMesh();
  }

  private void SpawnRandomStoryLogs()
  {
  }

  public void SetLevelObjectVariables()
  {
    this.StartCoroutine(this.waitForMainEntranceTeleportToSpawn());
  }

  private IEnumerator waitForMainEntranceTeleportToSpawn()
  {
    float startTime = Time.timeSinceLevelLoad;
    while (RoundManager.FindMainEntrancePosition() == Vector3.zero && (double) Time.timeSinceLevelLoad - (double) startTime < 15.0)
      yield return (object) new WaitForSeconds(1f);
    Vector3 entrancePosition = RoundManager.FindMainEntrancePosition();
    this.SetLockedDoors(entrancePosition);
    this.SetSteamValveTimes(entrancePosition);
    this.SetBigDoorCodes(entrancePosition);
    this.SetExitIDs(entrancePosition);
    this.SetPowerOffAtStart();
  }

  private void SetPowerOffAtStart()
  {
    if (new System.Random(StartOfRound.Instance.randomMapSeed + 3).NextDouble() < 0.079999998211860657)
    {
      this.TurnBreakerSwitchesOff();
      Debug.Log((object) "Turning lights off at start");
    }
    else
    {
      this.TurnOnAllLights(true);
      Debug.Log((object) "Turning lights on at start");
    }
  }

  private void SetBigDoorCodes(Vector3 mainEntrancePosition)
  {
    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 17);
    TerminalAccessibleObject[] array = ((IEnumerable<TerminalAccessibleObject>) UnityEngine.Object.FindObjectsOfType<TerminalAccessibleObject>()).OrderBy<TerminalAccessibleObject, float>((Func<TerminalAccessibleObject, float>) (x => (x.transform.position - mainEntrancePosition).sqrMagnitude)).ToArray<TerminalAccessibleObject>();
    int num1 = 3;
    int num2 = 0;
    for (int index = 0; index < array.Length; ++index)
    {
      array[index].InitializeValues();
      array[index].SetCodeTo(random.Next(this.possibleCodesForBigDoors.Length));
      if (array[index].isBigDoor && (num2 < num1 || random.NextDouble() < 0.2199999988079071))
      {
        ++num2;
        array[index].SetDoorOpen(true);
      }
    }
  }

  private void SetExitIDs(Vector3 mainEntrancePosition)
  {
    int num = 1;
    EntranceTeleport[] array = ((IEnumerable<EntranceTeleport>) UnityEngine.Object.FindObjectsOfType<EntranceTeleport>()).OrderBy<EntranceTeleport, float>((Func<EntranceTeleport, float>) (x => (x.transform.position - mainEntrancePosition).sqrMagnitude)).ToArray<EntranceTeleport>();
    for (int index = 0; index < array.Length; ++index)
    {
      if (array[index].entranceId == 1 && !array[index].isEntranceToBuilding)
      {
        array[index].entranceId = num;
        ++num;
      }
    }
  }

  private void SetSteamValveTimes(Vector3 mainEntrancePosition)
  {
    System.Random random = new System.Random(StartOfRound.Instance.randomMapSeed + 513);
    SteamValveHazard[] array = ((IEnumerable<SteamValveHazard>) UnityEngine.Object.FindObjectsOfType<SteamValveHazard>()).OrderBy<SteamValveHazard, float>((Func<SteamValveHazard, float>) (x => (x.transform.position - mainEntrancePosition).sqrMagnitude)).ToArray<SteamValveHazard>();
    for (int index = 0; index < array.Length; ++index)
    {
      if (random.NextDouble() < 0.75)
      {
        array[index].valveBurstTime = Mathf.Clamp((float) random.NextDouble(), 0.2f, 1f);
        array[index].valveCrackTime = array[index].valveBurstTime * (float) random.NextDouble();
        array[index].fogSizeMultiplier = Mathf.Clamp((float) random.NextDouble(), 0.6f, 0.98f);
      }
      else if (random.NextDouble() < 0.25)
        array[index].valveCrackTime = Mathf.Clamp((float) random.NextDouble(), 0.3f, 0.9f);
    }
  }

  private void SetLockedDoors(Vector3 mainEntrancePosition)
  {
    if (mainEntrancePosition == Vector3.zero)
      Debug.Log((object) "Main entrance teleport was not spawned on local client within 12 seconds. Locking doors based on origin instead.");
    List<DoorLock> list1 = ((IEnumerable<DoorLock>) UnityEngine.Object.FindObjectsOfType<DoorLock>()).ToList<DoorLock>();
    for (int index = list1.Count - 1; index >= 0; --index)
    {
      if ((double) list1[index].transform.position.y > -160.0)
        list1.RemoveAt(index);
    }
    List<DoorLock> list2 = list1.OrderByDescending<DoorLock, float>((Func<DoorLock, float>) (x => (mainEntrancePosition - x.transform.position).sqrMagnitude)).ToList<DoorLock>();
    float num1 = 1.1f;
    int num2 = 0;
    for (int index = 0; index < list2.Count; ++index)
    {
      if (this.LevelRandom.NextDouble() < (double) num1)
      {
        float timeToLockPick = Mathf.Clamp((float) this.LevelRandom.Next(2, 90), 2f, 32f);
        list2[index].LockDoor(timeToLockPick);
        ++num2;
      }
      num1 /= 1.55f;
    }
    if (!this.IsServer)
      return;
    for (int index = 0; index < num2; ++index)
    {
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(this.keyPrefab, this.GetRandomNavMeshPositionInBoxPredictable(this.insideAINodes[this.AnomalyRandom.Next(0, this.insideAINodes.Length)].transform.position, 8f, this.navHit, this.AnomalyRandom), Quaternion.identity, this.spawnedScrapContainer);
      gameObject.GetComponent<NetworkObject>().Spawn();
      Debug.Log((object) string.Format("Spawning key: {0}; isSpawned? : {1}", (object) gameObject.name, (object) gameObject.GetComponent<NetworkObject>().IsSpawned));
    }
  }

  [ServerRpc]
  public void LightningStrikeServerRpc(Vector3 strikePosition)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(1145714957U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in strikePosition);
      this.__endSendServerRpc(ref bufferWriter, 1145714957U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.LightningStrikeClientRpc(strikePosition);
  }

  [ClientRpc]
  public void LightningStrikeClientRpc(Vector3 strikePosition)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(112447504U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe(in strikePosition);
      this.__endSendClientRpc(ref bufferWriter, 112447504U, clientRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost)
      return;
    UnityEngine.Object.FindObjectOfType<StormyWeather>(true).LightningStrike(strikePosition, true);
  }

  [ServerRpc]
  public void ShowStaticElectricityWarningServerRpc(
    NetworkObjectReference warningObject,
    float timeLeft)
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
      FastBufferWriter bufferWriter = this.__beginSendServerRpc(445397880U, serverRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in warningObject, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<float>(in timeLeft, new FastBufferWriter.ForPrimitives());
      this.__endSendServerRpc(ref bufferWriter, 445397880U, serverRpcParams, RpcDelivery.Reliable);
    }
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Server || !networkManager.IsServer && !networkManager.IsHost)
      return;
    this.ShowStaticElectricityWarningClientRpc(warningObject, timeLeft);
  }

  [ClientRpc]
  public void ShowStaticElectricityWarningClientRpc(
    NetworkObjectReference warningObject,
    float timeLeft)
  {
    NetworkManager networkManager = this.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client && (networkManager.IsServer || networkManager.IsHost))
    {
      ClientRpcParams clientRpcParams;
      FastBufferWriter bufferWriter = this.__beginSendClientRpc(3840203489U, clientRpcParams, RpcDelivery.Reliable);
      bufferWriter.WriteValueSafe<NetworkObjectReference>(in warningObject, new FastBufferWriter.ForNetworkSerializable());
      bufferWriter.WriteValueSafe<float>(in timeLeft, new FastBufferWriter.ForPrimitives());
      this.__endSendClientRpc(ref bufferWriter, 3840203489U, clientRpcParams, RpcDelivery.Reliable);
    }
    NetworkObject networkObject;
    if (this.__rpc_exec_stage != NetworkBehaviour.__RpcExecStage.Client || !networkManager.IsClient && !networkManager.IsHost || !warningObject.TryGet(out networkObject))
      return;
    UnityEngine.Object.FindObjectOfType<StormyWeather>(true).SetStaticElectricityWarning(networkObject, timeLeft);
  }

  public Vector3 RandomlyOffsetPosition(Vector3 pos, float maxRadius, float padding = 1f)
  {
    this.tempTransform.position = pos;
    this.tempTransform.eulerAngles = Vector3.forward;
    for (int index = 0; index < 5; ++index)
    {
      this.tempTransform.localEulerAngles = new Vector3(0.0f, this.tempTransform.localEulerAngles.y + UnityEngine.Random.Range(-180f, 180f), 0.0f);
      Ray ray = new Ray(this.tempTransform.position, this.tempTransform.forward);
      RaycastHit hitInfo;
      if (Physics.Raycast(ray, out hitInfo, 6f, 2304))
      {
        float num = hitInfo.distance - padding;
        if ((double) num < 0.0)
          return ray.GetPoint(num);
        float distance = Mathf.Clamp(UnityEngine.Random.Range(0.1f, maxRadius), 0.0f, num);
        return ray.GetPoint(distance);
      }
    }
    return pos;
  }

  public static Vector3 RandomPointInBounds(Bounds bounds)
  {
    return new Vector3(UnityEngine.Random.Range(bounds.min.x, bounds.max.x), UnityEngine.Random.Range(bounds.min.y, bounds.max.y), UnityEngine.Random.Range(bounds.min.z, bounds.max.z));
  }

  public Vector3 GetNavMeshPosition(
    Vector3 pos,
    NavMeshHit navMeshHit = default (NavMeshHit),
    float sampleRadius = 5f,
    int areaMask = -1)
  {
    if (NavMesh.SamplePosition(pos, out navMeshHit, sampleRadius, areaMask))
    {
      this.GotNavMeshPositionResult = true;
      return navMeshHit.position;
    }
    this.GotNavMeshPositionResult = false;
    return pos;
  }

  public Transform GetClosestNode(Vector3 pos, bool outside = true)
  {
    GameObject[] gameObjectArray;
    if (outside)
    {
      if (this.outsideAINodes == null)
        this.outsideAINodes = GameObject.FindGameObjectsWithTag("OutsideAINode");
      gameObjectArray = this.outsideAINodes;
    }
    else
    {
      if (this.insideAINodes == null)
        this.outsideAINodes = GameObject.FindGameObjectsWithTag("AINode");
      gameObjectArray = this.insideAINodes;
    }
    float num = 99999f;
    int index1 = 0;
    for (int index2 = 0; index2 < gameObjectArray.Length; ++index2)
    {
      float sqrMagnitude = (gameObjectArray[index2].transform.position - pos).sqrMagnitude;
      if ((double) sqrMagnitude < (double) num)
      {
        num = sqrMagnitude;
        index1 = index2;
      }
    }
    return gameObjectArray[index1].transform;
  }

  public Vector3 GetRandomNavMeshPositionInRadius(Vector3 pos, float radius = 10f, NavMeshHit navHit = default (NavMeshHit))
  {
    float y = pos.y;
    pos = UnityEngine.Random.insideUnitSphere * radius + pos;
    pos.y = y;
    if (NavMesh.SamplePosition(pos, out navHit, radius, -1))
      return navHit.position;
    Debug.Log((object) "Unable to get random nav mesh position in radius! Returning old pos");
    return pos;
  }

  public Vector3 GetRandomNavMeshPositionInBoxPredictable(
    Vector3 pos,
    float radius = 10f,
    NavMeshHit navHit = default (NavMeshHit),
    System.Random randomSeed = null,
    int layerMask = -1)
  {
    float y1 = pos.y;
    double x = (double) this.RandomNumberInRadius(radius, randomSeed);
    float num1 = this.RandomNumberInRadius(radius, randomSeed);
    float num2 = this.RandomNumberInRadius(radius, randomSeed);
    double y2 = (double) num1;
    double z = (double) num2;
    pos = new Vector3((float) x, (float) y2, (float) z) + pos;
    pos.y = y1;
    return NavMesh.SamplePosition(pos, out navHit, radius, layerMask) ? navHit.position : pos;
  }

  public Vector3 GetRandomPositionInBoxPredictable(Vector3 pos, float radius = 10f, System.Random randomSeed = null)
  {
    double x = (double) this.RandomNumberInRadius(radius, randomSeed);
    float num1 = this.RandomNumberInRadius(radius, randomSeed);
    float num2 = this.RandomNumberInRadius(radius, randomSeed);
    double y = (double) num1;
    double z = (double) num2;
    return new Vector3((float) x, (float) y, (float) z) + pos;
  }

  private float RandomNumberInRadius(float radius, System.Random randomSeed)
  {
    return ((float) randomSeed.NextDouble() - 0.5f) * radius;
  }

  public Vector3 GetRandomNavMeshPositionInRadiusSpherical(
    Vector3 pos,
    float radius = 10f,
    NavMeshHit navHit = default (NavMeshHit))
  {
    pos = UnityEngine.Random.insideUnitSphere * radius + pos;
    if (NavMesh.SamplePosition(pos, out navHit, radius + 2f, 1))
    {
      Debug.Log((object) "Got nav mesh position success!");
      Debug.DrawRay(pos + Vector3.forward * 0.01f, Vector3.up * 2f, Color.blue);
      return navHit.position;
    }
    Debug.Log((object) "Get navmesh position failed!");
    Debug.DrawRay(pos + Vector3.forward * 0.01f, Vector3.up * 2f, Color.yellow);
    return pos;
  }

  public Vector3 GetRandomPositionInRadius(
    Vector3 pos,
    float minRadius,
    float radius,
    System.Random randomGen = null)
  {
    radius *= 2f;
    return new Vector3(pos.x + this.RandomFloatWithinRadius(minRadius, radius, randomGen), pos.y + this.RandomFloatWithinRadius(minRadius, radius, randomGen), pos.z + this.RandomFloatWithinRadius(minRadius, radius, randomGen));
  }

  private float RandomFloatWithinRadius(float minValue, float radius, System.Random randomGenerator)
  {
    return randomGenerator == null ? UnityEngine.Random.Range(minValue, radius) * ((double) UnityEngine.Random.value > 0.5 ? 1f : -1f) : (float) randomGenerator.Next((int) minValue, (int) radius) * (randomGenerator.NextDouble() > 0.5 ? 1f : -1f);
  }

  public static Vector3 AverageOfLivingGroupedPlayerPositions()
  {
    Vector3 zero = Vector3.zero;
    for (int index = 0; index < StartOfRound.Instance.connectedPlayersAmount + 1; ++index)
    {
      if (StartOfRound.Instance.allPlayerScripts[index].isPlayerControlled && !StartOfRound.Instance.allPlayerScripts[index].isPlayerAlone)
        zero += StartOfRound.Instance.allPlayerScripts[index].transform.position;
    }
    return zero / (float) (StartOfRound.Instance.connectedPlayersAmount + 1);
  }

  public void PlayAudibleNoise(
    Vector3 noisePosition,
    float noiseRange = 10f,
    float noiseLoudness = 0.5f,
    int timesPlayedInSameSpot = 0,
    bool noiseIsInsideClosedShip = false,
    int noiseID = 0)
  {
    if (noiseIsInsideClosedShip)
      noiseRange /= 2f;
    int num = Physics.OverlapSphereNonAlloc(noisePosition, noiseRange, this.tempColliderResults, 8912896);
    for (int index = 0; index < num; ++index)
    {
      INoiseListener component1;
      if (this.tempColliderResults[index].transform.TryGetComponent<INoiseListener>(out component1))
      {
        if (noiseIsInsideClosedShip)
        {
          EnemyAI component2 = this.tempColliderResults[index].gameObject.GetComponent<EnemyAI>();
          if (((UnityEngine.Object) component2 == (UnityEngine.Object) null || !component2.isInsidePlayerShip) && (double) noiseLoudness < 0.89999997615814209)
            continue;
        }
        component1.DetectNoise(noisePosition, noiseLoudness, timesPlayedInSameSpot, noiseID);
      }
    }
  }

  public static int PlayRandomClip(
    AudioSource audioSource,
    AudioClip[] clipsArray,
    bool randomize = true,
    float oneShotVolume = 1f,
    int audibleNoiseID = 0)
  {
    if (randomize)
      audioSource.pitch = UnityEngine.Random.Range(0.94f, 1.06f);
    int index = UnityEngine.Random.Range(0, clipsArray.Length);
    audioSource.PlayOneShot(clipsArray[index], UnityEngine.Random.Range(oneShotVolume - 0.18f, oneShotVolume));
    WalkieTalkie.TransmitOneShotAudio(audioSource, clipsArray[index], 0.85f);
    if ((double) audioSource.spatialBlend > 0.0 && audibleNoiseID >= 0)
      RoundManager.Instance.PlayAudibleNoise(audioSource.transform.position, 4f * oneShotVolume, oneShotVolume / 2f, noiseIsInsideClosedShip: true, noiseID: audibleNoiseID);
    return index;
  }

  public static EntranceTeleport FindMainEntranceScript(bool getOutsideEntrance = false)
  {
    EntranceTeleport[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>(false);
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].entranceId == 0)
      {
        if (!getOutsideEntrance)
        {
          if (!objectsOfType[index].isEntranceToBuilding)
            return objectsOfType[index];
        }
        else if (objectsOfType[index].isEntranceToBuilding)
          return objectsOfType[index];
      }
    }
    if (objectsOfType.Length == 0)
    {
      Debug.LogError((object) "Main entrance was not spawned and could not be found; returning null");
      return (EntranceTeleport) null;
    }
    Debug.LogError((object) "Main entrance script could not be found. Returning first entrance teleport script found.");
    return objectsOfType[0];
  }

  public static Vector3 FindMainEntrancePosition(bool getTeleportPosition = false, bool getOutsideEntrance = false)
  {
    EntranceTeleport[] objectsOfType = UnityEngine.Object.FindObjectsOfType<EntranceTeleport>(false);
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (objectsOfType[index].entranceId == 0)
      {
        if (!getOutsideEntrance)
        {
          if (!objectsOfType[index].isEntranceToBuilding)
            return getTeleportPosition ? objectsOfType[index].entrancePoint.position : objectsOfType[index].transform.position;
        }
        else if (objectsOfType[index].isEntranceToBuilding)
          return getTeleportPosition ? objectsOfType[index].entrancePoint.position : objectsOfType[index].transform.position;
      }
    }
    Debug.LogError((object) "Main entrance position could not be found. Returning origin.");
    return Vector3.zero;
  }

  public int GetRandomWeightedIndex(int[] weights, System.Random randomSeed = null)
  {
    if (randomSeed == null)
      randomSeed = this.AnomalyRandom;
    if (weights == null || weights.Length == 0)
    {
      Debug.Log((object) "Could not get random weighted index; array is empty or null.");
      return -1;
    }
    int num1 = 0;
    for (int index = 0; index < weights.Length; ++index)
    {
      if (weights[index] >= 0)
        num1 += weights[index];
    }
    if (num1 <= 0)
      return randomSeed.Next(0, weights.Length);
    float num2 = (float) randomSeed.NextDouble();
    float num3 = 0.0f;
    for (int randomWeightedIndex = 0; randomWeightedIndex < weights.Length; ++randomWeightedIndex)
    {
      if ((double) weights[randomWeightedIndex] > 0.0)
      {
        num3 += (float) weights[randomWeightedIndex] / (float) num1;
        if ((double) num3 >= (double) num2)
          return randomWeightedIndex;
      }
    }
    Debug.LogError((object) "Error while calculating random weighted index. Choosing randomly. Weights given:");
    for (int index = 0; index < weights.Length; ++index)
      Debug.LogError((object) string.Format("{0},", (object) weights[index]));
    if (!this.hasInitializedLevelRandomSeed)
      this.InitializeRandomNumberGenerators();
    return randomSeed.Next(0, weights.Length);
  }

  public int GetRandomWeightedIndexList(List<int> weights, System.Random randomSeed = null)
  {
    if (weights == null || weights.Count == 0)
    {
      Debug.Log((object) "Could not get random weighted index; array is empty or null.");
      return -1;
    }
    int num1 = 0;
    for (int index = 0; index < weights.Count; ++index)
    {
      if (weights[index] >= 0)
        num1 += weights[index];
    }
    float num2 = randomSeed != null ? (float) randomSeed.NextDouble() : UnityEngine.Random.value;
    float num3 = 0.0f;
    for (int index = 0; index < weights.Count; ++index)
    {
      if ((double) weights[index] > 0.0)
      {
        num3 += (float) weights[index] / (float) num1;
        if ((double) num3 >= (double) num2)
          return index;
      }
    }
    Debug.LogError((object) "Error while calculating random weighted index.");
    for (int index = 0; index < weights.Count; ++index)
      Debug.LogError((object) string.Format("{0},", (object) weights[index]));
    if (!this.hasInitializedLevelRandomSeed)
      this.InitializeRandomNumberGenerators();
    return randomSeed.Next(0, weights.Count);
  }

  public int GetWeightedValue(int indexLength)
  {
    return Mathf.Clamp(UnityEngine.Random.Range(0, indexLength * 2) - (indexLength - 1), 0, indexLength);
  }

  private static int SortBySize(int p1, int p2) => p1.CompareTo(p2);

  protected override void __initializeVariables() => base.__initializeVariables();

  [RuntimeInitializeOnLoadMethod]
  internal static void InitializeRPCS_RoundManager()
  {
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1659269112U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_1659269112)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1193916134U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_1193916134)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(192551691U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_192551691)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(710372063U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_710372063)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(2729232387U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_2729232387)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(46494176U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_46494176)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3840785488U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_3840785488)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1061166170U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_1061166170)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1586488299U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_1586488299)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(1145714957U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_1145714957)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(112447504U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_112447504)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(445397880U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_445397880)));
    // ISSUE: explicit non-virtual call
    __nonvirtual (NetworkManager.__rpc_func_table.Add(3840203489U, new NetworkManager.RpcReceiveHandler(RoundManager.__rpc_handler_3840203489)));
  }

  private static void __rpc_handler_1659269112(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    bool flag1;
    reader.ReadValueSafe<bool>(out flag1, new FastBufferWriter.ForPrimitives());
    NetworkObjectReference[] spawnedScrap = (NetworkObjectReference[]) null;
    if (flag1)
      reader.ReadValueSafe<NetworkObjectReference>(out spawnedScrap, new FastBufferWriter.ForNetworkSerializable());
    bool flag2;
    reader.ReadValueSafe<bool>(out flag2, new FastBufferWriter.ForPrimitives());
    int[] allScrapValue = (int[]) null;
    if (flag2)
      reader.ReadValueSafe<int>(out allScrapValue, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).SyncScrapValuesClientRpc(spawnedScrap, allScrapValue);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1193916134(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    int randomSeed;
    ByteUnpacker.ReadValueBitPacked(reader, out randomSeed);
    int levelID;
    ByteUnpacker.ReadValueBitPacked(reader, out levelID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).GenerateNewLevelClientRpc(randomSeed, levelID);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_192551691(
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
    ((RoundManager) target).FinishedGeneratingLevelServerRpc(clientId);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_710372063(
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
      ((RoundManager) target).FinishGeneratingNewLevelServerRpc();
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_2729232387(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).FinishGeneratingNewLevelClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_46494176(
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
      Vector3 spawnPosition;
      reader.ReadValueSafe(out spawnPosition);
      float yRot;
      reader.ReadValueSafe<float>(out yRot, new FastBufferWriter.ForPrimitives());
      int enemyNumber;
      ByteUnpacker.ReadValueBitPacked(reader, out enemyNumber);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((RoundManager) target).SpawnEnemyServerRpc(spawnPosition, yRot, enemyNumber);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3840785488(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference enemyNetworkObject;
    reader.ReadValueSafe<NetworkObjectReference>(out enemyNetworkObject, new FastBufferWriter.ForNetworkSerializable());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
    ((RoundManager) target).DespawnEnemyServerRpc(enemyNetworkObject);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1061166170(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).PowerSwitchOnClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1586488299(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).PowerSwitchOffClientRpc();
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_1145714957(
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
      Vector3 strikePosition;
      reader.ReadValueSafe(out strikePosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((RoundManager) target).LightningStrikeServerRpc(strikePosition);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_112447504(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    Vector3 strikePosition;
    reader.ReadValueSafe(out strikePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).LightningStrikeClientRpc(strikePosition);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  private static void __rpc_handler_445397880(
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
      NetworkObjectReference warningObject;
      reader.ReadValueSafe<NetworkObjectReference>(out warningObject, new FastBufferWriter.ForNetworkSerializable());
      float timeLeft;
      reader.ReadValueSafe<float>(out timeLeft, new FastBufferWriter.ForPrimitives());
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Server;
      ((RoundManager) target).ShowStaticElectricityWarningServerRpc(warningObject, timeLeft);
      target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
    }
  }

  private static void __rpc_handler_3840203489(
    NetworkBehaviour target,
    FastBufferReader reader,
    __RpcParams rpcParams)
  {
    NetworkManager networkManager = target.NetworkManager;
    if (networkManager == null || !networkManager.IsListening)
      return;
    NetworkObjectReference warningObject;
    reader.ReadValueSafe<NetworkObjectReference>(out warningObject, new FastBufferWriter.ForNetworkSerializable());
    float timeLeft;
    reader.ReadValueSafe<float>(out timeLeft, new FastBufferWriter.ForPrimitives());
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.Client;
    ((RoundManager) target).ShowStaticElectricityWarningClientRpc(warningObject, timeLeft);
    target.__rpc_exec_stage = NetworkBehaviour.__RpcExecStage.None;
  }

  protected internal override string __getTypeName() => nameof (RoundManager);
}
