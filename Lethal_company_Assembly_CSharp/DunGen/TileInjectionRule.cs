// Decompiled with JetBrains decompiler
// Type: DunGen.TileInjectionRule
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  [Serializable]
  public sealed class TileInjectionRule
  {
    public TileSet TileSet;
    public FloatRange NormalizedPathDepth = new FloatRange(0.0f, 1f);
    public FloatRange NormalizedBranchDepth = new FloatRange(0.0f, 1f);
    public bool CanAppearOnMainPath = true;
    public bool CanAppearOnBranchPath;
    public bool IsRequired;
    public bool IsLocked;
    public int LockID;
  }
}
