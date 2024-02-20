// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningGeneratorPath
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningGeneratorPath : LightningGenerator
  {
    public static readonly LightningGeneratorPath PathGeneratorInstance = new LightningGeneratorPath();

    public void GenerateLightningBoltPath(
      LightningBolt bolt,
      Vector3 start,
      Vector3 end,
      LightningBoltParameters parameters)
    {
      if (parameters.Points.Count < 2)
      {
        Debug.LogError((object) "Lightning path should have at least two points");
      }
      else
      {
        int generations = parameters.Generations;
        int totalGenerations = generations;
        float num1 = generations == parameters.Generations ? parameters.ChaosFactor : parameters.ChaosFactorForks;
        int num2 = parameters.SmoothingFactor - 1;
        LightningBoltSegmentGroup boltSegmentGroup1 = bolt.AddGroup();
        boltSegmentGroup1.LineWidth = parameters.TrunkWidth;
        LightningBoltSegmentGroup boltSegmentGroup2 = boltSegmentGroup1;
        int num3 = generations;
        int generation = num3 - 1;
        boltSegmentGroup2.Generation = num3;
        boltSegmentGroup1.EndWidthMultiplier = parameters.EndWidthMultiplier;
        boltSegmentGroup1.Color = parameters.Color;
        if (generation == parameters.Generations && (parameters.MainTrunkTintColor.r != byte.MaxValue || parameters.MainTrunkTintColor.g != byte.MaxValue || parameters.MainTrunkTintColor.b != byte.MaxValue || parameters.MainTrunkTintColor.a != byte.MaxValue))
        {
          boltSegmentGroup1.Color.r = (byte) (0.0039215688593685627 * (double) boltSegmentGroup1.Color.r * (double) parameters.MainTrunkTintColor.r);
          boltSegmentGroup1.Color.g = (byte) (0.0039215688593685627 * (double) boltSegmentGroup1.Color.g * (double) parameters.MainTrunkTintColor.g);
          boltSegmentGroup1.Color.b = (byte) (0.0039215688593685627 * (double) boltSegmentGroup1.Color.b * (double) parameters.MainTrunkTintColor.b);
          boltSegmentGroup1.Color.a = (byte) (0.0039215688593685627 * (double) boltSegmentGroup1.Color.a * (double) parameters.MainTrunkTintColor.a);
        }
        parameters.Start = parameters.Points[0] + start;
        parameters.End = parameters.Points[parameters.Points.Count - 1] + end;
        end = parameters.Start;
        for (int index = 1; index < parameters.Points.Count; ++index)
        {
          start = end;
          end = parameters.Points[index];
          Vector3 vector3_1 = end - start;
          float num4 = PathGenerator.SquareRoot(vector3_1.sqrMagnitude);
          if ((double) num1 > 0.0)
          {
            if (bolt.CameraMode == CameraMode.Perspective)
              end += num4 * num1 * this.RandomDirection3D(parameters.Random);
            else if (bolt.CameraMode == CameraMode.OrthographicXY)
              end += num4 * num1 * this.RandomDirection2D(parameters.Random);
            else
              end += num4 * num1 * this.RandomDirection2DXZ(parameters.Random);
            vector3_1 = end - start;
          }
          boltSegmentGroup1.Segments.Add(new LightningBoltSegment()
          {
            Start = start,
            End = end
          });
          float offsetAmount = num4 * num1;
          Vector3 result;
          this.RandomVector(bolt, ref start, ref end, offsetAmount, parameters.Random, out result);
          if (this.ShouldCreateFork(parameters, generation, totalGenerations))
          {
            Vector3 vector3_2 = vector3_1 * parameters.ForkMultiplier() * (float) num2 * 0.5f;
            Vector3 end1 = end + vector3_2 + result;
            this.GenerateLightningBoltStandard(bolt, start, end1, generation, totalGenerations, 0.0f, parameters);
          }
          if (--num2 == 0)
            num2 = parameters.SmoothingFactor - 1;
        }
      }
    }

    protected override void OnGenerateLightningBolt(
      LightningBolt bolt,
      Vector3 start,
      Vector3 end,
      LightningBoltParameters parameters)
    {
      this.GenerateLightningBoltPath(bolt, start, end, parameters);
    }
  }
}
