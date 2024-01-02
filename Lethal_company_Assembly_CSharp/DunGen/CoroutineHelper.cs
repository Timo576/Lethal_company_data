// Decompiled with JetBrains decompiler
// Type: DunGen.CoroutineHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
namespace DunGen
{
  public sealed class CoroutineHelper : MonoBehaviour
  {
    private static CoroutineHelper instance;

    private static CoroutineHelper Instance
    {
      get
      {
        if ((Object) CoroutineHelper.instance == (Object) null)
        {
          GameObject gameObject = new GameObject("DunGen Coroutine Helper");
          gameObject.hideFlags = HideFlags.HideInHierarchy;
          CoroutineHelper.instance = gameObject.AddComponent<CoroutineHelper>();
        }
        return CoroutineHelper.instance;
      }
    }

    public static Coroutine Start(IEnumerator routine)
    {
      return CoroutineHelper.Instance.StartCoroutine(routine);
    }

    public static void StopAll() => CoroutineHelper.Instance.StopAllCoroutines();
  }
}
