// Decompiled with JetBrains decompiler
// Type: DunGen.DungenCharacter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [AddComponentMenu("DunGen/Character")]
  public class DungenCharacter : MonoBehaviour
  {
    private static readonly List<DungenCharacter> allCharacters = new List<DungenCharacter>();
    private List<Tile> overlappingTiles;

    public static event DungenCharacterDelegate CharacterAdded;

    public static event DungenCharacterDelegate CharacterRemoved;

    public static ReadOnlyCollection<DungenCharacter> AllCharacters { get; private set; }

    static DungenCharacter()
    {
      DungenCharacter.AllCharacters = new ReadOnlyCollection<DungenCharacter>((IList<DungenCharacter>) DungenCharacter.allCharacters);
    }

    public Tile CurrentTile
    {
      get
      {
        return this.overlappingTiles == null || this.overlappingTiles.Count == 0 ? (Tile) null : this.overlappingTiles[this.overlappingTiles.Count - 1];
      }
    }

    public event CharacterTileChangedEvent OnTileChanged;

    protected virtual void OnEnable()
    {
      if (this.overlappingTiles == null)
        this.overlappingTiles = new List<Tile>();
      DungenCharacter.allCharacters.Add(this);
      if (DungenCharacter.CharacterAdded == null)
        return;
      DungenCharacter.CharacterAdded(this);
    }

    protected virtual void OnDisable()
    {
      DungenCharacter.allCharacters.Remove(this);
      if (DungenCharacter.CharacterRemoved == null)
        return;
      DungenCharacter.CharacterRemoved(this);
    }

    internal void ForceRecheckTile()
    {
      this.overlappingTiles.Clear();
      foreach (Tile tile in Object.FindObjectsOfType<Tile>())
      {
        if (tile.Placement.Bounds.Contains(this.transform.position))
        {
          this.OnTileEntered(tile);
          break;
        }
      }
    }

    protected virtual void OnTileChangedEvent(Tile previousTile, Tile newTile)
    {
    }

    internal void OnTileEntered(Tile tile)
    {
      if (this.overlappingTiles.Contains(tile))
        return;
      Tile currentTile = this.CurrentTile;
      this.overlappingTiles.Add(tile);
      if (!((Object) this.CurrentTile != (Object) currentTile))
        return;
      CharacterTileChangedEvent onTileChanged = this.OnTileChanged;
      if (onTileChanged != null)
        onTileChanged(this, currentTile, this.CurrentTile);
      this.OnTileChangedEvent(currentTile, this.CurrentTile);
    }

    internal void OnTileExited(Tile tile)
    {
      if (!this.overlappingTiles.Contains(tile))
        return;
      Tile currentTile = this.CurrentTile;
      this.overlappingTiles.Remove(tile);
      if (!((Object) this.CurrentTile != (Object) currentTile))
        return;
      CharacterTileChangedEvent onTileChanged = this.OnTileChanged;
      if (onTileChanged != null)
        onTileChanged(this, currentTile, this.CurrentTile);
      this.OnTileChangedEvent(currentTile, this.CurrentTile);
    }
  }
}
