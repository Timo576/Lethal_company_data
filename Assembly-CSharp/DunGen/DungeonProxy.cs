// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonProxy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Graph;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class DungeonProxy
  {
    public List<TileProxy> AllTiles = new List<TileProxy>();
    public List<TileProxy> MainPathTiles = new List<TileProxy>();
    public List<TileProxy> BranchPathTiles = new List<TileProxy>();
    public List<ProxyDoorwayConnection> Connections = new List<ProxyDoorwayConnection>();
    private Transform visualsRoot;
    private Dictionary<TileProxy, GameObject> tileVisuals = new Dictionary<TileProxy, GameObject>();

    public DungeonProxy(Transform debugVisualsRoot = null) => this.visualsRoot = debugVisualsRoot;

    public void ClearDebugVisuals()
    {
      foreach (UnityEngine.Object @object in this.tileVisuals.Values.ToArray<GameObject>())
        UnityEngine.Object.DestroyImmediate(@object);
      this.tileVisuals.Clear();
    }

    public void MakeConnection(DoorwayProxy a, DoorwayProxy b)
    {
      DoorwayProxy.Connect(a, b);
      this.Connections.Add(new ProxyDoorwayConnection(a, b));
    }

    public void RemoveLastConnection()
    {
      this.RemoveConnection(this.Connections.Last<ProxyDoorwayConnection>());
    }

    public void RemoveConnection(ProxyDoorwayConnection connection)
    {
      connection.A.Disconnect();
      this.Connections.Remove(connection);
    }

    internal void AddTile(TileProxy tile)
    {
      this.AllTiles.Add(tile);
      if (tile.Placement.IsOnMainPath)
        this.MainPathTiles.Add(tile);
      else
        this.BranchPathTiles.Add(tile);
      if (!((UnityEngine.Object) this.visualsRoot != (UnityEngine.Object) null))
        return;
      GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(tile.Prefab, this.visualsRoot);
      gameObject.transform.localPosition = tile.Placement.Position;
      gameObject.transform.localRotation = tile.Placement.Rotation;
      this.tileVisuals[tile] = gameObject;
    }

    internal void RemoveTile(TileProxy tile)
    {
      this.AllTiles.Remove(tile);
      if (tile.Placement.IsOnMainPath)
        this.MainPathTiles.Remove(tile);
      else
        this.BranchPathTiles.Remove(tile);
      GameObject gameObject;
      if (!this.tileVisuals.TryGetValue(tile, out gameObject))
        return;
      UnityEngine.Object.DestroyImmediate((UnityEngine.Object) gameObject);
      this.tileVisuals.Remove(tile);
    }

    internal void ConnectOverlappingDoorways(
      float globalChance,
      DungeonFlow dungeonFlow,
      RandomStream randomStream)
    {
      IEnumerable<DoorwayProxy> doorwayProxies = this.AllTiles.SelectMany<TileProxy, DoorwayProxy>((Func<TileProxy, IEnumerable<DoorwayProxy>>) (t => t.UnusedDoorways));
      foreach (DoorwayProxy a in doorwayProxies)
      {
        foreach (DoorwayProxy b in doorwayProxies)
        {
          if (!a.Used && !b.Used && a != b && a.TileProxy != b.TileProxy && dungeonFlow.CanDoorwaysConnect(a.TileProxy.PrefabTile, b.TileProxy.PrefabTile, a.DoorwayComponent, b.DoorwayComponent) && (double) (a.Position - b.Position).sqrMagnitude < 9.9999997473787516E-06)
          {
            if (dungeonFlow.RestrictConnectionToSameSection)
            {
              bool flag = a.TileProxy.Placement.GraphLine == b.TileProxy.Placement.GraphLine;
              if (a.TileProxy.Placement.GraphLine == null)
                flag = false;
              if (!flag)
                continue;
            }
            float num = globalChance;
            if (a.TileProxy.PrefabTile.OverrideConnectionChance && b.TileProxy.PrefabTile.OverrideConnectionChance)
              num = Mathf.Min(a.TileProxy.PrefabTile.ConnectionChance, b.TileProxy.PrefabTile.ConnectionChance);
            else if (a.TileProxy.PrefabTile.OverrideConnectionChance)
              num = a.TileProxy.PrefabTile.ConnectionChance;
            else if (b.TileProxy.PrefabTile.OverrideConnectionChance)
              num = b.TileProxy.PrefabTile.ConnectionChance;
            if ((double) num > 0.0 && randomStream.NextDouble() < (double) num)
              this.MakeConnection(a, b);
          }
        }
      }
    }
  }
}
