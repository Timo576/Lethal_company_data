// Decompiled with JetBrains decompiler
// Type: DunGen.GenerationStats
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Diagnostics;

#nullable disable
namespace DunGen
{
  public sealed class GenerationStats
  {
    private Stopwatch stopwatch = new Stopwatch();
    private GenerationStatus generationStatus;

    public int MainPathRoomCount { get; private set; }

    public int BranchPathRoomCount { get; private set; }

    public int TotalRoomCount { get; private set; }

    public int MaxBranchDepth { get; private set; }

    public int TotalRetries { get; private set; }

    public int PrunedBranchTileCount { get; internal set; }

    public float PreProcessTime { get; private set; }

    public float MainPathGenerationTime { get; private set; }

    public float BranchPathGenerationTime { get; private set; }

    public float PostProcessTime { get; private set; }

    public float TotalTime { get; private set; }

    internal void Clear()
    {
      this.MainPathRoomCount = 0;
      this.BranchPathRoomCount = 0;
      this.TotalRoomCount = 0;
      this.MaxBranchDepth = 0;
      this.TotalRetries = 0;
      this.PrunedBranchTileCount = 0;
      this.PreProcessTime = 0.0f;
      this.MainPathGenerationTime = 0.0f;
      this.BranchPathGenerationTime = 0.0f;
      this.PostProcessTime = 0.0f;
      this.TotalTime = 0.0f;
    }

    internal void IncrementRetryCount() => ++this.TotalRetries;

    internal void SetRoomStatistics(
      int mainPathRoomCount,
      int branchPathRoomCount,
      int maxBranchDepth)
    {
      this.MainPathRoomCount = mainPathRoomCount;
      this.BranchPathRoomCount = branchPathRoomCount;
      this.MaxBranchDepth = maxBranchDepth;
      this.TotalRoomCount = this.MainPathRoomCount + this.BranchPathRoomCount;
    }

    internal void BeginTime(GenerationStatus status)
    {
      if (this.stopwatch.IsRunning)
        this.EndTime();
      this.generationStatus = status;
      this.stopwatch.Reset();
      this.stopwatch.Start();
    }

    internal void EndTime()
    {
      this.stopwatch.Stop();
      float totalMilliseconds = (float) this.stopwatch.Elapsed.TotalMilliseconds;
      switch (this.generationStatus)
      {
        case GenerationStatus.PreProcessing:
          this.PreProcessTime += totalMilliseconds;
          break;
        case GenerationStatus.MainPath:
          this.MainPathGenerationTime += totalMilliseconds;
          break;
        case GenerationStatus.Branching:
          this.BranchPathGenerationTime += totalMilliseconds;
          break;
        case GenerationStatus.PostProcessing:
          this.PostProcessTime += totalMilliseconds;
          break;
      }
      this.TotalTime = this.PreProcessTime + this.MainPathGenerationTime + this.BranchPathGenerationTime + this.PostProcessTime;
    }

    public GenerationStats Clone()
    {
      return new GenerationStats()
      {
        MainPathRoomCount = this.MainPathRoomCount,
        BranchPathRoomCount = this.BranchPathRoomCount,
        TotalRoomCount = this.TotalRoomCount,
        MaxBranchDepth = this.MaxBranchDepth,
        TotalRetries = this.TotalRetries,
        PrunedBranchTileCount = this.PrunedBranchTileCount,
        PreProcessTime = this.PreProcessTime,
        MainPathGenerationTime = this.MainPathGenerationTime,
        BranchPathGenerationTime = this.BranchPathGenerationTime,
        PostProcessTime = this.PostProcessTime,
        TotalTime = this.TotalTime
      };
    }
  }
}
