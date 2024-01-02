// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonGeneratorPostProcessStep
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  public struct DungeonGeneratorPostProcessStep
  {
    public Action<DungeonGenerator> PostProcessCallback;
    public PostProcessPhase Phase;
    public int Priority;

    public DungeonGeneratorPostProcessStep(
      Action<DungeonGenerator> postProcessCallback,
      int priority,
      PostProcessPhase phase)
    {
      this.PostProcessCallback = postProcessCallback;
      this.Priority = priority;
      this.Phase = phase;
    }
  }
}
