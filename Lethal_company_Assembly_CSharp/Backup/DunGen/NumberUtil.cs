// Decompiled with JetBrains decompiler
// Type: DunGen.NumberUtil
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  public static class NumberUtil
  {
    public static float ClampToNearest(float value, params float[] possibleValues)
    {
      float[] numArray = new float[possibleValues.Length];
      for (int index = 0; index < possibleValues.Length; ++index)
        numArray[index] = Mathf.Abs(value - possibleValues[index]);
      float num1 = float.MaxValue;
      int index1 = 0;
      for (int index2 = 0; index2 < numArray.Length; ++index2)
      {
        float num2 = numArray[index2];
        if ((double) num2 < (double) num1)
        {
          num1 = num2;
          index1 = index2;
        }
      }
      return possibleValues[index1];
    }
  }
}
