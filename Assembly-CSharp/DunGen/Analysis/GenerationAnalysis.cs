// Decompiled with JetBrains decompiler
// Type: DunGen.Analysis.GenerationAnalysis
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using System.Linq;

#nullable disable
namespace DunGen.Analysis
{
  public class GenerationAnalysis
  {
    private readonly List<GenerationStats> statsSet = new List<GenerationStats>();

    public int TargetIterationCount { get; private set; }

    public int IterationCount { get; private set; }

    public NumberSetData MainPathRoomCount { get; private set; }

    public NumberSetData BranchPathRoomCount { get; private set; }

    public NumberSetData TotalRoomCount { get; private set; }

    public NumberSetData MaxBranchDepth { get; private set; }

    public NumberSetData TotalRetries { get; private set; }

    public NumberSetData PreProcessTime { get; private set; }

    public NumberSetData MainPathGenerationTime { get; private set; }

    public NumberSetData BranchPathGenerationTime { get; private set; }

    public NumberSetData PostProcessTime { get; private set; }

    public NumberSetData TotalTime { get; private set; }

    public float AnalysisTime { get; private set; }

    public int SuccessCount { get; private set; }

    public float SuccessPercentage
    {
      get => (float) ((double) this.SuccessCount / (double) this.TargetIterationCount * 100.0);
    }

    public GenerationAnalysis(int targetIterationCount)
    {
      this.TargetIterationCount = targetIterationCount;
    }

    public void Clear()
    {
      this.IterationCount = 0;
      this.AnalysisTime = 0.0f;
      this.SuccessCount = 0;
      this.statsSet.Clear();
    }

    public void Add(GenerationStats stats)
    {
      this.statsSet.Add(stats.Clone());
      this.AnalysisTime += stats.TotalTime;
      ++this.IterationCount;
    }

    public void IncrementSuccessCount() => ++this.SuccessCount;

    public void Analyze()
    {
      this.MainPathRoomCount = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => (float) x.MainPathRoomCount)));
      this.BranchPathRoomCount = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => (float) x.BranchPathRoomCount)));
      this.TotalRoomCount = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => (float) x.TotalRoomCount)));
      this.MaxBranchDepth = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => (float) x.MaxBranchDepth)));
      this.TotalRetries = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => (float) x.TotalRetries)));
      this.PreProcessTime = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => x.PreProcessTime)));
      this.MainPathGenerationTime = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => x.MainPathGenerationTime)));
      this.BranchPathGenerationTime = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => x.BranchPathGenerationTime)));
      this.PostProcessTime = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => x.PostProcessTime)));
      this.TotalTime = new NumberSetData(this.statsSet.Select<GenerationStats, float>((Func<GenerationStats, float>) (x => x.TotalTime)));
    }
  }
}
