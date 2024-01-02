// Decompiled with JetBrains decompiler
// Type: DunGen.Editor.RuntimeAnalyzer
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using DunGen.Analysis;
using DunGen.Graph;
using System.Diagnostics;
using System.Text;
using UnityEngine;

#nullable disable
namespace DunGen.Editor
{
  [AddComponentMenu("DunGen/Analysis/Runtime Analyzer")]
  public sealed class RuntimeAnalyzer : MonoBehaviour
  {
    public DungeonFlow DungeonFlow;
    public int Iterations = 100;
    public int MaxFailedAttempts = 20;
    public bool RunOnStart = true;
    public float MaximumAnalysisTime;
    private DungeonGenerator generator = new DungeonGenerator();
    private GenerationAnalysis analysis;
    private StringBuilder infoText = new StringBuilder();
    private bool finishedEarly;
    private bool prevShouldRandomizeSeed;
    private int targetIterations;
    private int remainingIterations;
    private Stopwatch analysisTime;
    private bool generateNextFrame;

    private int currentIterations => this.targetIterations - this.remainingIterations;

    private void Start()
    {
      if (!this.RunOnStart)
        return;
      this.Analyze();
    }

    public void Analyze()
    {
      bool flag = false;
      if ((Object) this.DungeonFlow == (Object) null)
        UnityEngine.Debug.LogError((object) "No DungeonFlow assigned to analyzer");
      else if (this.Iterations <= 0)
        UnityEngine.Debug.LogError((object) "Iteration count must be greater than 0");
      else if (this.MaxFailedAttempts <= 0)
        UnityEngine.Debug.LogError((object) "Max failed attempt count must be greater than 0");
      else
        flag = true;
      if (!flag)
        return;
      this.prevShouldRandomizeSeed = this.generator.ShouldRandomizeSeed;
      this.generator.IsAnalysis = true;
      this.generator.DungeonFlow = this.DungeonFlow;
      this.generator.MaxAttemptCount = this.MaxFailedAttempts;
      this.generator.ShouldRandomizeSeed = true;
      this.analysis = new GenerationAnalysis(this.Iterations);
      this.analysisTime = Stopwatch.StartNew();
      this.remainingIterations = this.targetIterations = this.Iterations;
      this.generator.OnGenerationStatusChanged += new GenerationStatusDelegate(this.OnGenerationStatusChanged);
      this.generator.Generate();
    }

    private void Update()
    {
      if ((double) this.MaximumAnalysisTime > 0.0 && this.analysisTime.Elapsed.TotalSeconds >= (double) this.MaximumAnalysisTime)
      {
        this.remainingIterations = 0;
        this.finishedEarly = true;
      }
      if (!this.generateNextFrame)
        return;
      this.generateNextFrame = false;
      this.generator.Generate();
    }

    private void CompleteAnalysis()
    {
      this.analysisTime.Stop();
      this.analysis.Analyze();
      UnityUtil.Destroy((Object) this.generator.Root);
      this.OnAnalysisComplete();
    }

    private void OnGenerationStatusChanged(DungeonGenerator generator, GenerationStatus status)
    {
      if (status != GenerationStatus.Complete)
        return;
      this.analysis.IncrementSuccessCount();
      this.analysis.Add(generator.GenerationStats);
      --this.remainingIterations;
      if (this.remainingIterations <= 0)
      {
        generator.OnGenerationStatusChanged -= new GenerationStatusDelegate(this.OnGenerationStatusChanged);
        this.CompleteAnalysis();
      }
      else
        this.generateNextFrame = true;
    }

    private void OnAnalysisComplete()
    {
      this.generator.ShouldRandomizeSeed = this.prevShouldRandomizeSeed;
      this.infoText.Length = 0;
      if (this.finishedEarly)
        this.infoText.AppendLine("[ Reached maximum analysis time before the target number of iterations was reached ]");
      this.infoText.AppendFormat("Iterations: {0}, Max Failed Attempts: {1}", (object) (this.finishedEarly ? this.analysis.IterationCount : this.analysis.TargetIterationCount), (object) this.MaxFailedAttempts);
      this.infoText.AppendFormat("\nTotal Analysis Time: {0:0.00} seconds", (object) this.analysisTime.Elapsed.TotalSeconds);
      this.infoText.AppendFormat("\nDungeons successfully generated: {0}% ({1} failed)", (object) Mathf.RoundToInt(this.analysis.SuccessPercentage), (object) (this.analysis.TargetIterationCount - this.analysis.SuccessCount));
      this.infoText.AppendLine();
      this.infoText.AppendLine();
      this.infoText.Append("## TIME TAKEN (in milliseconds) ##");
      this.infoText.AppendFormat("\n\tPre-Processing:\t\t\t\t\t{0}", (object) this.analysis.PreProcessTime);
      this.infoText.AppendFormat("\n\tMain Path Generation:\t\t{0}", (object) this.analysis.MainPathGenerationTime);
      this.infoText.AppendFormat("\n\tBranch Path Generation:\t\t{0}", (object) this.analysis.BranchPathGenerationTime);
      this.infoText.AppendFormat("\n\tPost-Processing:\t\t\t\t{0}", (object) this.analysis.PostProcessTime);
      this.infoText.Append("\n\t-------------------------------------------------------");
      this.infoText.AppendFormat("\n\tTotal:\t\t\t\t\t\t\t\t{0}", (object) this.analysis.TotalTime);
      this.infoText.AppendLine();
      this.infoText.AppendLine();
      this.infoText.AppendLine("## ROOM DATA ##");
      this.infoText.AppendFormat("\n\tMain Path Rooms: {0}", (object) this.analysis.MainPathRoomCount);
      this.infoText.AppendFormat("\n\tBranch Path Rooms: {0}", (object) this.analysis.BranchPathRoomCount);
      this.infoText.Append("\n\t-------------------");
      this.infoText.AppendFormat("\n\tTotal: {0}", (object) this.analysis.TotalRoomCount);
      this.infoText.AppendLine();
      this.infoText.AppendLine();
      this.infoText.AppendFormat("Retry Count: {0}", (object) this.analysis.TotalRetries);
    }

    private void OnGUI()
    {
      if (this.analysis == null || this.infoText == null || this.infoText.Length == 0)
        GUILayout.Label(string.Format("Analysing... {0} / {1} ({2:0.0}%){3}", (object) this.currentIterations, (object) this.targetIterations, (object) (float) ((double) this.currentIterations / (double) this.targetIterations * 100.0), (object) (this.analysis.SuccessCount < this.analysis.IterationCount ? "\nFailed Dungeons: " + (this.analysis.IterationCount - this.analysis.SuccessCount).ToString() : "")));
      else
        GUILayout.Label(this.infoText.ToString());
    }
  }
}
