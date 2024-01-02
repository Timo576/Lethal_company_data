// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltPrefabScriptBase
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public abstract class LightningBoltPrefabScriptBase : LightningBoltScript
  {
    private readonly List<LightningBoltParameters> batchParameters = new List<LightningBoltParameters>();
    private readonly System.Random random = new System.Random();
    [Header("Lightning Spawn Properties")]
    [SingleLineClamp("How long to wait before creating another round of lightning bolts in seconds", 0.001, 1.7976931348623157E+308)]
    public RangeOfFloats IntervalRange = new RangeOfFloats()
    {
      Minimum = 0.05f,
      Maximum = 0.1f
    };
    [SingleLineClamp("How many lightning bolts to emit for each interval", 0.0, 100.0)]
    public RangeOfIntegers CountRange = new RangeOfIntegers()
    {
      Minimum = 1,
      Maximum = 1
    };
    [Tooltip("Reduces the probability that additional bolts from CountRange will actually happen (0 - 1).")]
    [Range(0.0f, 1f)]
    public float CountProbabilityModifier = 1f;
    public RangeOfFloats DelayRange = new RangeOfFloats()
    {
      Minimum = 0.0f,
      Maximum = 0.0f
    };
    [SingleLineClamp("For each bolt emitted, how long should it stay in seconds", 0.01, 10.0)]
    public RangeOfFloats DurationRange = new RangeOfFloats()
    {
      Minimum = 0.06f,
      Maximum = 0.12f
    };
    [Header("Lightning Appearance Properties")]
    [SingleLineClamp("The trunk width range in unity units (x = min, y = max)", 0.0001, 100.0)]
    public RangeOfFloats TrunkWidthRange = new RangeOfFloats()
    {
      Minimum = 0.1f,
      Maximum = 0.2f
    };
    [Tooltip("How long (in seconds) this game object should live before destroying itself. Leave as 0 for infinite.")]
    [Range(0.0f, 1000f)]
    public float LifeTime;
    [Tooltip("Generations (1 - 8, higher makes more detailed but more expensive lightning)")]
    [Range(1f, 8f)]
    public int Generations = 6;
    [Tooltip("The chaos factor that determines how far the lightning main trunk can spread out, higher numbers spread out more. 0 - 1.")]
    [Range(0.0f, 1f)]
    public float ChaosFactor = 0.075f;
    [Tooltip("The chaos factor that determines how far the forks of the lightning can spread out, higher numbers spread out more. 0 - 1.")]
    [Range(0.0f, 1f)]
    public float ChaosFactorForks = 0.095f;
    [Tooltip("Intensity of the lightning")]
    [Range(0.0f, 10f)]
    public float Intensity = 1f;
    [Tooltip("The intensity of the glow")]
    [Range(0.0f, 10f)]
    public float GlowIntensity = 0.1f;
    [Tooltip("The width multiplier for the glow, 0 - 64")]
    [Range(0.0f, 64f)]
    public float GlowWidthMultiplier = 4f;
    [Tooltip("What percent of time the lightning should fade in and out. For example, 0.15 fades in 15% of the time and fades out 15% of the time, with full visibility 70% of the time.")]
    [Range(0.0f, 0.5f)]
    public float FadePercent = 0.15f;
    [Tooltip("Modify the duration of lightning fade in.")]
    [Range(0.0f, 1f)]
    public float FadeInMultiplier = 1f;
    [Tooltip("Modify the duration of fully lit lightning.")]
    [Range(0.0f, 1f)]
    public float FadeFullyLitMultiplier = 1f;
    [Tooltip("Modify the duration of lightning fade out.")]
    [Range(0.0f, 1f)]
    public float FadeOutMultiplier = 1f;
    [Tooltip("0 - 1, how slowly the lightning should grow. 0 for instant, 1 for slow.")]
    [Range(0.0f, 1f)]
    public float GrowthMultiplier;
    [Tooltip("How much smaller the lightning should get as it goes towards the end of the bolt. For example, 0.5 will make the end 50% the width of the start.")]
    [Range(0.0f, 10f)]
    public float EndWidthMultiplier = 0.5f;
    [Tooltip("How forked should the lightning be? (0 - 1, 0 for none, 1 for lots of forks)")]
    [Range(0.0f, 1f)]
    public float Forkedness = 0.25f;
    [Range(0.0f, 10f)]
    [Tooltip("Minimum distance multiplier for forks")]
    public float ForkLengthMultiplier = 0.6f;
    [Range(0.0f, 10f)]
    [Tooltip("Fork distance multiplier variance. Random range of 0 to n that is added to Fork Length Multiplier.")]
    public float ForkLengthVariance = 0.2f;
    [Tooltip("Forks have their EndWidthMultiplier multiplied by this value")]
    [Range(0.0f, 10f)]
    public float ForkEndWidthMultiplier = 1f;
    [Header("Lightning Light Properties")]
    [Tooltip("Light parameters")]
    public LightningLightParameters LightParameters;
    [Tooltip("Maximum number of lights that can be created per batch of lightning")]
    [Range(0.0f, 64f)]
    public int MaximumLightsPerBatch = 8;
    [Header("Lightning Trigger Type")]
    [Tooltip("Manual or automatic mode. Manual requires that you call the Trigger method in script. Automatic uses the interval to create lightning continuously.")]
    public bool ManualMode;
    [Tooltip("Turns lightning into automatic mode for this number of seconds, then puts it into manual mode.")]
    [Range(0.0f, 120f)]
    public float AutomaticModeSeconds;
    [Header("Lightning custom transform handler")]
    [Tooltip("Custom handler to modify the transform of each lightning bolt, useful if it will be alive longer than a few frames and needs to scale and rotate based on the position of other objects.")]
    public LightningCustomTransformDelegate CustomTransformHandler;
    private float nextLightningTimestamp;
    private float lifeTimeRemaining;

    public System.Random RandomOverride { get; set; }

    private void CalculateNextLightningTimestamp(float offset)
    {
      this.nextLightningTimestamp = (double) this.IntervalRange.Minimum == (double) this.IntervalRange.Maximum ? this.IntervalRange.Minimum : offset + this.IntervalRange.Random();
    }

    private void CustomTransform(LightningCustomTransformStateInfo state)
    {
      if (this.CustomTransformHandler == null)
        return;
      this.CustomTransformHandler.Invoke(state);
    }

    private void CallLightning() => this.CallLightning(new Vector3?(), new Vector3?());

    private void CallLightning(Vector3? start, Vector3? end)
    {
      int num = this.CountRange.Random(this.RandomOverride ?? this.random);
      for (int index = 0; index < num; ++index)
      {
        LightningBoltParameters parameters = this.CreateParameters();
        if ((double) this.CountProbabilityModifier >= 0.99989998340606689 || index == 0 || parameters.Random.NextDouble() <= (double) this.CountProbabilityModifier)
        {
          parameters.CustomTransform = this.CustomTransformHandler == null ? (Action<LightningCustomTransformStateInfo>) null : new Action<LightningCustomTransformStateInfo>(this.CustomTransform);
          this.CreateLightningBolt(parameters);
          if (start.HasValue)
            parameters.Start = start.Value;
          if (end.HasValue)
            parameters.End = end.Value;
        }
        else
          LightningBoltParameters.ReturnParametersToCache(parameters);
      }
      this.CreateLightningBoltsNow();
    }

    protected void CreateLightningBoltsNow()
    {
      int maximumLightsPerBatch = LightningBolt.MaximumLightsPerBatch;
      LightningBolt.MaximumLightsPerBatch = this.MaximumLightsPerBatch;
      this.CreateLightningBolts((ICollection<LightningBoltParameters>) this.batchParameters);
      LightningBolt.MaximumLightsPerBatch = maximumLightsPerBatch;
      this.batchParameters.Clear();
    }

    protected override void PopulateParameters(LightningBoltParameters parameters)
    {
      base.PopulateParameters(parameters);
      parameters.RandomOverride = this.RandomOverride;
      float num1 = this.DurationRange.Random(parameters.Random);
      float num2 = this.TrunkWidthRange.Random(parameters.Random);
      parameters.Generations = this.Generations;
      parameters.LifeTime = num1;
      parameters.ChaosFactor = this.ChaosFactor;
      parameters.ChaosFactorForks = this.ChaosFactorForks;
      parameters.TrunkWidth = num2;
      parameters.Intensity = this.Intensity;
      parameters.GlowIntensity = this.GlowIntensity;
      parameters.GlowWidthMultiplier = this.GlowWidthMultiplier;
      parameters.Forkedness = this.Forkedness;
      parameters.ForkLengthMultiplier = this.ForkLengthMultiplier;
      parameters.ForkLengthVariance = this.ForkLengthVariance;
      parameters.FadePercent = this.FadePercent;
      parameters.FadeInMultiplier = this.FadeInMultiplier;
      parameters.FadeOutMultiplier = this.FadeOutMultiplier;
      parameters.FadeFullyLitMultiplier = this.FadeFullyLitMultiplier;
      parameters.GrowthMultiplier = this.GrowthMultiplier;
      parameters.EndWidthMultiplier = this.EndWidthMultiplier;
      parameters.ForkEndWidthMultiplier = this.ForkEndWidthMultiplier;
      parameters.DelayRange = this.DelayRange;
      parameters.LightParameters = this.LightParameters;
    }

    protected override void Start()
    {
      base.Start();
      this.CalculateNextLightningTimestamp(0.0f);
      this.lifeTimeRemaining = (double) this.LifeTime <= 0.0 ? float.MaxValue : this.LifeTime;
    }

    protected override void Update()
    {
      base.Update();
      if ((double) Time.timeScale <= 0.0)
        return;
      if ((double) (this.lifeTimeRemaining -= LightningBoltScript.DeltaTime) < 0.0)
        UnityEngine.Object.Destroy((UnityEngine.Object) this.gameObject);
      if ((double) (this.nextLightningTimestamp -= LightningBoltScript.DeltaTime) <= 0.0)
      {
        this.CalculateNextLightningTimestamp(this.nextLightningTimestamp);
        if (!this.ManualMode)
          this.CallLightning();
      }
      if ((double) this.AutomaticModeSeconds <= 0.0)
        return;
      this.AutomaticModeSeconds = Mathf.Max(0.0f, this.AutomaticModeSeconds - LightningBoltScript.DeltaTime);
      this.ManualMode = (double) this.AutomaticModeSeconds == 0.0;
    }

    protected virtual void OnDrawGizmos()
    {
    }

    public override void CreateLightningBolt(LightningBoltParameters p)
    {
      this.batchParameters.Add(p);
    }

    public void Trigger() => this.Trigger(-1f);

    public void Trigger(float seconds)
    {
      this.CallLightning();
      if ((double) seconds < 0.0)
        return;
      this.AutomaticModeSeconds = Mathf.Max(0.0f, seconds);
    }

    public void Trigger(Vector3? start, Vector3? end) => this.CallLightning(start, end);
  }
}
