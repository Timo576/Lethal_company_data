// Decompiled with JetBrains decompiler
// Type: DunGen.RandomStream
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;

#nullable disable
namespace DunGen
{
  public sealed class RandomStream
  {
    private const int maxValue = 2147483647;
    private const int seed = 161803398;
    private int iNext;
    private int iNextP;
    private int[] seedArray = new int[56];

    public RandomStream()
      : this(Environment.TickCount)
    {
    }

    public RandomStream(int Seed)
    {
      int num1 = 161803398 - (Seed == int.MinValue ? int.MaxValue : Math.Abs(Seed));
      this.seedArray[55] = num1;
      int num2 = 1;
      for (int index1 = 1; index1 < 55; ++index1)
      {
        int index2 = 21 * index1 % 55;
        this.seedArray[index2] = num2;
        num2 = num1 - num2;
        if (num2 < 0)
          num2 += int.MaxValue;
        num1 = this.seedArray[index2];
      }
      for (int index3 = 1; index3 < 5; ++index3)
      {
        for (int index4 = 1; index4 < 56; ++index4)
        {
          this.seedArray[index4] -= this.seedArray[1 + (index4 + 30) % 55];
          if (this.seedArray[index4] < 0)
            this.seedArray[index4] += int.MaxValue;
        }
      }
      this.iNext = 0;
      this.iNextP = 21;
      Seed = 1;
    }

    private double Sample() => (double) this.InternalSample() * 4.6566128752457969E-10;

    private int InternalSample()
    {
      int iNext = this.iNext;
      int iNextP = this.iNextP;
      int index1;
      if ((index1 = iNext + 1) >= 56)
        index1 = 1;
      int index2;
      if ((index2 = iNextP + 1) >= 56)
        index2 = 1;
      int num = this.seedArray[index1] - this.seedArray[index2];
      if (num == int.MaxValue)
        --num;
      if (num < 0)
        num += int.MaxValue;
      this.seedArray[index1] = num;
      this.iNext = index1;
      this.iNextP = index2;
      return num;
    }

    public int Next() => this.InternalSample();

    private double GetSampleForLargeRange()
    {
      int num = this.InternalSample();
      if ((this.InternalSample() % 2 == 0 ? 1 : 0) != 0)
        num = -num;
      return ((double) num + 2147483646.0) / 4294967293.0;
    }

    public int Next(int minValue, int maxValue)
    {
      if (minValue > maxValue)
        throw new ArgumentOutOfRangeException(nameof (minValue));
      long num = (long) maxValue - (long) minValue;
      return num <= (long) int.MaxValue ? (int) (this.Sample() * (double) num) + minValue : (int) ((long) (this.GetSampleForLargeRange() * (double) num) + (long) minValue);
    }

    public int Next(int maxValue)
    {
      if (maxValue < 0)
        throw new ArgumentOutOfRangeException(nameof (maxValue));
      return (int) (this.Sample() * (double) maxValue);
    }

    public double NextDouble() => this.Sample();

    public void NextBytes(byte[] buffer)
    {
      if (buffer == null)
        throw new ArgumentNullException(nameof (buffer));
      for (int index = 0; index < buffer.Length; ++index)
        buffer[index] = (byte) (this.InternalSample() % 256);
    }
  }
}
