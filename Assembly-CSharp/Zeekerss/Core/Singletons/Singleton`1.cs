// Decompiled with JetBrains decompiler
// Type: Zeekerss.Core.Singletons.Singleton`1
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
namespace Zeekerss.Core.Singletons
{
  public class Singleton<T> : MonoBehaviour where T : Component
  {
    private static T _instance;

    public static T Instance
    {
      get
      {
        if ((Object) Singleton<T>._instance == (Object) null)
        {
          T[] objectsOfType = Object.FindObjectsOfType(typeof (T)) as T[];
          if (objectsOfType.Length != 0)
            Singleton<T>._instance = objectsOfType[0];
          if (objectsOfType.Length > 1)
            Debug.LogError((object) ("There is more than one " + typeof (T).Name + " in the scene."));
          if ((Object) Singleton<T>._instance == (Object) null)
          {
            GameObject gameObject = new GameObject();
            gameObject.name = string.Format("_{0}", (object) typeof (T).Name);
            Singleton<T>._instance = gameObject.AddComponent<T>();
          }
        }
        return Singleton<T>._instance;
      }
    }
  }
}
