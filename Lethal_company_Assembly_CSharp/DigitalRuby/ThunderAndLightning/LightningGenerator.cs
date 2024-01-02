// Decompiled with JetBrains decompiler
// Type: DigitalRuby.ThunderAndLightning.LightningGenerator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections.Generic;
using UnityEngine;

#nullable disable
namespace DigitalRuby.ThunderAndLightning
{
  public class LightningGenerator
  {
    internal const float oneOver255 = 0.003921569f;
    internal const float mainTrunkMultiplier = 0.003921569f;
    public static readonly LightningGenerator GeneratorInstance = new LightningGenerator();

    private void GetPerpendicularVector(ref Vector3 directionNormalized, out Vector3 side)
    {
      if (directionNormalized == Vector3.zero)
      {
        side = Vector3.right;
      }
      else
      {
        float x1 = directionNormalized.x;
        float y1 = directionNormalized.y;
        float z1 = directionNormalized.z;
        double num1 = (double) Mathf.Abs(x1);
        float num2 = Mathf.Abs(y1);
        float num3 = Mathf.Abs(z1);
        double num4 = (double) num2;
        float y2;
        float z2;
        float x2;
        if (num1 >= num4 && (double) num2 >= (double) num3)
        {
          y2 = 1f;
          z2 = 1f;
          x2 = (float) -((double) y1 * (double) y2 + (double) z1 * (double) z2) / x1;
        }
        else if ((double) num2 >= (double) num3)
        {
          x2 = 1f;
          z2 = 1f;
          y2 = (float) -((double) x1 * (double) x2 + (double) z1 * (double) z2) / y1;
        }
        else
        {
          x2 = 1f;
          y2 = 1f;
          z2 = (float) -((double) x1 * (double) x2 + (double) y1 * (double) y2) / z1;
        }
        side = new Vector3(x2, y2, z2).normalized;
      }
    }

    protected virtual void OnGenerateLightningBolt(
      LightningBolt bolt,
      Vector3 start,
      Vector3 end,
      LightningBoltParameters parameters)
    {
      this.GenerateLightningBoltStandard(bolt, start, end, parameters.Generations, parameters.Generations, 0.0f, parameters);
    }

    public bool ShouldCreateFork(
      LightningBoltParameters parameters,
      int generation,
      int totalGenerations)
    {
      return generation > parameters.generationWhereForksStop && generation >= totalGenerations - parameters.forkednessCalculated && parameters.Random.NextDouble() < (double) parameters.Forkedness;
    }

    public void CreateFork(
      LightningBolt bolt,
      LightningBoltParameters parameters,
      int generation,
      int totalGenerations,
      Vector3 start,
      Vector3 midPoint)
    {
      if (!this.ShouldCreateFork(parameters, generation, totalGenerations))
        return;
      Vector3 vector3 = (midPoint - start) * parameters.ForkMultiplier();
      Vector3 end = midPoint + vector3;
      this.GenerateLightningBoltStandard(bolt, midPoint, end, generation, totalGenerations, 0.0f, parameters);
    }

    public void GenerateLightningBoltStandard(
      LightningBolt bolt,
      Vector3 start,
      Vector3 end,
      int generation,
      int totalGenerations,
      float offsetAmount,
      LightningBoltParameters parameters)
    {
      if (generation < 1)
        return;
      LightningBoltSegmentGroup boltSegmentGroup = bolt.AddGroup();
      boltSegmentGroup.Segments.Add(new LightningBoltSegment()
      {
        Start = start,
        End = end
      });
      float num1 = (float) generation / (float) totalGenerations;
      float num2 = num1 * num1;
      boltSegmentGroup.LineWidth = parameters.TrunkWidth * num2;
      boltSegmentGroup.Generation = generation;
      boltSegmentGroup.Color = parameters.Color;
      if (generation == parameters.Generations && (parameters.MainTrunkTintColor.r != byte.MaxValue || parameters.MainTrunkTintColor.g != byte.MaxValue || parameters.MainTrunkTintColor.b != byte.MaxValue || parameters.MainTrunkTintColor.a != byte.MaxValue))
      {
        boltSegmentGroup.Color.r = (byte) (0.0039215688593685627 * (double) boltSegmentGroup.Color.r * (double) parameters.MainTrunkTintColor.r);
        boltSegmentGroup.Color.g = (byte) (0.0039215688593685627 * (double) boltSegmentGroup.Color.g * (double) parameters.MainTrunkTintColor.g);
        boltSegmentGroup.Color.b = (byte) (0.0039215688593685627 * (double) boltSegmentGroup.Color.b * (double) parameters.MainTrunkTintColor.b);
        boltSegmentGroup.Color.a = (byte) (0.0039215688593685627 * (double) boltSegmentGroup.Color.a * (double) parameters.MainTrunkTintColor.a);
      }
      boltSegmentGroup.Color.a = (byte) ((double) byte.MaxValue * (double) num2);
      boltSegmentGroup.EndWidthMultiplier = parameters.EndWidthMultiplier * parameters.ForkEndWidthMultiplier;
      if ((double) offsetAmount <= 0.0)
        offsetAmount = (end - start).magnitude * (generation == totalGenerations ? parameters.ChaosFactor : parameters.ChaosFactorForks);
      while (generation-- > 0)
      {
        int startIndex = boltSegmentGroup.StartIndex;
        boltSegmentGroup.StartIndex = boltSegmentGroup.Segments.Count;
        for (int index = startIndex; index < boltSegmentGroup.StartIndex; ++index)
        {
          start = boltSegmentGroup.Segments[index].Start;
          end = boltSegmentGroup.Segments[index].End;
          Vector3 vector3 = (start + end) * 0.5f;
          Vector3 result;
          this.RandomVector(bolt, ref start, ref end, offsetAmount, parameters.Random, out result);
          Vector3 midPoint = vector3 + result;
          List<LightningBoltSegment> segments1 = boltSegmentGroup.Segments;
          LightningBoltSegment lightningBoltSegment1 = new LightningBoltSegment();
          lightningBoltSegment1.Start = start;
          lightningBoltSegment1.End = midPoint;
          LightningBoltSegment lightningBoltSegment2 = lightningBoltSegment1;
          segments1.Add(lightningBoltSegment2);
          List<LightningBoltSegment> segments2 = boltSegmentGroup.Segments;
          lightningBoltSegment1 = new LightningBoltSegment();
          lightningBoltSegment1.Start = midPoint;
          lightningBoltSegment1.End = end;
          LightningBoltSegment lightningBoltSegment3 = lightningBoltSegment1;
          segments2.Add(lightningBoltSegment3);
          this.CreateFork(bolt, parameters, generation, totalGenerations, start, midPoint);
        }
        offsetAmount *= 0.5f;
      }
    }

    public Vector3 RandomDirection3D(System.Random random)
    {
      float num = (float) (2.0 * random.NextDouble() - 1.0);
      return (this.RandomDirection2D(random) * Mathf.Sqrt((float) (1.0 - (double) num * (double) num))) with
      {
        z = num
      };
    }

    public Vector3 RandomDirection2D(System.Random random)
    {
      float f = (float) (random.NextDouble() * 2.0 * 3.1415927410125732);
      return new Vector3(Mathf.Cos(f), Mathf.Sin(f), 0.0f);
    }

    public Vector3 RandomDirection2DXZ(System.Random random)
    {
      float f = (float) (random.NextDouble() * 2.0 * 3.1415927410125732);
      return new Vector3(Mathf.Cos(f), 0.0f, Mathf.Sin(f));
    }

    public void RandomVector(
      LightningBolt bolt,
      ref Vector3 start,
      ref Vector3 end,
      float offsetAmount,
      System.Random random,
      out Vector3 result)
    {
      if (bolt.CameraMode == CameraMode.Perspective)
      {
        Vector3 normalized = (end - start).normalized;
        Vector3 side = Vector3.Cross(start, end);
        if (side == Vector3.zero)
          this.GetPerpendicularVector(ref normalized, out side);
        else
          side.Normalize();
        float num1 = ((float) random.NextDouble() + 0.1f) * offsetAmount;
        float num2 = (float) random.NextDouble() * 3.14159274f;
        Vector3 vector3 = normalized * (float) Math.Sin((double) num2);
        Quaternion quaternion;
        quaternion.x = vector3.x;
        quaternion.y = vector3.y;
        quaternion.z = vector3.z;
        quaternion.w = (float) Math.Cos((double) num2);
        result = quaternion * side * num1;
      }
      else if (bolt.CameraMode == CameraMode.OrthographicXY)
      {
        end.z = start.z;
        Vector3 normalized = (end - start).normalized;
        Vector3 vector3 = new Vector3(-normalized.y, normalized.x, 0.0f);
        float num = (float) (random.NextDouble() * (double) offsetAmount * 2.0) - offsetAmount;
        result = vector3 * num;
      }
      else
      {
        end.y = start.y;
        Vector3 normalized = (end - start).normalized;
        Vector3 vector3 = new Vector3(-normalized.z, 0.0f, normalized.x);
        float num = (float) (random.NextDouble() * (double) offsetAmount * 2.0) - offsetAmount;
        result = vector3 * num;
      }
    }

    public void GenerateLightningBolt(LightningBolt bolt, LightningBoltParameters parameters)
    {
      this.GenerateLightningBolt(bolt, parameters, out Vector3 _, out Vector3 _);
    }

    public void GenerateLightningBolt(
      LightningBolt bolt,
      LightningBoltParameters parameters,
      out Vector3 start,
      out Vector3 end)
    {
      start = parameters.ApplyVariance(parameters.Start, parameters.StartVariance);
      end = parameters.ApplyVariance(parameters.End, parameters.EndVariance);
      this.OnGenerateLightningBolt(bolt, start, end, parameters);
    }
  }
}
