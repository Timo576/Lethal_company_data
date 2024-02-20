// Decompiled with JetBrains decompiler
// Type: DunGen.AdjacentRoomCulling
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Culling/Adjacent Room Culling")]
  public class AdjacentRoomCulling : MonoBehaviour
  {
    public int AdjacentTileDepth = 1;
    public bool CullBehindClosedDoors = true;
    public Transform TargetOverride;
    public bool IncludeDisabledComponents;
    [NonSerialized]
    public Dictionary<Renderer, bool> OverrideRendererVisibilities = new Dictionary<Renderer, bool>();
    [NonSerialized]
    public Dictionary<Light, bool> OverrideLightVisibilities = new Dictionary<Light, bool>();
    protected List<Tile> allTiles;
    protected List<Door> allDoors;
    protected List<Tile> oldVisibleTiles;
    protected List<Tile> visibleTiles;
    protected Dictionary<Tile, bool> tileVisibilities;
    protected Dictionary<Tile, List<Renderer>> tileRenderers;
    protected Dictionary<Tile, List<Light>> lightSources;
    protected Dictionary<Tile, List<ReflectionProbe>> reflectionProbes;
    protected Dictionary<Door, List<Renderer>> doorRenderers;
    private bool dirty;
    private DungeonGenerator generator;
    private Tile currentTile;
    private Queue<Tile> tilesToSearch;
    private List<Tile> searchedTiles;

    public bool Ready { get; protected set; }

    public event AdjacentRoomCulling.VisibilityChangedDelegate TileVisibilityChanged;

    protected Transform targetTransform
    {
      get
      {
        return !((UnityEngine.Object) this.TargetOverride != (UnityEngine.Object) null) ? this.transform : this.TargetOverride;
      }
    }

    protected virtual void OnEnable()
    {
      RuntimeDungeon objectOfType = UnityEngine.Object.FindObjectOfType<RuntimeDungeon>();
      if (!((UnityEngine.Object) objectOfType != (UnityEngine.Object) null))
        return;
      this.generator = objectOfType.Generator;
      this.generator.OnGenerationStatusChanged += new GenerationStatusDelegate(this.OnDungeonGenerationStatusChanged);
      if (this.generator.Status != GenerationStatus.Complete)
        return;
      this.SetDungeon(this.generator.CurrentDungeon);
    }

    protected virtual void OnDisable()
    {
      if (this.generator != null)
        this.generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.OnDungeonGenerationStatusChanged);
      this.ClearDungeon();
    }

    public virtual void SetDungeon(Dungeon dungeon)
    {
      if (this.Ready)
        this.ClearDungeon();
      if ((UnityEngine.Object) dungeon == (UnityEngine.Object) null)
        return;
      this.allTiles = new List<Tile>((IEnumerable<Tile>) dungeon.AllTiles);
      this.allDoors = new List<Door>(this.GetAllDoorsInDungeon(dungeon));
      this.oldVisibleTiles = new List<Tile>(this.allTiles.Count);
      this.visibleTiles = new List<Tile>(this.allTiles.Count);
      this.tileVisibilities = new Dictionary<Tile, bool>();
      this.tileRenderers = new Dictionary<Tile, List<Renderer>>();
      this.lightSources = new Dictionary<Tile, List<Light>>();
      this.reflectionProbes = new Dictionary<Tile, List<ReflectionProbe>>();
      this.doorRenderers = new Dictionary<Door, List<Renderer>>();
      this.UpdateRendererLists();
      foreach (Tile allTile in this.allTiles)
        this.SetTileVisibility(allTile, false);
      foreach (Door allDoor in this.allDoors)
      {
        allDoor.OnDoorStateChanged += new Door.DoorStateChangedDelegate(this.OnDoorStateChanged);
        this.SetDoorVisibility(allDoor, false);
      }
      this.Ready = true;
      this.dirty = true;
    }

    public virtual bool IsTileVisible(Tile tile)
    {
      bool flag;
      return this.tileVisibilities.TryGetValue(tile, out flag) && flag;
    }

    protected IEnumerable<Door> GetAllDoorsInDungeon(Dungeon dungeon)
    {
      foreach (GameObject door in dungeon.Doors)
      {
        if (!((UnityEngine.Object) door == (UnityEngine.Object) null))
        {
          Door component = door.GetComponent<Door>();
          if ((UnityEngine.Object) component != (UnityEngine.Object) null)
            yield return component;
        }
      }
    }

    protected virtual void ClearDungeon()
    {
      if (!this.Ready)
        return;
      foreach (Door allDoor in this.allDoors)
      {
        this.SetDoorVisibility(allDoor, true);
        allDoor.OnDoorStateChanged -= new Door.DoorStateChangedDelegate(this.OnDoorStateChanged);
      }
      foreach (Tile allTile in this.allTiles)
        this.SetTileVisibility(allTile, true);
      this.Ready = false;
    }

    protected virtual void OnDoorStateChanged(Door door, bool isOpen) => this.dirty = true;

    protected virtual void OnDungeonGenerationStatusChanged(
      DungeonGenerator generator,
      GenerationStatus status)
    {
      if (status == GenerationStatus.Complete)
      {
        this.SetDungeon(generator.CurrentDungeon);
      }
      else
      {
        if (status != GenerationStatus.Failed)
          return;
        this.ClearDungeon();
      }
    }

    protected virtual void LateUpdate()
    {
      if (!this.Ready)
        return;
      Tile currentTile = this.currentTile;
      if ((UnityEngine.Object) this.currentTile == (UnityEngine.Object) null)
        this.currentTile = this.FindCurrentTile();
      else if (!this.currentTile.Bounds.Contains(this.targetTransform.position))
        this.currentTile = this.SearchForNewCurrentTile();
      if ((UnityEngine.Object) this.currentTile != (UnityEngine.Object) currentTile)
        this.dirty = true;
      if (this.dirty)
        this.RefreshVisibility();
      this.dirty = false;
    }

    protected virtual void RefreshVisibility()
    {
      List<Tile> visibleTiles = this.visibleTiles;
      this.visibleTiles = this.oldVisibleTiles;
      this.oldVisibleTiles = visibleTiles;
      this.UpdateVisibleTiles();
      foreach (Tile oldVisibleTile in this.oldVisibleTiles)
      {
        if (!this.visibleTiles.Contains(oldVisibleTile))
          this.SetTileVisibility(oldVisibleTile, false);
      }
      foreach (Tile visibleTile in this.visibleTiles)
      {
        if (!this.oldVisibleTiles.Contains(visibleTile))
          this.SetTileVisibility(visibleTile, true);
      }
      this.oldVisibleTiles.Clear();
      this.RefreshDoorVisibilities();
    }

    protected virtual void RefreshDoorVisibilities()
    {
      foreach (Door allDoor in this.allDoors)
      {
        bool visible = this.visibleTiles.Contains(allDoor.DoorwayA.Tile) || this.visibleTiles.Contains(allDoor.DoorwayB.Tile);
        this.SetDoorVisibility(allDoor, visible);
      }
    }

    protected virtual void SetDoorVisibility(Door door, bool visible)
    {
      List<Renderer> rendererList;
      if (!this.doorRenderers.TryGetValue(door, out rendererList))
        return;
      for (int index = rendererList.Count - 1; index >= 0; --index)
      {
        Renderer key = rendererList[index];
        if ((UnityEngine.Object) key == (UnityEngine.Object) null)
        {
          rendererList.RemoveAt(index);
        }
        else
        {
          bool flag;
          key.enabled = !this.OverrideRendererVisibilities.TryGetValue(key, out flag) ? visible : flag;
        }
      }
    }

    protected virtual void UpdateVisibleTiles()
    {
      this.visibleTiles.Clear();
      if ((UnityEngine.Object) this.currentTile != (UnityEngine.Object) null)
        this.visibleTiles.Add(this.currentTile);
      int num = 0;
      for (int index1 = 0; index1 < this.AdjacentTileDepth; ++index1)
      {
        int count = this.visibleTiles.Count;
        for (int index2 = num; index2 < count; ++index2)
        {
          foreach (Doorway usedDoorway in this.visibleTiles[index2].UsedDoorways)
          {
            Tile tile = usedDoorway.ConnectedDoorway.Tile;
            if (!this.visibleTiles.Contains(tile))
            {
              if (this.CullBehindClosedDoors)
              {
                Door doorComponent = usedDoorway.DoorComponent;
                if ((UnityEngine.Object) doorComponent != (UnityEngine.Object) null && doorComponent.ShouldCullBehind)
                  continue;
              }
              this.visibleTiles.Add(tile);
            }
          }
        }
        num = count;
      }
    }

    protected virtual void SetTileVisibility(Tile tile, bool visible)
    {
      this.tileVisibilities[tile] = visible;
      List<Renderer> rendererList;
      if (this.tileRenderers.TryGetValue(tile, out rendererList))
      {
        for (int index = rendererList.Count - 1; index >= 0; --index)
        {
          Renderer key = rendererList[index];
          if ((UnityEngine.Object) key == (UnityEngine.Object) null)
          {
            rendererList.RemoveAt(index);
          }
          else
          {
            bool flag;
            key.enabled = !this.OverrideRendererVisibilities.TryGetValue(key, out flag) ? visible : flag;
          }
        }
      }
      List<Light> lightList;
      if (this.lightSources.TryGetValue(tile, out lightList))
      {
        for (int index = lightList.Count - 1; index >= 0; --index)
        {
          Light key = lightList[index];
          if ((UnityEngine.Object) key == (UnityEngine.Object) null)
          {
            lightList.RemoveAt(index);
          }
          else
          {
            bool flag;
            if (this.OverrideLightVisibilities.TryGetValue(key, out flag))
              key.enabled = flag;
            else
              key.enabled = visible;
          }
        }
      }
      List<ReflectionProbe> reflectionProbeList;
      if (this.reflectionProbes.TryGetValue(tile, out reflectionProbeList))
      {
        for (int index = reflectionProbeList.Count - 1; index >= 0; --index)
        {
          ReflectionProbe reflectionProbe = reflectionProbeList[index];
          if ((UnityEngine.Object) reflectionProbe == (UnityEngine.Object) null)
            reflectionProbeList.RemoveAt(index);
          else
            reflectionProbe.enabled = visible;
        }
      }
      if (this.TileVisibilityChanged == null)
        return;
      this.TileVisibilityChanged(tile, visible);
    }

    public virtual void UpdateRendererLists()
    {
      foreach (Tile allTile in this.allTiles)
      {
        List<Renderer> rendererList;
        if (!this.tileRenderers.TryGetValue(allTile, out rendererList))
          this.tileRenderers[allTile] = rendererList = new List<Renderer>();
        foreach (Renderer componentsInChild in allTile.GetComponentsInChildren<Renderer>())
        {
          if (this.IncludeDisabledComponents || componentsInChild.enabled && componentsInChild.gameObject.activeInHierarchy)
            rendererList.Add(componentsInChild);
        }
        List<Light> lightList;
        if (!this.lightSources.TryGetValue(allTile, out lightList))
          this.lightSources[allTile] = lightList = new List<Light>();
        foreach (Light componentsInChild in allTile.GetComponentsInChildren<Light>())
        {
          if (this.IncludeDisabledComponents || componentsInChild.enabled && componentsInChild.gameObject.activeInHierarchy)
            lightList.Add(componentsInChild);
        }
        List<ReflectionProbe> reflectionProbeList;
        if (!this.reflectionProbes.TryGetValue(allTile, out reflectionProbeList))
          this.reflectionProbes[allTile] = reflectionProbeList = new List<ReflectionProbe>();
        foreach (ReflectionProbe componentsInChild in allTile.GetComponentsInChildren<ReflectionProbe>())
        {
          if (this.IncludeDisabledComponents || componentsInChild.enabled && componentsInChild.gameObject.activeInHierarchy)
            reflectionProbeList.Add(componentsInChild);
        }
      }
      foreach (Door allDoor in this.allDoors)
      {
        List<Renderer> rendererList = new List<Renderer>();
        this.doorRenderers[allDoor] = rendererList;
        foreach (Renderer componentsInChild in allDoor.GetComponentsInChildren<Renderer>(true))
        {
          if (this.IncludeDisabledComponents || componentsInChild.enabled && componentsInChild.gameObject.activeInHierarchy)
            rendererList.Add(componentsInChild);
        }
      }
    }

    protected Tile FindCurrentTile()
    {
      Dungeon objectOfType = UnityEngine.Object.FindObjectOfType<Dungeon>();
      if ((UnityEngine.Object) objectOfType == (UnityEngine.Object) null)
        return (Tile) null;
      foreach (Tile allTile in objectOfType.AllTiles)
      {
        if (allTile.Bounds.Contains(this.targetTransform.position))
          return allTile;
      }
      return (Tile) null;
    }

    protected Tile SearchForNewCurrentTile()
    {
      if (this.tilesToSearch == null)
        this.tilesToSearch = new Queue<Tile>();
      if (this.searchedTiles == null)
        this.searchedTiles = new List<Tile>();
      foreach (Doorway usedDoorway in this.currentTile.UsedDoorways)
      {
        Tile tile = usedDoorway.ConnectedDoorway.Tile;
        if (!this.tilesToSearch.Contains(tile))
          this.tilesToSearch.Enqueue(tile);
      }
      while (this.tilesToSearch.Count > 0)
      {
        Tile tile1 = this.tilesToSearch.Dequeue();
        if (tile1.Bounds.Contains(this.targetTransform.position))
        {
          this.tilesToSearch.Clear();
          this.searchedTiles.Clear();
          return tile1;
        }
        this.searchedTiles.Add(tile1);
        foreach (Doorway usedDoorway in tile1.UsedDoorways)
        {
          Tile tile2 = usedDoorway.ConnectedDoorway.Tile;
          if (!this.tilesToSearch.Contains(tile2) && !this.searchedTiles.Contains(tile2))
            this.tilesToSearch.Enqueue(tile2);
        }
      }
      this.searchedTiles.Clear();
      return (Tile) null;
    }

    public delegate void VisibilityChangedDelegate(Tile tile, bool visible);
  }
}
