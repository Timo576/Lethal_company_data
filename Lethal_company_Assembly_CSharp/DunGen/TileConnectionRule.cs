// Decompiled with JetBrains decompiler
// Type: DunGen.TileConnectionRule
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen
{
  public sealed class TileConnectionRule
  {
    public int Priority;
    public TileConnectionRule.CanTilesConnectDelegate Delegate;

    public TileConnectionRule(
      TileConnectionRule.CanTilesConnectDelegate connectionDelegate,
      int priority = 0)
    {
      this.Delegate = connectionDelegate;
      this.Priority = priority;
    }

    public enum ConnectionResult
    {
      Allow,
      Deny,
      Passthrough,
    }

    public delegate TileConnectionRule.ConnectionResult CanTilesConnectDelegate(
      Tile tileA,
      Tile tileB,
      Doorway doorwayA,
      Doorway doorwayB);
  }
}
