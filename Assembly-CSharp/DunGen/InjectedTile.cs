// Decompiled with JetBrains decompiler
// Type: DunGen.InjectedTile
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class InjectedTile
  {
    public TileSet TileSet;
    public float NormalizedPathDepth;
    public float NormalizedBranchDepth;
    public bool IsOnMainPath;
    public bool IsRequired;
    public bool IsLocked;
    public int LockID;
    public GameObject LockedDoorPrefab;

    public InjectedTile(
      TileSet tileSet,
      bool isOnMainPath,
      float normalizedPathDepth,
      float normalizedBranchDepth,
      bool isRequired = false)
    {
      this.TileSet = tileSet;
      this.IsOnMainPath = isOnMainPath;
      this.NormalizedPathDepth = normalizedPathDepth;
      this.NormalizedBranchDepth = normalizedBranchDepth;
      this.IsRequired = isRequired;
    }

    public InjectedTile(TileInjectionRule rule, bool isOnMainPath, RandomStream randomStream)
    {
      this.TileSet = rule.TileSet;
      this.NormalizedPathDepth = rule.NormalizedPathDepth.GetRandom(randomStream);
      this.NormalizedBranchDepth = rule.NormalizedBranchDepth.GetRandom(randomStream);
      this.IsOnMainPath = isOnMainPath;
      this.IsRequired = rule.IsRequired;
      this.IsLocked = rule.IsLocked;
      this.LockID = rule.LockID;
    }

    public bool ShouldInjectTileAtPoint(bool isOnMainPath, float pathDepth, float branchDepth)
    {
      if (this.IsOnMainPath != isOnMainPath || (double) this.NormalizedPathDepth > (double) pathDepth)
        return false;
      return isOnMainPath || (double) this.NormalizedBranchDepth <= (double) branchDepth;
    }
  }
}
