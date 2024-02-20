// Decompiled with JetBrains decompiler
// Type: DunGen.Tile
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Tags;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Serialization;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Tile")]
  public class Tile : MonoBehaviour, ISerializationCallbackReceiver
  {
    public const int CurrentFileVersion = 1;
    [SerializeField]
    [FormerlySerializedAs("AllowImmediateRepeats")]
    private bool allowImmediateRepeats = true;
    public bool AllowRotation = true;
    public TileRepeatMode RepeatMode;
    public bool OverrideAutomaticTileBounds;
    public Bounds TileBoundsOverride = new Bounds(Vector3.zero, Vector3.one);
    public Doorway Entrance;
    public Doorway Exit;
    public bool OverrideConnectionChance;
    public float ConnectionChance;
    public TagContainer Tags = new TagContainer();
    public List<Doorway> AllDoorways = new List<Doorway>();
    public List<Doorway> UsedDoorways = new List<Doorway>();
    public List<Doorway> UnusedDoorways = new List<Doorway>();
    [SerializeField]
    private TilePlacementData placement;
    [SerializeField]
    private int fileVersion;

    [HideInInspector]
    public Bounds Bounds => this.transform.TransformBounds(this.Placement.LocalBounds);

    public TilePlacementData Placement
    {
      get => this.placement;
      internal set => this.placement = value;
    }

    public Dungeon Dungeon { get; internal set; }

    internal void AddTriggerVolume()
    {
      BoxCollider boxCollider = this.gameObject.AddComponent<BoxCollider>();
      boxCollider.center = this.Placement.LocalBounds.center;
      boxCollider.size = this.Placement.LocalBounds.size;
      boxCollider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
      if ((UnityEngine.Object) other == (UnityEngine.Object) null)
        return;
      DungenCharacter component = other.gameObject.GetComponent<DungenCharacter>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.OnTileEntered(this);
    }

    private void OnTriggerExit(Collider other)
    {
      if ((UnityEngine.Object) other == (UnityEngine.Object) null)
        return;
      DungenCharacter component = other.gameObject.GetComponent<DungenCharacter>();
      if (!((UnityEngine.Object) component != (UnityEngine.Object) null))
        return;
      component.OnTileExited(this);
    }

    private void OnDrawGizmosSelected()
    {
      Gizmos.color = Color.red;
      Bounds? nullable = new Bounds?();
      if (this.OverrideAutomaticTileBounds)
        nullable = new Bounds?(this.transform.TransformBounds(this.TileBoundsOverride));
      else if (this.placement != null)
        nullable = new Bounds?(this.Bounds);
      if (!nullable.HasValue)
        return;
      Bounds bounds = nullable.Value;
      Vector3 center = bounds.center;
      bounds = nullable.Value;
      Vector3 size = bounds.size;
      Gizmos.DrawWireCube(center, size);
    }

    public IEnumerable<Tile> GetAdjactedTiles()
    {
      return this.UsedDoorways.Select<Doorway, Tile>((Func<Doorway, Tile>) (x => x.ConnectedDoorway.Tile)).Distinct<Tile>();
    }

    public bool IsAdjacentTo(Tile other)
    {
      foreach (Doorway usedDoorway in this.UsedDoorways)
      {
        if ((UnityEngine.Object) usedDoorway.ConnectedDoorway.Tile == (UnityEngine.Object) other)
          return true;
      }
      return false;
    }

    public Doorway GetEntranceDoorway()
    {
      foreach (Doorway usedDoorway in this.UsedDoorways)
      {
        Tile tile = usedDoorway.ConnectedDoorway.Tile;
        if (this.Placement.IsOnMainPath)
        {
          if (tile.Placement.IsOnMainPath && this.Placement.PathDepth > tile.Placement.PathDepth)
            return usedDoorway;
        }
        else if (tile.Placement.IsOnMainPath || this.Placement.Depth > tile.Placement.Depth)
          return usedDoorway;
      }
      return (Doorway) null;
    }

    public Doorway GetExitDoorway()
    {
      foreach (Doorway usedDoorway in this.UsedDoorways)
      {
        Tile tile = usedDoorway.ConnectedDoorway.Tile;
        if (this.Placement.IsOnMainPath)
        {
          if (tile.Placement.IsOnMainPath && this.Placement.PathDepth < tile.Placement.PathDepth)
            return usedDoorway;
        }
        else if (!tile.Placement.IsOnMainPath && this.Placement.Depth < tile.Placement.Depth)
          return usedDoorway;
      }
      return (Doorway) null;
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
