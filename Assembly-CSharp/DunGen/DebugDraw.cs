// Decompiled with JetBrains decompiler
// Type: DunGen.DebugDraw
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace DunGen
{
  public static class DebugDraw
  {
    public static void Bounds(
      UnityEngine.Bounds localBounds,
      Matrix4x4 transform,
      Color colour,
      float duration = 0.0f,
      bool depthTest = false)
    {
      Vector3 min = localBounds.min;
      Vector3 max = localBounds.max;
      Vector3 start = transform.MultiplyPoint(new Vector3(min.x, max.y, max.z));
      Vector3 vector3_1 = transform.MultiplyPoint(new Vector3(min.x, max.y, min.z));
      Vector3 vector3_2 = transform.MultiplyPoint(new Vector3(max.x, max.y, max.z));
      Vector3 vector3_3 = transform.MultiplyPoint(new Vector3(max.x, max.y, min.z));
      Vector3 vector3_4 = transform.MultiplyPoint(new Vector3(min.x, min.y, max.z));
      Vector3 vector3_5 = transform.MultiplyPoint(new Vector3(min.x, min.y, min.z));
      Vector3 vector3_6 = transform.MultiplyPoint(new Vector3(max.x, min.y, max.z));
      Vector3 end = transform.MultiplyPoint(new Vector3(max.x, min.y, min.z));
      Debug.DrawLine(start, vector3_1, colour, duration, depthTest);
      Debug.DrawLine(start, vector3_2, colour, duration, depthTest);
      Debug.DrawLine(vector3_1, vector3_3, colour, duration, depthTest);
      Debug.DrawLine(vector3_2, vector3_3, colour, duration, depthTest);
      Debug.DrawLine(vector3_4, vector3_5, colour, duration, depthTest);
      Debug.DrawLine(vector3_4, vector3_6, colour, duration, depthTest);
      Debug.DrawLine(vector3_5, end, colour, duration, depthTest);
      Debug.DrawLine(vector3_6, end, colour, duration, depthTest);
      Debug.DrawLine(start, vector3_4, colour, duration, depthTest);
      Debug.DrawLine(vector3_2, vector3_6, colour, duration, depthTest);
      Debug.DrawLine(vector3_3, end, colour, duration, depthTest);
      Debug.DrawLine(vector3_1, vector3_5, colour, duration, depthTest);
    }
  }
}
