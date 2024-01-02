﻿// Decompiled with JetBrains decompiler
// Type: DunGen.Adapters.NavMeshAdapter
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

#nullable disable
namespace DunGen.Adapters
{
  public abstract class NavMeshAdapter : BaseAdapter
  {
    public NavMeshAdapter.OnNavMeshGenerationProgress OnProgress;

    protected override void Run(DungeonGenerator generator)
    {
      this.Generate(generator.CurrentDungeon);
    }

    public abstract void Generate(Dungeon dungeon);

    public struct NavMeshGenerationProgress
    {
      public float Percentage;
      public string Description;
    }

    public delegate void OnNavMeshGenerationProgress(
      NavMeshAdapter.NavMeshGenerationProgress progress);
  }
}
