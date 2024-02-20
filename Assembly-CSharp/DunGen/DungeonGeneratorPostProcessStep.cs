// Decompiled with JetBrains decompiler
// Type: DunGen.DungeonGeneratorPostProcessStep
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
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
