// Decompiled with JetBrains decompiler
// Type: DunGen.DoorwayPair
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public struct DoorwayPair
  {
    public TileProxy PreviousTile { get; private set; }

    public DoorwayProxy PreviousDoorway { get; private set; }

    public TileProxy NextTemplate { get; private set; }

    public DoorwayProxy NextDoorway { get; private set; }

    public TileSet NextTileSet { get; private set; }

    public float TileWeight { get; private set; }

    public float DoorwayWeight { get; private set; }

    public DoorwayPair(
      TileProxy previousTile,
      DoorwayProxy previousDoorway,
      TileProxy nextTemplate,
      DoorwayProxy nextDoorway,
      TileSet nextTileSet,
      float tileWeight,
      float doorwayWeight)
    {
      this.PreviousTile = previousTile;
      this.PreviousDoorway = previousDoorway;
      this.NextTemplate = nextTemplate;
      this.NextDoorway = nextDoorway;
      this.NextTileSet = nextTileSet;
      this.TileWeight = tileWeight;
      this.DoorwayWeight = doorwayWeight;
    }
  }
}
