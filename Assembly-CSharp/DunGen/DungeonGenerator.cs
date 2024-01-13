// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonGenerator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
namespace DunGen
{
  [Serializable]
  public class DungeonGenerator : ISerializationCallbackReceiver
  {
    public const int CurrentFileVersion = 1;
    [SerializeField]
    [FormerlySerializedAs("AllowImmediateRepeats")]
    private bool allowImmediateRepeats;
    public int Seed;
    public bool ShouldRandomizeSeed = true;
    public int MaxAttemptCount = 20;
    public bool UseMaximumPairingAttempts;
    public int MaxPairingAttempts = 5;
    public bool IgnoreSpriteBounds;
    public AxisDirection UpDirection = AxisDirection.PosY;
    [FormerlySerializedAs("OverrideAllowImmediateRepeats")]
    public bool OverrideRepeatMode;
    public TileRepeatMode RepeatMode;
    public bool OverrideAllowTileRotation;
    public bool AllowTileRotation;
    public bool DebugRender;
    public float LengthMultiplier = 1f;
    public bool PlaceTileTriggers = true;
    public int TileTriggerLayer = 2;
    public bool GenerateAsynchronously;
    public float MaxAsyncFrameMilliseconds = 50f;
    public float PauseBetweenRooms;
    public bool RestrictDungeonToBounds;
    public Bounds TilePlacementBounds = new Bounds(Vector3.zero, Vector3.one * 10f);
    public float OverlapThreshold = 0.01f;
    public float Padding;
    public bool DisallowOverhangs;
    public GameObject Root;
    public DungeonFlow DungeonFlow;
    protected int retryCount;
    protected DungeonProxy proxyDungeon;
    protected readonly Dictionary<TilePlacementResult, int> tilePlacementResultCounters = new Dictionary<TilePlacementResult, int>();
    protected readonly List<GameObject> useableTiles = new List<GameObject>();
    protected int targetLength;
    protected List<InjectedTile> tilesPendingInjection;
    protected List<DungeonGeneratorPostProcessStep> postProcessSteps = new List<DungeonGeneratorPostProcessStep>();
    [SerializeField]
    private int fileVersion;
    private int nextNodeIndex;
    private DungeonArchetype currentArchetype;
    private GraphLine previousLineSegment;
    private List<TileProxy> preProcessData = new List<TileProxy>();
    private Stopwatch yieldTimer = new Stopwatch();
    private Dictionary<TileProxy, InjectedTile> injectedTiles = new Dictionary<TileProxy, InjectedTile>();

    public RandomStream RandomStream { get; protected set; }

    public Vector3 UpVector
    {
      get
      {
        switch (this.UpDirection)
        {
          case AxisDirection.PosX:
            return new Vector3(1f, 0.0f, 0.0f);
          case AxisDirection.NegX:
            return new Vector3(-1f, 0.0f, 0.0f);
          case AxisDirection.PosY:
            return new Vector3(0.0f, 1f, 0.0f);
          case AxisDirection.NegY:
            return new Vector3(0.0f, -1f, 0.0f);
          case AxisDirection.PosZ:
            return new Vector3(0.0f, 0.0f, 1f);
          case AxisDirection.NegZ:
            return new Vector3(0.0f, 0.0f, -1f);
          default:
            throw new NotImplementedException("AxisDirection '" + this.UpDirection.ToString() + "' not implemented");
        }
      }
    }

    public event GenerationStatusDelegate OnGenerationStatusChanged;

    public static event GenerationStatusDelegate OnAnyDungeonGenerationStatusChanged;

    public event TileInjectionDelegate TileInjectionMethods;

    public event Action Cleared;

    public event Action Retrying;

    public GenerationStatus Status { get; private set; }

    public GenerationStats GenerationStats { get; private set; }

    public int ChosenSeed { get; protected set; }

    public Dungeon CurrentDungeon { get; private set; }

    public bool IsGenerating { get; private set; }

    public bool IsAnalysis { get; set; }

    public DungeonGenerator() => this.GenerationStats = new GenerationStats();

    public DungeonGenerator(GameObject root)
      : this()
    {
      this.Root = root;
    }

    public void Generate()
    {
      if (this.IsGenerating)
        return;
      this.IsAnalysis = false;
      this.IsGenerating = true;
      this.Wait(this.OuterGenerate());
    }

    public void Cancel()
    {
      if (!this.IsGenerating)
        return;
      this.Clear(true);
      this.IsGenerating = false;
    }

    public Dungeon DetachDungeon()
    {
      if ((UnityEngine.Object) this.CurrentDungeon == (UnityEngine.Object) null)
        return (Dungeon) null;
      Dungeon currentDungeon = this.CurrentDungeon;
      this.CurrentDungeon = (Dungeon) null;
      this.Root = (GameObject) null;
      this.Clear(true);
      return currentDungeon;
    }

    protected virtual IEnumerator OuterGenerate()
    {
      this.Clear(false);
      this.yieldTimer.Restart();
      this.Status = GenerationStatus.NotStarted;
      this.ChosenSeed = this.ShouldRandomizeSeed ? new RandomStream().Next() : this.Seed;
      this.RandomStream = new RandomStream(this.ChosenSeed);
      if ((UnityEngine.Object) this.Root == (UnityEngine.Object) null)
        this.Root = new GameObject("Dungeon");
      yield return (object) this.Wait(this.InnerGenerate(false));
      this.IsGenerating = false;
    }

    private Coroutine Wait(IEnumerator routine)
    {
      if (this.GenerateAsynchronously)
        return CoroutineHelper.Start(routine);
      do
        ;
      while (routine.MoveNext());
      return (Coroutine) null;
    }

    public void RandomizeSeed() => this.Seed = new RandomStream().Next();

    protected virtual IEnumerator InnerGenerate(bool isRetry)
    {
      DungeonGenerator dungeonGenerator = this;
      if (isRetry)
      {
        dungeonGenerator.ChosenSeed = dungeonGenerator.RandomStream.Next();
        dungeonGenerator.RandomStream = new RandomStream(dungeonGenerator.ChosenSeed);
        if (dungeonGenerator.retryCount >= dungeonGenerator.MaxAttemptCount && Application.isEditor)
        {
          string message = "Failed to generate the dungeon " + dungeonGenerator.MaxAttemptCount.ToString() + " times.\nThis could indicate a problem with the way the tiles are set up. Try to make sure most rooms have more than one doorway and that all doorways are easily accessible.\nHere are a list of all reasons a tile placement had to be retried:";
          foreach (KeyValuePair<TilePlacementResult, int> placementResultCounter in dungeonGenerator.tilePlacementResultCounters)
          {
            if (placementResultCounter.Value > 0)
              message = message + "\n" + placementResultCounter.Key.ToString() + " (x" + placementResultCounter.Value.ToString() + ")";
          }
          UnityEngine.Debug.LogError((object) message);
          dungeonGenerator.ChangeStatus(GenerationStatus.Failed);
          yield break;
        }
        else
        {
          ++dungeonGenerator.retryCount;
          dungeonGenerator.GenerationStats.IncrementRetryCount();
          if (dungeonGenerator.Retrying != null)
            dungeonGenerator.Retrying();
        }
      }
      else
      {
        dungeonGenerator.retryCount = 0;
        dungeonGenerator.GenerationStats.Clear();
      }
      dungeonGenerator.CurrentDungeon = dungeonGenerator.Root.GetComponent<Dungeon>();
      if ((UnityEngine.Object) dungeonGenerator.CurrentDungeon == (UnityEngine.Object) null)
        dungeonGenerator.CurrentDungeon = dungeonGenerator.Root.AddComponent<Dungeon>();
      dungeonGenerator.CurrentDungeon.DebugRender = dungeonGenerator.DebugRender;
      dungeonGenerator.CurrentDungeon.PreGenerateDungeon(dungeonGenerator);
      dungeonGenerator.Clear(false);
      dungeonGenerator.targetLength = Mathf.RoundToInt((float) dungeonGenerator.DungeonFlow.Length.GetRandom(dungeonGenerator.RandomStream) * dungeonGenerator.LengthMultiplier);
      dungeonGenerator.targetLength = Mathf.Max(dungeonGenerator.targetLength, 2);
      Transform transform = (double) dungeonGenerator.PauseBetweenRooms > 0.0 ? dungeonGenerator.Root.transform : (Transform) null;
      dungeonGenerator.proxyDungeon = new DungeonProxy(transform);
      dungeonGenerator.GenerationStats.BeginTime(GenerationStatus.TileInjection);
      if (dungeonGenerator.tilesPendingInjection == null)
        dungeonGenerator.tilesPendingInjection = new List<InjectedTile>();
      else
        dungeonGenerator.tilesPendingInjection.Clear();
      dungeonGenerator.injectedTiles.Clear();
      dungeonGenerator.GatherTilesToInject();
      dungeonGenerator.GenerationStats.BeginTime(GenerationStatus.PreProcessing);
      dungeonGenerator.PreProcess();
      dungeonGenerator.GenerationStats.BeginTime(GenerationStatus.MainPath);
      yield return (object) dungeonGenerator.Wait(dungeonGenerator.GenerateMainPath());
      if (dungeonGenerator.Status != GenerationStatus.Complete && dungeonGenerator.Status != GenerationStatus.Failed)
      {
        dungeonGenerator.GenerationStats.BeginTime(GenerationStatus.Branching);
        yield return (object) dungeonGenerator.Wait(dungeonGenerator.GenerateBranchPaths());
        foreach (InjectedTile injectedTile in dungeonGenerator.tilesPendingInjection)
        {
          if (injectedTile.IsRequired)
          {
            yield return (object) dungeonGenerator.Wait(dungeonGenerator.InnerGenerate(true));
            yield break;
          }
        }
        if (dungeonGenerator.Status != GenerationStatus.Complete && dungeonGenerator.Status != GenerationStatus.Failed)
        {
          if (dungeonGenerator.DungeonFlow.BranchPruneTags.Count > 0)
            dungeonGenerator.PruneBranches();
          dungeonGenerator.proxyDungeon.ConnectOverlappingDoorways(dungeonGenerator.DungeonFlow.DoorwayConnectionChance, dungeonGenerator.DungeonFlow, dungeonGenerator.RandomStream);
          dungeonGenerator.CurrentDungeon.FromProxy(dungeonGenerator.proxyDungeon, dungeonGenerator);
          yield return (object) dungeonGenerator.Wait(dungeonGenerator.PostProcess());
          yield return (object) null;
          foreach (IDungeonCompleteReceiver componentsInChild in dungeonGenerator.CurrentDungeon.gameObject.GetComponentsInChildren<IDungeonCompleteReceiver>(false))
            componentsInChild.OnDungeonComplete(dungeonGenerator.CurrentDungeon);
          dungeonGenerator.ChangeStatus(GenerationStatus.Complete);
          if (true)
          {
            foreach (DungenCharacter dungenCharacter in UnityEngine.Object.FindObjectsOfType<DungenCharacter>())
              dungenCharacter.ForceRecheckTile();
          }
        }
      }
    }

    private void PruneBranches()
    {
      Stack<TileProxy> tileProxyStack = new Stack<TileProxy>();
      foreach (TileProxy branchPathTile in this.proxyDungeon.BranchPathTiles)
      {
        TileProxy tile = branchPathTile;
        if (!tile.UsedDoorways.Select<DoorwayProxy, TileProxy>((Func<DoorwayProxy, TileProxy>) (d => d.ConnectedDoorway.TileProxy)).Any<TileProxy>((Func<TileProxy, bool>) (t => t.Placement.BranchDepth > tile.Placement.BranchDepth)))
          tileProxyStack.Push(tile);
      }
      while (tileProxyStack.Count > 0)
      {
        TileProxy tile = tileProxyStack.Pop();
        if (((tile.Placement.InjectionData == null ? 0 : (tile.Placement.InjectionData.IsRequired ? 1 : 0)) != 0 ? 0 : (this.DungeonFlow.ShouldPruneTileWithTags(tile.PrefabTile.Tags) ? 1 : 0)) != 0)
        {
          ProxyDoorwayConnection connection = tile.UsedDoorways.Select<DoorwayProxy, DoorwayProxy>((Func<DoorwayProxy, DoorwayProxy>) (d => d.ConnectedDoorway)).Where<DoorwayProxy>((Func<DoorwayProxy, bool>) (d => d.TileProxy.Placement.IsOnMainPath || d.TileProxy.Placement.BranchDepth < tile.Placement.BranchDepth)).Select<DoorwayProxy, ProxyDoorwayConnection>((Func<DoorwayProxy, ProxyDoorwayConnection>) (d => new ProxyDoorwayConnection(d, d.ConnectedDoorway))).First<ProxyDoorwayConnection>();
          this.proxyDungeon.RemoveTile(tile);
          this.proxyDungeon.RemoveConnection(connection);
          ++this.GenerationStats.PrunedBranchTileCount;
          TileProxy tileProxy = connection.A.TileProxy;
          if (!tileProxy.Placement.IsOnMainPath)
            tileProxyStack.Push(tileProxy);
        }
      }
    }

    public virtual void Clear(bool stopCoroutines)
    {
      if (stopCoroutines)
        CoroutineHelper.StopAll();
      if (this.proxyDungeon != null)
        this.proxyDungeon.ClearDebugVisuals();
      this.proxyDungeon = (DungeonProxy) null;
      if ((UnityEngine.Object) this.CurrentDungeon != (UnityEngine.Object) null)
        this.CurrentDungeon.Clear();
      this.useableTiles.Clear();
      this.preProcessData.Clear();
      this.previousLineSegment = (GraphLine) null;
      this.tilePlacementResultCounters.Clear();
      if (this.Cleared == null)
        return;
      this.Cleared();
    }

    private void ChangeStatus(GenerationStatus status)
    {
      int status1 = (int) this.Status;
      this.Status = status;
      if (status == GenerationStatus.Complete || status == GenerationStatus.Failed)
        this.IsGenerating = false;
      if (status == GenerationStatus.Failed)
        this.Clear(true);
      int num = (int) status;
      if (status1 == num)
        return;
      GenerationStatusDelegate generationStatusChanged1 = this.OnGenerationStatusChanged;
      if (generationStatusChanged1 != null)
        generationStatusChanged1(this, status);
      GenerationStatusDelegate generationStatusChanged2 = DungeonGenerator.OnAnyDungeonGenerationStatusChanged;
      if (generationStatusChanged2 == null)
        return;
      generationStatusChanged2(this, status);
    }

    protected virtual void PreProcess()
    {
      if (this.preProcessData.Count > 0)
        return;
      this.ChangeStatus(GenerationStatus.PreProcessing);
      foreach (TileSet tileSet in ((IEnumerable<TileSet>) this.DungeonFlow.GetUsedTileSets()).Concat<TileSet>(this.tilesPendingInjection.Select<InjectedTile, TileSet>((Func<InjectedTile, TileSet>) (x => x.TileSet))).Distinct<TileSet>())
      {
        foreach (GameObjectChance weight in tileSet.TileWeights.Weights)
        {
          if ((UnityEngine.Object) weight.Value != (UnityEngine.Object) null)
          {
            this.useableTiles.Add(weight.Value);
            weight.TileSet = tileSet;
          }
        }
      }
    }

    protected virtual void GatherTilesToInject()
    {
      RandomStream randomStream = new RandomStream(this.ChosenSeed);
      foreach (TileInjectionRule tileInjectionRule in this.DungeonFlow.TileInjectionRules)
      {
        if (!((UnityEngine.Object) tileInjectionRule.TileSet == (UnityEngine.Object) null) && (tileInjectionRule.CanAppearOnMainPath || tileInjectionRule.CanAppearOnBranchPath))
        {
          bool isOnMainPath = !tileInjectionRule.CanAppearOnBranchPath || tileInjectionRule.CanAppearOnMainPath && randomStream.NextDouble() > 0.5;
          this.tilesPendingInjection.Add(new InjectedTile(tileInjectionRule, isOnMainPath, randomStream));
        }
      }
      if (this.TileInjectionMethods == null)
        return;
      this.TileInjectionMethods(randomStream, ref this.tilesPendingInjection);
    }

    protected virtual IEnumerator GenerateMainPath()
    {
      this.ChangeStatus(GenerationStatus.MainPath);
      this.nextNodeIndex = 0;
      List<GraphNode> graphNodeList = new List<GraphNode>(this.DungeonFlow.Nodes.Count);
      bool flag = false;
      int num = 0;
      List<List<TileSet>> tileSets = new List<List<TileSet>>(this.targetLength);
      List<DungeonArchetype> archetypes = new List<DungeonArchetype>(this.targetLength);
      List<GraphNode> nodes = new List<GraphNode>(this.targetLength);
      List<GraphLine> lines = new List<GraphLine>(this.targetLength);
      while (!flag)
      {
        float normalizedDepth = Mathf.Clamp((float) num / (float) (this.targetLength - 1), 0.0f, 1f);
        GraphLine lineAtDepth = this.DungeonFlow.GetLineAtDepth(normalizedDepth);
        if (lineAtDepth == null)
        {
          yield return (object) this.Wait(this.InnerGenerate(true));
          yield break;
        }
        else
        {
          if (lineAtDepth != this.previousLineSegment)
          {
            this.currentArchetype = lineAtDepth.GetRandomArchetype(this.RandomStream, (IList<DungeonArchetype>) archetypes);
            this.previousLineSegment = lineAtDepth;
          }
          GraphNode graphNode1 = (GraphNode) null;
          GraphNode[] array = this.DungeonFlow.Nodes.OrderBy<GraphNode, float>((Func<GraphNode, float>) (x => x.Position)).ToArray<GraphNode>();
          foreach (GraphNode graphNode2 in array)
          {
            if ((double) normalizedDepth >= (double) graphNode2.Position && !graphNodeList.Contains(graphNode2))
            {
              graphNode1 = graphNode2;
              graphNodeList.Add(graphNode2);
              break;
            }
          }
          List<TileSet> tileSets1;
          if (graphNode1 != null)
          {
            tileSets1 = graphNode1.TileSets;
            this.nextNodeIndex = this.nextNodeIndex >= array.Length - 1 ? -1 : this.nextNodeIndex + 1;
            archetypes.Add((DungeonArchetype) null);
            lines.Add((GraphLine) null);
            nodes.Add(graphNode1);
            if (graphNode1 == array[array.Length - 1])
              flag = true;
          }
          else
          {
            tileSets1 = this.currentArchetype.TileSets;
            archetypes.Add(this.currentArchetype);
            lines.Add(lineAtDepth);
            nodes.Add((GraphNode) null);
          }
          tileSets.Add(tileSets1);
          ++num;
        }
      }
      int tileRetryCount = 0;
      int totalForLoopRetryCount = 0;
      for (int j = 0; j < tileSets.Count; ++j)
      {
        TileProxy tileProxy = this.AddTile(j == 0 ? (TileProxy) null : this.proxyDungeon.MainPathTiles[this.proxyDungeon.MainPathTiles.Count - 1], (IEnumerable<TileSet>) tileSets[j], (float) j / (float) (tileSets.Count - 1), archetypes[j]);
        if (j > 5 && tileProxy == null && tileRetryCount < 5 && totalForLoopRetryCount < 20)
        {
          TileProxy mainPathTile = this.proxyDungeon.MainPathTiles[j - 1];
          InjectedTile injectedTile;
          if (this.injectedTiles.TryGetValue(mainPathTile, out injectedTile))
          {
            this.tilesPendingInjection.Add(injectedTile);
            this.injectedTiles.Remove(mainPathTile);
          }
          this.proxyDungeon.RemoveLastConnection();
          this.proxyDungeon.RemoveTile(mainPathTile);
          j -= 2;
          ++tileRetryCount;
          ++totalForLoopRetryCount;
        }
        else
        {
          if (tileProxy == null)
          {
            yield return (object) this.Wait(this.InnerGenerate(true));
            break;
          }
          tileProxy.Placement.GraphNode = nodes[j];
          tileProxy.Placement.GraphLine = lines[j];
          tileRetryCount = 0;
          if (this.ShouldSkipFrame(true))
            yield return (object) this.GetRoomPause();
        }
      }
    }

    private bool ShouldSkipFrame(bool isRoomPlacement)
    {
      if (!this.GenerateAsynchronously)
        return false;
      if (isRoomPlacement && (double) this.PauseBetweenRooms > 0.0)
        return true;
      if (this.yieldTimer.Elapsed.TotalMilliseconds < (double) this.MaxAsyncFrameMilliseconds)
        return false;
      this.yieldTimer.Restart();
      return true;
    }

    private YieldInstruction GetRoomPause()
    {
      return (double) this.PauseBetweenRooms > 0.0 ? (YieldInstruction) new WaitForSeconds(this.PauseBetweenRooms) : (YieldInstruction) null;
    }

    protected virtual IEnumerator GenerateBranchPaths()
    {
      this.ChangeStatus(GenerationStatus.Branching);
      int[] mainPathBranches = new int[this.proxyDungeon.MainPathTiles.Count];
      BranchCountHelper.ComputeBranchCounts(this.DungeonFlow, this.RandomStream, this.proxyDungeon, ref mainPathBranches);
      for (int b = 0; b < mainPathBranches.Length; ++b)
      {
        TileProxy tile = this.proxyDungeon.MainPathTiles[b];
        int branchCount = mainPathBranches[b];
        if (!((UnityEngine.Object) tile.Placement.Archetype == (UnityEngine.Object) null) && branchCount != 0)
        {
          for (int i = 0; i < branchCount; ++i)
          {
            TileProxy previousTile = tile;
            int branchDepth = tile.Placement.Archetype.BranchingDepth.GetRandom(this.RandomStream);
            for (int j = 0; j < branchDepth; ++j)
            {
              List<TileSet> useableTileSets = j != branchDepth - 1 || !tile.Placement.Archetype.GetHasValidBranchCapTiles() ? tile.Placement.Archetype.TileSets : (tile.Placement.Archetype.BranchCapType != BranchCapType.InsteadOf ? tile.Placement.Archetype.TileSets.Concat<TileSet>((IEnumerable<TileSet>) tile.Placement.Archetype.BranchCapTileSets).ToList<TileSet>() : tile.Placement.Archetype.BranchCapTileSets);
              float normalizedDepth = branchDepth <= 1 ? 1f : (float) j / (float) (branchDepth - 1);
              TileProxy tileProxy = this.AddTile(previousTile, (IEnumerable<TileSet>) useableTileSets, normalizedDepth, tile.Placement.Archetype);
              if (tileProxy != null)
              {
                tileProxy.Placement.BranchDepth = j;
                tileProxy.Placement.NormalizedBranchDepth = normalizedDepth;
                tileProxy.Placement.GraphNode = previousTile.Placement.GraphNode;
                tileProxy.Placement.GraphLine = previousTile.Placement.GraphLine;
                previousTile = tileProxy;
                if (this.ShouldSkipFrame(true))
                  yield return (object) this.GetRoomPause();
              }
              else
                break;
            }
            previousTile = (TileProxy) null;
          }
          tile = (TileProxy) null;
        }
      }
    }

    protected virtual TileProxy AddTile(
      TileProxy attachTo,
      IEnumerable<TileSet> useableTileSets,
      float normalizedDepth,
      DungeonArchetype archetype,
      TilePlacementResult result = TilePlacementResult.None)
    {
      bool isOnMainPath = this.Status == GenerationStatus.MainPath;
      bool flag1 = attachTo == null;
      InjectedTile injectedTile1 = (InjectedTile) null;
      int index1 = -1;
      bool flag2 = isOnMainPath && (UnityEngine.Object) archetype == (UnityEngine.Object) null;
      if (this.tilesPendingInjection != null && !flag2)
      {
        float pathDepth = isOnMainPath ? normalizedDepth : (float) attachTo.Placement.PathDepth / ((float) this.targetLength - 1f);
        float branchDepth = isOnMainPath ? 0.0f : normalizedDepth;
        for (int index2 = 0; index2 < this.tilesPendingInjection.Count; ++index2)
        {
          InjectedTile injectedTile2 = this.tilesPendingInjection[index2];
          if (injectedTile2.ShouldInjectTileAtPoint(isOnMainPath, pathDepth, branchDepth))
          {
            injectedTile1 = injectedTile2;
            index1 = index2;
            break;
          }
        }
      }
      IEnumerable<GameObjectChance> collection = injectedTile1 == null ? useableTileSets.SelectMany<TileSet, GameObjectChance>((Func<TileSet, IEnumerable<GameObjectChance>>) (x => (IEnumerable<GameObjectChance>) x.TileWeights.Weights)) : (IEnumerable<GameObjectChance>) new List<GameObjectChance>((IEnumerable<GameObjectChance>) injectedTile1.TileSet.TileWeights.Weights);
      bool flag3 = !flag1 && attachTo.PrefabTile.AllowRotation;
      if (this.OverrideAllowTileRotation)
        flag3 = this.AllowTileRotation;
      Queue<DoorwayPair> doorwayPairs = new DoorwayPairFinder()
      {
        DungeonFlow = this.DungeonFlow,
        RandomStream = this.RandomStream,
        Archetype = archetype,
        GetTileTemplateDelegate = new GetTileTemplateDelegate(this.GetTileTemplate),
        IsOnMainPath = isOnMainPath,
        NormalizedDepth = normalizedDepth,
        PreviousTile = attachTo,
        UpVector = this.UpVector,
        AllowRotation = new bool?(flag3),
        TileWeights = new List<GameObjectChance>(collection),
        IsTileAllowedPredicate = ((TileMatchDelegate) ((TileProxy previousTile, TileProxy potentialNextTile, ref float weight) =>
        {
          bool flag4 = previousTile != null && (UnityEngine.Object) potentialNextTile.Prefab == (UnityEngine.Object) previousTile.Prefab;
          TileRepeatMode tileRepeatMode = TileRepeatMode.Allow;
          if (this.OverrideRepeatMode)
            tileRepeatMode = this.RepeatMode;
          else if (potentialNextTile != null)
            tileRepeatMode = potentialNextTile.PrefabTile.RepeatMode;
          switch (tileRepeatMode)
          {
            case TileRepeatMode.Allow:
              return true;
            case TileRepeatMode.DisallowImmediate:
              return !flag4;
            case TileRepeatMode.Disallow:
              return !this.proxyDungeon.AllTiles.Where<TileProxy>((Func<TileProxy, bool>) (t => (UnityEngine.Object) t.Prefab == (UnityEngine.Object) potentialNextTile.Prefab)).Any<TileProxy>();
            default:
              throw new NotImplementedException("TileRepeatMode " + tileRepeatMode.ToString() + " is not implemented");
          }
        }))
      }.GetDoorwayPairs(this.UseMaximumPairingAttempts ? new int?(this.MaxPairingAttempts) : new int?());
      TilePlacementResult result1 = TilePlacementResult.NoValidTile;
      TileProxy tile = (TileProxy) null;
      while (doorwayPairs.Count > 0)
      {
        result1 = this.TryPlaceTile(doorwayPairs.Dequeue(), archetype, out tile);
        if (result1 != TilePlacementResult.None)
          this.AddTilePlacementResult(result1);
        else
          break;
      }
      if (result1 != TilePlacementResult.None)
        return (TileProxy) null;
      if (injectedTile1 != null)
      {
        tile.Placement.InjectionData = injectedTile1;
        this.injectedTiles[tile] = injectedTile1;
        this.tilesPendingInjection.RemoveAt(index1);
        if (isOnMainPath)
          ++this.targetLength;
      }
      return tile;
    }

    protected void AddTilePlacementResult(TilePlacementResult result)
    {
      int num;
      if (!this.tilePlacementResultCounters.TryGetValue(result, out num))
        this.tilePlacementResultCounters[result] = 1;
      else
        this.tilePlacementResultCounters[result] = num + 1;
    }

    protected TilePlacementResult TryPlaceTile(
      DoorwayPair pair,
      DungeonArchetype archetype,
      out TileProxy tile)
    {
      tile = (TileProxy) null;
      TileProxy nextTemplate = pair.NextTemplate;
      DoorwayProxy previousDoorway = pair.PreviousDoorway;
      if (nextTemplate == null)
        return TilePlacementResult.TemplateIsNull;
      int index = pair.NextTemplate.Doorways.IndexOf(pair.NextDoorway);
      tile = new TileProxy(nextTemplate);
      tile.Placement.IsOnMainPath = this.Status == GenerationStatus.MainPath;
      tile.Placement.Archetype = archetype;
      tile.Placement.TileSet = pair.NextTileSet;
      if (previousDoorway != null)
      {
        DoorwayProxy doorway = tile.Doorways[index];
        tile.PositionBySocket(doorway, previousDoorway);
        Bounds bounds = tile.Placement.Bounds;
        if (this.RestrictDungeonToBounds && !this.TilePlacementBounds.Contains(bounds))
          return TilePlacementResult.OutOfBounds;
        if (this.IsCollidingWithAnyTile(tile, previousDoorway.TileProxy))
          return TilePlacementResult.TileIsColliding;
      }
      if (tile == null)
        return TilePlacementResult.NewTileIsNull;
      if (tile.Placement.IsOnMainPath)
      {
        if (pair.PreviousTile != null)
          tile.Placement.PathDepth = pair.PreviousTile.Placement.PathDepth + 1;
      }
      else
      {
        tile.Placement.PathDepth = pair.PreviousTile.Placement.PathDepth;
        tile.Placement.BranchDepth = pair.PreviousTile.Placement.IsOnMainPath ? 0 : pair.PreviousTile.Placement.BranchDepth + 1;
      }
      if (previousDoorway != null)
      {
        DoorwayProxy doorway = tile.Doorways[index];
        this.proxyDungeon.MakeConnection(previousDoorway, doorway);
      }
      this.proxyDungeon.AddTile(tile);
      return TilePlacementResult.None;
    }

    protected TileProxy GetTileTemplate(GameObject prefab)
    {
      TileProxy tileTemplate = this.preProcessData.Where<TileProxy>((Func<TileProxy, bool>) (x => (UnityEngine.Object) x.Prefab == (UnityEngine.Object) prefab)).FirstOrDefault<TileProxy>();
      if (tileTemplate == null)
      {
        tileTemplate = new TileProxy(prefab, this.IgnoreSpriteBounds, this.UpVector);
        this.preProcessData.Add(tileTemplate);
      }
      return tileTemplate;
    }

    protected TileProxy PickRandomTemplate(DoorwaySocket socketGroupFilter)
    {
      TileProxy tileTemplate = this.GetTileTemplate(this.useableTiles[this.RandomStream.Next(0, this.useableTiles.Count)]);
      return (UnityEngine.Object) socketGroupFilter != (UnityEngine.Object) null && !tileTemplate.UnusedDoorways.Where<DoorwayProxy>((Func<DoorwayProxy, bool>) (d => (UnityEngine.Object) d.Socket == (UnityEngine.Object) socketGroupFilter)).Any<DoorwayProxy>() ? this.PickRandomTemplate(socketGroupFilter) : tileTemplate;
    }

    protected int NormalizedDepthToIndex(float normalizedDepth)
    {
      return Mathf.RoundToInt(normalizedDepth * (float) (this.targetLength - 1));
    }

    protected float IndexToNormalizedDepth(int index) => (float) index / (float) this.targetLength;

    protected bool IsCollidingWithAnyTile(TileProxy newTile, TileProxy previousTile)
    {
      foreach (TileProxy allTile in this.proxyDungeon.AllTiles)
      {
        bool flag = previousTile == allTile;
        float maxOverlap = flag ? this.OverlapThreshold : -this.Padding;
        if (this.DisallowOverhangs && !flag)
        {
          if (newTile.IsOverlappingOrOverhanging(allTile, this.UpDirection, maxOverlap))
            return true;
        }
        else if (newTile.IsOverlapping(allTile, maxOverlap))
          return true;
      }
      return false;
    }

    public void RegisterPostProcessStep(
      Action<DungeonGenerator> postProcessCallback,
      int priority = 0,
      PostProcessPhase phase = PostProcessPhase.AfterBuiltIn)
    {
      this.postProcessSteps.Add(new DungeonGeneratorPostProcessStep(postProcessCallback, priority, phase));
    }

    public void UnregisterPostProcessStep(Action<DungeonGenerator> postProcessCallback)
    {
      for (int index = 0; index < this.postProcessSteps.Count; ++index)
      {
        if (this.postProcessSteps[index].PostProcessCallback == postProcessCallback)
          this.postProcessSteps.RemoveAt(index);
      }
    }

    protected virtual IEnumerator PostProcess()
    {
      DungeonGenerator dungeonGenerator = this;
      int length = dungeonGenerator.proxyDungeon.MainPathTiles.Count;
      int maxBranchDepth = 0;
      if (dungeonGenerator.proxyDungeon.BranchPathTiles.Count > 0)
      {
        List<TileProxy> list = dungeonGenerator.proxyDungeon.BranchPathTiles.ToList<TileProxy>();
        list.Sort((Comparison<TileProxy>) ((a, b) => b.Placement.BranchDepth.CompareTo(a.Placement.BranchDepth)));
        maxBranchDepth = list[0].Placement.BranchDepth;
      }
      yield return (object) null;
      dungeonGenerator.GenerationStats.BeginTime(GenerationStatus.PostProcessing);
      dungeonGenerator.ChangeStatus(GenerationStatus.PostProcessing);
      dungeonGenerator.postProcessSteps.Sort((Comparison<DungeonGeneratorPostProcessStep>) ((a, b) => b.Priority.CompareTo(a.Priority)));
      DungeonGeneratorPostProcessStep step;
      foreach (DungeonGeneratorPostProcessStep postProcessStep in dungeonGenerator.postProcessSteps)
      {
        step = postProcessStep;
        if (dungeonGenerator.ShouldSkipFrame(false))
          yield return (object) null;
        if (step.Phase == PostProcessPhase.BeforeBuiltIn)
          step.PostProcessCallback(dungeonGenerator);
        step = new DungeonGeneratorPostProcessStep();
      }
      yield return (object) null;
      if (!dungeonGenerator.IsAnalysis)
      {
        Tile tile;
        foreach (Tile allTile in dungeonGenerator.CurrentDungeon.AllTiles)
        {
          tile = allTile;
          if (dungeonGenerator.ShouldSkipFrame(false))
            yield return (object) null;
          tile.Placement.NormalizedPathDepth = (float) tile.Placement.PathDepth / (float) (length - 1);
          tile = (Tile) null;
        }
        dungeonGenerator.CurrentDungeon.PostGenerateDungeon(dungeonGenerator);
        foreach (Tile allTile in dungeonGenerator.CurrentDungeon.AllTiles)
        {
          tile = allTile;
          if (dungeonGenerator.ShouldSkipFrame(false))
            yield return (object) null;
          dungeonGenerator.ProcessProps(tile, tile.gameObject);
          tile = (Tile) null;
        }
        dungeonGenerator.ProcessGlobalProps();
        if ((UnityEngine.Object) dungeonGenerator.DungeonFlow.KeyManager != (UnityEngine.Object) null)
          dungeonGenerator.PlaceLocksAndKeys();
      }
      dungeonGenerator.GenerationStats.SetRoomStatistics(dungeonGenerator.CurrentDungeon.MainPathTiles.Count, dungeonGenerator.CurrentDungeon.BranchPathTiles.Count, maxBranchDepth);
      dungeonGenerator.preProcessData.Clear();
      yield return (object) null;
      foreach (DungeonGeneratorPostProcessStep postProcessStep in dungeonGenerator.postProcessSteps)
      {
        step = postProcessStep;
        if (dungeonGenerator.ShouldSkipFrame(false))
          yield return (object) null;
        if (step.Phase == PostProcessPhase.AfterBuiltIn)
          step.PostProcessCallback(dungeonGenerator);
        step = new DungeonGeneratorPostProcessStep();
      }
      dungeonGenerator.GenerationStats.EndTime();
      foreach (GameObject door in dungeonGenerator.CurrentDungeon.Doors)
      {
        if ((UnityEngine.Object) door != (UnityEngine.Object) null)
          door.SetActive(true);
      }
    }

    protected void ProcessProps(Tile tile, GameObject root)
    {
      if ((UnityEngine.Object) root == (UnityEngine.Object) null)
        return;
      foreach (RandomProp component in root.GetComponents<RandomProp>())
        component.Process(this.RandomStream, tile);
      if ((UnityEngine.Object) root == (UnityEngine.Object) null)
        return;
      int childCount = root.transform.childCount;
      List<GameObject> gameObjectList = new List<GameObject>(childCount);
      for (int index = 0; index < childCount; ++index)
      {
        GameObject gameObject = root.transform.GetChild(index).gameObject;
        gameObjectList.Add(gameObject);
      }
      foreach (GameObject root1 in gameObjectList)
      {
        if ((UnityEngine.Object) root1 != (UnityEngine.Object) null)
          this.ProcessProps(tile, root1);
      }
    }

    protected virtual void ProcessGlobalProps()
    {
      Dictionary<int, GameObjectChanceTable> dictionary = new Dictionary<int, GameObjectChanceTable>();
      foreach (Tile allTile in this.CurrentDungeon.AllTiles)
      {
        foreach (GlobalProp componentsInChild in allTile.GetComponentsInChildren<GlobalProp>())
        {
          GameObjectChanceTable objectChanceTable = (GameObjectChanceTable) null;
          if (!dictionary.TryGetValue(componentsInChild.PropGroupID, out objectChanceTable))
          {
            objectChanceTable = new GameObjectChanceTable();
            dictionary[componentsInChild.PropGroupID] = objectChanceTable;
          }
          float mainPathWeight = (allTile.Placement.IsOnMainPath ? componentsInChild.MainPathWeight : componentsInChild.BranchPathWeight) * componentsInChild.DepthWeightScale.Evaluate(allTile.Placement.NormalizedDepth);
          objectChanceTable.Weights.Add(new GameObjectChance(componentsInChild.gameObject, mainPathWeight, 0.0f, (TileSet) null));
        }
      }
      foreach (GameObjectChanceTable objectChanceTable in dictionary.Values)
      {
        foreach (GameObjectChance weight in objectChanceTable.Weights)
          weight.Value.SetActive(false);
      }
      List<int> intList = new List<int>(dictionary.Count);
      foreach (KeyValuePair<int, GameObjectChanceTable> keyValuePair in dictionary)
      {
        KeyValuePair<int, GameObjectChanceTable> pair = keyValuePair;
        if (intList.Contains(pair.Key))
        {
          UnityEngine.Debug.LogWarning((object) ("Dungeon Flow contains multiple entries for the global prop group ID: " + pair.Key.ToString() + ". Only the first entry will be used."));
        }
        else
        {
          DungeonFlow.GlobalPropSettings globalPropSettings = this.DungeonFlow.GlobalProps.Where<DungeonFlow.GlobalPropSettings>((Func<DungeonFlow.GlobalPropSettings, bool>) (x => x.ID == pair.Key)).FirstOrDefault<DungeonFlow.GlobalPropSettings>();
          if (globalPropSettings != null)
          {
            GameObjectChanceTable objectChanceTable = pair.Value.Clone();
            int num = Mathf.Clamp(globalPropSettings.Count.GetRandom(this.RandomStream), 0, objectChanceTable.Weights.Count);
            for (int index = 0; index < num; ++index)
            {
              GameObjectChance random = objectChanceTable.GetRandom(this.RandomStream, true, 0.0f, (GameObject) null, true, true);
              if (random != null && (UnityEngine.Object) random.Value != (UnityEngine.Object) null)
                random.Value.SetActive(true);
            }
            intList.Add(pair.Key);
          }
        }
      }
    }

    protected virtual void PlaceLocksAndKeys()
    {
      GraphNode[] array1 = this.CurrentDungeon.ConnectionGraph.Nodes.Select<DungeonGraphNode, GraphNode>((Func<DungeonGraphNode, GraphNode>) (x => x.Tile.Placement.GraphNode)).Where<GraphNode>((Func<GraphNode, bool>) (x => x != null)).Distinct<GraphNode>().ToArray<GraphNode>();
      GraphLine[] array2 = this.CurrentDungeon.ConnectionGraph.Nodes.Select<DungeonGraphNode, GraphLine>((Func<DungeonGraphNode, GraphLine>) (x => x.Tile.Placement.GraphLine)).Where<GraphLine>((Func<GraphLine, bool>) (x => x != null)).Distinct<GraphLine>().ToArray<GraphLine>();
      Dictionary<Doorway, Key> lockedDoorways = new Dictionary<Doorway, Key>();
      foreach (GraphNode graphNode in array1)
      {
        GraphNode node = graphNode;
        foreach (KeyLockPlacement keyLockPlacement in node.Locks)
        {
          Tile tile = this.CurrentDungeon.AllTiles.Where<Tile>((Func<Tile, bool>) (x => x.Placement.GraphNode == node)).FirstOrDefault<Tile>();
          List<DungeonGraphConnection> connections = this.CurrentDungeon.ConnectionGraph.Nodes.Where<DungeonGraphNode>((Func<DungeonGraphNode, bool>) (x => (UnityEngine.Object) x.Tile == (UnityEngine.Object) tile)).FirstOrDefault<DungeonGraphNode>().Connections;
          Doorway key1 = (Doorway) null;
          Doorway key2 = (Doorway) null;
          foreach (DungeonGraphConnection dungeonGraphConnection in connections)
          {
            if ((UnityEngine.Object) dungeonGraphConnection.DoorwayA.Tile == (UnityEngine.Object) tile)
              key2 = dungeonGraphConnection.DoorwayA;
            else if ((UnityEngine.Object) dungeonGraphConnection.DoorwayB.Tile == (UnityEngine.Object) tile)
              key1 = dungeonGraphConnection.DoorwayB;
          }
          Key keyById = node.Graph.KeyManager.GetKeyByID(keyLockPlacement.ID);
          if ((UnityEngine.Object) key1 != (UnityEngine.Object) null && (node.LockPlacement & NodeLockPlacement.Entrance) == NodeLockPlacement.Entrance)
            lockedDoorways.Add(key1, keyById);
          if ((UnityEngine.Object) key2 != (UnityEngine.Object) null && (node.LockPlacement & NodeLockPlacement.Exit) == NodeLockPlacement.Exit)
            lockedDoorways.Add(key2, keyById);
        }
      }
      foreach (GraphLine graphLine in array2)
      {
        GraphLine line = graphLine;
        List<Doorway> list = this.CurrentDungeon.ConnectionGraph.Connections.Where<DungeonGraphConnection>((Func<DungeonGraphConnection, bool>) (x =>
        {
          bool flag1 = lockedDoorways.ContainsKey(x.DoorwayA) || lockedDoorways.ContainsKey(x.DoorwayB);
          bool flag2 = x.DoorwayA.Tile.Placement.TileSet.LockPrefabs.Count > 0;
          return ((x.DoorwayA.Tile.Placement.GraphLine != line || x.DoorwayB.Tile.Placement.GraphLine != line ? 0 : (!flag1 ? 1 : 0)) & (flag2 ? 1 : 0)) != 0;
        })).Select<DungeonGraphConnection, Doorway>((Func<DungeonGraphConnection, Doorway>) (x => x.DoorwayA)).ToList<Doorway>();
        if (list.Count != 0)
        {
          using (List<KeyLockPlacement>.Enumerator enumerator = line.Locks.GetEnumerator())
          {
label_28:
            while (enumerator.MoveNext())
            {
              KeyLockPlacement current = enumerator.Current;
              int num1 = Mathf.Clamp(current.Range.GetRandom(this.RandomStream), 0, list.Count);
              int num2 = 0;
              while (true)
              {
                if (num2 < num1 && list.Count != 0)
                {
                  Doorway key = list[this.RandomStream.Next(0, list.Count)];
                  list.Remove(key);
                  if (!lockedDoorways.ContainsKey(key))
                  {
                    Key keyById = line.Graph.KeyManager.GetKeyByID(current.ID);
                    lockedDoorways.Add(key, keyById);
                  }
                  ++num2;
                }
                else
                  goto label_28;
              }
            }
          }
        }
      }
      foreach (Tile allTile in this.CurrentDungeon.AllTiles)
      {
        if (allTile.Placement.InjectionData != null && allTile.Placement.InjectionData.IsLocked)
        {
          List<Doorway> source = new List<Doorway>();
          foreach (Doorway usedDoorway in allTile.UsedDoorways)
          {
            if ((lockedDoorways.ContainsKey(usedDoorway) ? 1 : (lockedDoorways.ContainsKey(usedDoorway.ConnectedDoorway) ? 1 : 0)) == 0 & allTile.Placement.TileSet.LockPrefabs.Count > 0 & (UnityEngine.Object) allTile.GetEntranceDoorway() == (UnityEngine.Object) usedDoorway)
              source.Add(usedDoorway);
          }
          if (source.Any<Doorway>())
          {
            Doorway key = source.First<Doorway>();
            Key keyById = this.DungeonFlow.KeyManager.GetKeyByID(allTile.Placement.InjectionData.LockID);
            lockedDoorways.Add(key, keyById);
          }
        }
      }
      List<Doorway> doorwayList = new List<Doorway>();
      foreach (KeyValuePair<Doorway, Key> keyValuePair in lockedDoorways)
      {
        Doorway key3 = keyValuePair.Key;
        Key key = keyValuePair.Value;
        List<Tile> source = new List<Tile>();
        foreach (Tile allTile in this.CurrentDungeon.AllTiles)
        {
          if ((double) allTile.Placement.NormalizedPathDepth < (double) key3.Tile.Placement.NormalizedPathDepth)
          {
            bool flag = false;
            if (allTile.Placement.GraphNode != null && allTile.Placement.GraphNode.Keys.Where<KeyLockPlacement>((Func<KeyLockPlacement, bool>) (x => x.ID == key.ID)).Count<KeyLockPlacement>() > 0)
              flag = true;
            else if (allTile.Placement.GraphLine != null && allTile.Placement.GraphLine.Keys.Where<KeyLockPlacement>((Func<KeyLockPlacement, bool>) (x => x.ID == key.ID)).Count<KeyLockPlacement>() > 0)
              flag = true;
            if (flag)
              source.Add(allTile);
          }
        }
        List<IKeySpawnable> list = source.SelectMany<Tile, IKeySpawnable>((Func<Tile, IEnumerable<IKeySpawnable>>) (x => x.GetComponentsInChildren<Component>().OfType<IKeySpawnable>())).ToList<IKeySpawnable>();
        if (list.Count == 0)
        {
          doorwayList.Add(key3);
        }
        else
        {
          int num = Math.Min(key.KeysPerLock.GetRandom(this.RandomStream), list.Count);
          for (int index1 = 0; index1 < num; ++index1)
          {
            int index2 = this.RandomStream.Next(0, list.Count);
            IKeySpawnable keySpawnable = list[index2];
            keySpawnable.SpawnKey(key, this.DungeonFlow.KeyManager);
            foreach (IKeyLock keyLock in (keySpawnable as Component).GetComponentsInChildren<Component>().OfType<IKeyLock>())
              keyLock.OnKeyAssigned(key, this.DungeonFlow.KeyManager);
            list.RemoveAt(index2);
          }
        }
      }
      foreach (Doorway key in doorwayList)
        lockedDoorways.Remove(key);
      foreach (KeyValuePair<Doorway, Key> keyValuePair in lockedDoorways)
      {
        keyValuePair.Key.RemoveUsedPrefab();
        this.LockDoorway(keyValuePair.Key, keyValuePair.Value, this.DungeonFlow.KeyManager);
      }
    }

    protected virtual void LockDoorway(Doorway doorway, Key key, KeyManager keyManager)
    {
      TilePlacementData placement = doorway.Tile.Placement;
      GameObjectChanceTable[] array = doorway.Tile.Placement.TileSet.LockPrefabs.Where<LockedDoorwayAssociation>((Func<LockedDoorwayAssociation, bool>) (x =>
      {
        DoorwaySocket socket = x.Socket;
        return (UnityEngine.Object) socket == (UnityEngine.Object) null || DoorwaySocket.CanSocketsConnect(socket, doorway.Socket);
      })).Select<LockedDoorwayAssociation, GameObjectChanceTable>((Func<LockedDoorwayAssociation, GameObjectChanceTable>) (x => x.LockPrefabs)).ToArray<GameObjectChanceTable>();
      if (array.Length == 0)
        return;
      GameObject doorPrefab = UnityEngine.Object.Instantiate<GameObject>(array[this.RandomStream.Next(0, array.Length)].GetRandom(this.RandomStream, placement.IsOnMainPath, placement.NormalizedDepth, (GameObject) null, true).Value, doorway.transform);
      DungeonUtil.AddAndSetupDoorComponent(this.CurrentDungeon, doorPrefab, doorway);
      doorway.SetUsedPrefab(doorPrefab);
      doorway.ConnectedDoorway.SetUsedPrefab(doorPrefab);
      foreach (IKeyLock keyLock in doorPrefab.GetComponentsInChildren<Component>().OfType<IKeyLock>())
        keyLock.OnKeyAssigned(key, keyManager);
    }

    public void OnBeforeSerialize() => this.fileVersion = 1;

    public void OnAfterDeserialize()
    {
      if (this.fileVersion >= 1)
        return;
      this.RepeatMode = this.allowImmediateRepeats ? TileRepeatMode.Allow : TileRepeatMode.DisallowImmediate;
    }
  }
}
