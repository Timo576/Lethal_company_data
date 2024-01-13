// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningBoltParameters
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  [Serializable]
  public sealed class LightningBoltParameters
  {
    private static int randomSeed = Environment.TickCount;
    private static readonly List<LightningBoltParameters> cache = new List<LightningBoltParameters>();
    internal int generationWhereForksStop;
    internal int forkednessCalculated;
    internal LightningBoltQualitySetting quality;
    internal float delaySeconds;
    internal int maxLights;
    public static float Scale = 1f;
    public static readonly Dictionary<int, LightningQualityMaximum> QualityMaximums = new Dictionary<int, LightningQualityMaximum>();
    public LightningGenerator Generator;
    public Vector3 Start;
    public Vector3 End;
    public Vector3 StartVariance;
    public Vector3 EndVariance;
    public Action<LightningCustomTransformStateInfo> CustomTransform;
    private int generations;
    public float LifeTime;
    public float Delay;
    public RangeOfFloats DelayRange;
    public float ChaosFactor;
    public float ChaosFactorForks = -1f;
    public float TrunkWidth;
    public float EndWidthMultiplier = 0.5f;
    public float Intensity = 1f;
    public float GlowIntensity;
    public float GlowWidthMultiplier;
    public float Forkedness;
    public int GenerationWhereForksStopSubtractor = 5;
    public Color32 Color = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    public Color32 MainTrunkTintColor = new Color32(byte.MaxValue, byte.MaxValue, byte.MaxValue, byte.MaxValue);
    private System.Random random;
    private System.Random currentRandom;
    private System.Random randomOverride;
    public float FadePercent = 0.15f;
    public float FadeInMultiplier = 1f;
    public float FadeFullyLitMultiplier = 1f;
    public float FadeOutMultiplier = 1f;
    private float growthMultiplier;
    public float ForkLengthMultiplier = 0.6f;
    public float ForkLengthVariance = 0.2f;
    public float ForkEndWidthMultiplier = 1f;
    public LightningLightParameters LightParameters;
    public int SmoothingFactor;

    static LightningBoltParameters()
    {
      string[] names = QualitySettings.names;
      for (int key = 0; key < names.Length; ++key)
      {
        switch (key)
        {
          case 0:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 3,
              MaximumLightPercent = 0.0f,
              MaximumShadowPercent = 0.0f
            };
            break;
          case 1:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 4,
              MaximumLightPercent = 0.0f,
              MaximumShadowPercent = 0.0f
            };
            break;
          case 2:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 5,
              MaximumLightPercent = 0.1f,
              MaximumShadowPercent = 0.0f
            };
            break;
          case 3:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 5,
              MaximumLightPercent = 0.1f,
              MaximumShadowPercent = 0.0f
            };
            break;
          case 4:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 6,
              MaximumLightPercent = 0.05f,
              MaximumShadowPercent = 0.1f
            };
            break;
          case 5:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 7,
              MaximumLightPercent = 0.025f,
              MaximumShadowPercent = 0.05f
            };
            break;
          default:
            LightningBoltParameters.QualityMaximums[key] = new LightningQualityMaximum()
            {
              MaximumGenerations = 8,
              MaximumLightPercent = 0.025f,
              MaximumShadowPercent = 0.05f
            };
            break;
        }
      }
    }

    public LightningBoltParameters()
    {
      this.random = this.currentRandom = new System.Random(LightningBoltParameters.randomSeed++);
      this.Points = new List<Vector3>();
    }

    public int Generations
    {
      get => this.generations;
      set
      {
        int b = Mathf.Clamp(value, 1, 8);
        if (this.quality == LightningBoltQualitySetting.UseScript)
        {
          this.generations = b;
        }
        else
        {
          int qualityLevel = QualitySettings.GetQualityLevel();
          LightningQualityMaximum lightningQualityMaximum;
          if (LightningBoltParameters.QualityMaximums.TryGetValue(qualityLevel, out lightningQualityMaximum))
          {
            this.generations = Mathf.Min(lightningQualityMaximum.MaximumGenerations, b);
          }
          else
          {
            this.generations = b;
            Debug.LogError((object) ("Unable to read lightning quality settings from level " + qualityLevel.ToString()));
          }
        }
      }
    }

    public System.Random Random
    {
      get => this.currentRandom;
      set
      {
        this.random = value ?? this.random;
        this.currentRandom = this.randomOverride ?? this.random;
      }
    }

    public System.Random RandomOverride
    {
      get => this.randomOverride;
      set
      {
        this.randomOverride = value;
        this.currentRandom = this.randomOverride ?? this.random;
      }
    }

    public float GrowthMultiplier
    {
      get => this.growthMultiplier;
      set => this.growthMultiplier = Mathf.Clamp(value, 0.0f, 0.999f);
    }

    public List<Vector3> Points { get; set; }

    public float ForkMultiplier()
    {
      return (float) this.Random.NextDouble() * this.ForkLengthVariance + this.ForkLengthMultiplier;
    }

    public Vector3 ApplyVariance(Vector3 pos, Vector3 variance)
    {
      return new Vector3(pos.x + (float) (this.Random.NextDouble() * 2.0 - 1.0) * variance.x, pos.y + (float) (this.Random.NextDouble() * 2.0 - 1.0) * variance.y, pos.z + (float) (this.Random.NextDouble() * 2.0 - 1.0) * variance.z);
    }

    public void Reset()
    {
      this.Start = this.End = Vector3.zero;
      this.Generator = (LightningGenerator) null;
      this.SmoothingFactor = 0;
      this.RandomOverride = (System.Random) null;
      this.CustomTransform = (Action<LightningCustomTransformStateInfo>) null;
      if (this.Points == null)
        return;
      this.Points.Clear();
    }

    public static LightningBoltParameters GetOrCreateParameters()
    {
      LightningBoltParameters parameters;
      if (LightningBoltParameters.cache.Count == 0)
      {
        parameters = new LightningBoltParameters();
      }
      else
      {
        int index = LightningBoltParameters.cache.Count - 1;
        parameters = LightningBoltParameters.cache[index];
        LightningBoltParameters.cache.RemoveAt(index);
      }
      return parameters;
    }

    public static void ReturnParametersToCache(LightningBoltParameters p)
    {
      if (LightningBoltParameters.cache.Contains(p))
        return;
      p.Reset();
      LightningBoltParameters.cache.Add(p);
    }
  }
}
