// Decompiled with JetBrains decompiler
// Type: DunGen.TileProxy
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class TileProxy
  {
    private List<DoorwayProxy> doorways = new List<DoorwayProxy>();

    public GameObject Prefab { get; private set; }

    public Tile PrefabTile { get; private set; }

    public TilePlacementData Placement { get; internal set; }

    public DoorwayProxy Entrance { get; private set; }

    public DoorwayProxy Exit { get; private set; }

    public ReadOnlyCollection<DoorwayProxy> Doorways { get; private set; }

    public IEnumerable<DoorwayProxy> UsedDoorways
    {
      get => this.doorways.Where<DoorwayProxy>((Func<DoorwayProxy, bool>) (d => d.Used));
    }

    public IEnumerable<DoorwayProxy> UnusedDoorways
    {
      get => this.doorways.Where<DoorwayProxy>((Func<DoorwayProxy, bool>) (d => !d.Used));
    }

    public TileProxy(TileProxy existingTile)
    {
      this.Prefab = existingTile.Prefab;
      this.PrefabTile = existingTile.PrefabTile;
      this.Placement = new TilePlacementData(existingTile.Placement);
      this.Doorways = new ReadOnlyCollection<DoorwayProxy>((IList<DoorwayProxy>) this.doorways);
      foreach (DoorwayProxy doorway in existingTile.doorways)
      {
        DoorwayProxy doorwayProxy = new DoorwayProxy(this, doorway);
        this.doorways.Add(doorwayProxy);
        if (existingTile.Entrance == doorway)
          this.Entrance = doorwayProxy;
        if (existingTile.Exit == doorway)
          this.Exit = doorwayProxy;
      }
    }

    public TileProxy(GameObject prefab, bool ignoreSpriteRendererBounds, Vector3 upVector)
    {
      prefab.transform.localPosition = Vector3.zero;
      prefab.transform.localRotation = Quaternion.identity;
      this.Prefab = prefab;
      this.PrefabTile = prefab.GetComponent<Tile>();
      if ((UnityEngine.Object) this.PrefabTile == (UnityEngine.Object) null)
        this.PrefabTile = prefab.AddComponent<Tile>();
      this.Placement = new TilePlacementData();
      this.Doorways = new ReadOnlyCollection<DoorwayProxy>((IList<DoorwayProxy>) this.doorways);
      Doorway[] componentsInChildren = prefab.GetComponentsInChildren<Doorway>();
      for (int index = 0; index < componentsInChildren.Length; ++index)
      {
        Doorway doorwayComponent = componentsInChildren[index];
        Vector3 position = doorwayComponent.transform.position;
        Quaternion rotation = doorwayComponent.transform.rotation;
        DoorwayProxy doorwayProxy = new DoorwayProxy(this, index, doorwayComponent, position, rotation);
        this.doorways.Add(doorwayProxy);
        if ((UnityEngine.Object) this.PrefabTile.Entrance == (UnityEngine.Object) doorwayComponent)
          this.Entrance = doorwayProxy;
        if ((UnityEngine.Object) this.PrefabTile.Exit == (UnityEngine.Object) doorwayComponent)
          this.Exit = doorwayProxy;
      }
      Bounds bounds = !((UnityEngine.Object) this.PrefabTile != (UnityEngine.Object) null) || !this.PrefabTile.OverrideAutomaticTileBounds ? UnityUtil.CalculateProxyBounds(this.Prefab, ignoreSpriteRendererBounds, upVector) : this.PrefabTile.TileBoundsOverride;
      if ((double) bounds.size.x <= 0.0 || (double) bounds.size.y <= 0.0 || (double) bounds.size.z <= 0.0)
        Debug.LogError((object) string.Format("Tile prefab '{0}' has automatic bounds that are zero or negative in size. The bounding volume for this tile will need to be manually defined.", (object) prefab), (UnityEngine.Object) prefab);
      this.Placement.LocalBounds = UnityUtil.CondenseBounds(bounds, (IEnumerable<Doorway>) this.Prefab.GetComponentsInChildren<Doorway>());
    }

    public void PositionBySocket(DoorwayProxy myDoorway, DoorwayProxy otherDoorway)
    {
      this.Placement.Rotation = Quaternion.LookRotation(-otherDoorway.Forward, otherDoorway.Up) * Quaternion.Inverse(Quaternion.Inverse(this.Placement.Rotation) * (this.Placement.Rotation * myDoorway.LocalRotation));
      this.Placement.Position = otherDoorway.Position - (myDoorway.Position - this.Placement.Position);
    }

    private Vector3 CalculateOverlap(TileProxy other)
    {
      Bounds bounds1 = this.Placement.Bounds;
      Bounds bounds2 = other.Placement.Bounds;
      double a1 = (double) bounds1.max.x - (double) bounds2.min.x;
      float num = bounds2.max.x - bounds1.min.x;
      float a2 = bounds1.max.y - bounds2.min.y;
      float b1 = bounds2.max.y - bounds1.min.y;
      float a3 = bounds1.max.z - bounds2.min.z;
      float b2 = bounds2.max.z - bounds1.min.z;
      double b3 = (double) num;
      return new Vector3(Mathf.Min((float) a1, (float) b3), Mathf.Min(a2, b1), Mathf.Min(a3, b2));
    }

    public bool IsOverlapping(TileProxy other, float maxOverlap)
    {
      Vector3 overlap = this.CalculateOverlap(other);
      return (double) Mathf.Min(overlap.x, overlap.y, overlap.z) > (double) maxOverlap;
    }

    public bool IsOverlappingOrOverhanging(
      TileProxy other,
      AxisDirection upDirection,
      float maxOverlap)
    {
      Vector3 perAxisOverlap = UnityUtil.CalculatePerAxisOverlap(other.Placement.Bounds, this.Placement.Bounds);
      float num;
      switch (upDirection)
      {
        case AxisDirection.PosX:
        case AxisDirection.NegX:
          num = Mathf.Min(perAxisOverlap.y, perAxisOverlap.z);
          break;
        case AxisDirection.PosY:
        case AxisDirection.NegY:
          num = Mathf.Min(perAxisOverlap.x, perAxisOverlap.z);
          break;
        case AxisDirection.PosZ:
        case AxisDirection.NegZ:
          num = Mathf.Min(perAxisOverlap.x, perAxisOverlap.y);
          break;
        default:
          throw new NotImplementedException("AxisDirection '" + upDirection.ToString() + "' is not implemented");
      }
      return (double) num > (double) maxOverlap;
    }
  }
}
