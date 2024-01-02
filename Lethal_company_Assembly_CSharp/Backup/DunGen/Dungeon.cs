// Decompiled with JetBrains decompiler
// Type: DunGen.Dungeon
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public class Dungeon : MonoBehaviour
  {
    public bool DebugRender;
    private readonly List<Tile> allTiles = new List<Tile>();
    private readonly List<Tile> mainPathTiles = new List<Tile>();
    private readonly List<Tile> branchPathTiles = new List<Tile>();
    private readonly List<GameObject> doors = new List<GameObject>();
    private readonly List<DoorwayConnection> connections = new List<DoorwayConnection>();

    public Bounds Bounds { get; protected set; }

    public DungeonFlow DungeonFlow { get; protected set; }

    public ReadOnlyCollection<Tile> AllTiles { get; private set; }

    public ReadOnlyCollection<Tile> MainPathTiles { get; private set; }

    public ReadOnlyCollection<Tile> BranchPathTiles { get; private set; }

    public ReadOnlyCollection<GameObject> Doors { get; private set; }

    public ReadOnlyCollection<DoorwayConnection> Connections { get; private set; }

    public DungeonGraph ConnectionGraph { get; private set; }

    public Dungeon()
    {
      this.AllTiles = new ReadOnlyCollection<Tile>((IList<Tile>) this.allTiles);
      this.MainPathTiles = new ReadOnlyCollection<Tile>((IList<Tile>) this.mainPathTiles);
      this.BranchPathTiles = new ReadOnlyCollection<Tile>((IList<Tile>) this.branchPathTiles);
      this.Doors = new ReadOnlyCollection<GameObject>((IList<GameObject>) this.doors);
      this.Connections = new ReadOnlyCollection<DoorwayConnection>((IList<DoorwayConnection>) this.connections);
    }

    internal void AddAdditionalDoor(Door door)
    {
      if (!((UnityEngine.Object) door != (UnityEngine.Object) null))
        return;
      this.doors.Add(door.gameObject);
    }

    internal void PreGenerateDungeon(DungeonGenerator dungeonGenerator)
    {
      this.DungeonFlow = dungeonGenerator.DungeonFlow;
    }

    internal void PostGenerateDungeon(DungeonGenerator dungeonGenerator)
    {
      this.ConnectionGraph = new DungeonGraph(this);
      this.Bounds = UnityUtil.CombineBounds(this.allTiles.Select<Tile, Bounds>((Func<Tile, Bounds>) (x => x.Placement.Bounds)).ToArray<Bounds>());
    }

    public void Clear()
    {
      foreach (Tile allTile in this.allTiles)
      {
        foreach (Doorway usedDoorway in allTile.UsedDoorways)
        {
          if ((UnityEngine.Object) usedDoorway.UsedDoorPrefabInstance != (UnityEngine.Object) null)
            UnityUtil.Destroy((UnityEngine.Object) usedDoorway.UsedDoorPrefabInstance);
        }
        UnityUtil.Destroy((UnityEngine.Object) allTile.gameObject);
      }
      for (int index = 0; index < this.transform.childCount; ++index)
        UnityUtil.Destroy((UnityEngine.Object) this.transform.GetChild(index).gameObject);
      this.allTiles.Clear();
      this.mainPathTiles.Clear();
      this.branchPathTiles.Clear();
      this.doors.Clear();
      this.connections.Clear();
    }

    public Doorway GetConnectedDoorway(Doorway doorway)
    {
      foreach (DoorwayConnection connection in this.connections)
      {
        if ((UnityEngine.Object) connection.A == (UnityEngine.Object) doorway)
          return connection.B;
        if ((UnityEngine.Object) connection.B == (UnityEngine.Object) doorway)
          return connection.A;
      }
      return (Doorway) null;
    }

    public void FromProxy(DungeonProxy proxyDungeon, DungeonGenerator generator)
    {
      this.Clear();
      Dictionary<TileProxy, Tile> dictionary = new Dictionary<TileProxy, Tile>();
      foreach (TileProxy allTile in proxyDungeon.AllTiles)
      {
        GameObject gameObject1 = UnityEngine.Object.Instantiate<GameObject>(allTile.Prefab, generator.Root.transform);
        gameObject1.transform.localPosition = allTile.Placement.Position;
        gameObject1.transform.localRotation = allTile.Placement.Rotation;
        Tile component = gameObject1.GetComponent<Tile>();
        component.Dungeon = this;
        component.Placement = new TilePlacementData(allTile.Placement);
        dictionary[allTile] = component;
        this.allTiles.Add(component);
        if (component.Placement.IsOnMainPath)
          this.mainPathTiles.Add(component);
        else
          this.branchPathTiles.Add(component);
        if (generator.PlaceTileTriggers)
        {
          component.AddTriggerVolume();
          component.gameObject.layer = generator.TileTriggerLayer;
        }
        Doorway[] componentsInChildren = gameObject1.GetComponentsInChildren<Doorway>();
        foreach (Doorway doorway in componentsInChildren)
        {
          doorway.Tile = component;
          doorway.placedByGenerator = true;
          doorway.HideConditionalObjects = false;
          component.AllDoorways.Add(doorway);
        }
        foreach (DoorwayProxy usedDoorway in allTile.UsedDoorways)
        {
          Doorway doorway = componentsInChildren[usedDoorway.Index];
          component.UsedDoorways.Add(doorway);
          foreach (GameObject blockerSceneObject in doorway.BlockerSceneObjects)
          {
            if ((UnityEngine.Object) blockerSceneObject != (UnityEngine.Object) null)
              UnityEngine.Object.DestroyImmediate((UnityEngine.Object) blockerSceneObject, false);
          }
        }
        foreach (DoorwayProxy unusedDoorway in allTile.UnusedDoorways)
        {
          Doorway doorway = componentsInChildren[unusedDoorway.Index];
          component.UnusedDoorways.Add(doorway);
          foreach (GameObject connectorSceneObject in doorway.ConnectorSceneObjects)
          {
            if ((UnityEngine.Object) connectorSceneObject != (UnityEngine.Object) null)
              UnityEngine.Object.DestroyImmediate((UnityEngine.Object) connectorSceneObject, false);
          }
          if (doorway.BlockerPrefabWeights.HasAnyViableEntries())
          {
            GameObject gameObject2 = UnityEngine.Object.Instantiate<GameObject>(doorway.BlockerPrefabWeights.GetRandom(generator.RandomStream));
            gameObject2.transform.parent = doorway.gameObject.transform;
            gameObject2.transform.localPosition = Vector3.zero;
            gameObject2.transform.localScale = Vector3.one;
            if (!doorway.AvoidRotatingBlockerPrefab)
              gameObject2.transform.localRotation = Quaternion.identity;
          }
        }
      }
      foreach (ProxyDoorwayConnection connection in proxyDungeon.Connections)
      {
        Tile tile1 = dictionary[connection.A.TileProxy];
        Tile tile2 = dictionary[connection.B.TileProxy];
        Doorway allDoorway1 = tile1.AllDoorways[connection.A.Index];
        Doorway allDoorway2 = tile2.AllDoorways[connection.B.Index];
        allDoorway1.ConnectedDoorway = allDoorway2;
        allDoorway2.ConnectedDoorway = allDoorway1;
        this.connections.Add(new DoorwayConnection(allDoorway1, allDoorway2));
        this.SpawnDoorPrefab(allDoorway1, allDoorway2, generator.RandomStream);
      }
    }

    private void SpawnDoorPrefab(Doorway a, Doorway b, RandomStream randomStream)
    {
      if (a.HasDoorPrefabInstance || b.HasDoorPrefabInstance)
        return;
      bool flag1 = a.ConnectorPrefabWeights.HasAnyViableEntries();
      bool flag2 = b.ConnectorPrefabWeights.HasAnyViableEntries();
      if (!flag1 && !flag2)
        return;
      Doorway doorway = !(flag1 & flag2) ? (flag1 ? a : b) : (a.DoorPrefabPriority < b.DoorPrefabPriority ? b : a);
      GameObject random = doorway.ConnectorPrefabWeights.GetRandom(randomStream);
      if (!((UnityEngine.Object) random != (UnityEngine.Object) null))
        return;
      GameObject doorPrefab = UnityEngine.Object.Instantiate<GameObject>(random, doorway.transform);
      doorPrefab.transform.localPosition = Vector3.zero;
      if (!doorway.AvoidRotatingDoorPrefab)
        doorPrefab.transform.localRotation = Quaternion.identity;
      this.doors.Add(doorPrefab);
      DungeonUtil.AddAndSetupDoorComponent(this, doorPrefab, doorway);
      a.SetUsedPrefab(doorPrefab);
      b.SetUsedPrefab(doorPrefab);
    }

    public void OnDrawGizmos()
    {
      if (!this.DebugRender)
        return;
      this.DebugDraw();
    }

    public void DebugDraw()
    {
      Color red = Color.red;
      Color green = Color.green;
      Color blue = Color.blue;
      Color b = new Color(0.5f, 0.0f, 0.5f);
      float num = 0.75f;
      foreach (Tile allTile in this.allTiles)
      {
        Bounds bounds = allTile.Placement.Bounds;
        bounds.size *= 1.01f;
        Gizmos.color = (allTile.Placement.IsOnMainPath ? Color.Lerp(red, green, allTile.Placement.NormalizedDepth) : Color.Lerp(blue, b, allTile.Placement.NormalizedDepth)) with
        {
          a = num
        };
        Gizmos.DrawCube(bounds.center, bounds.size);
      }
    }
  }
}
