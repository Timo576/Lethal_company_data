// Decompiled with JetBrains decompiler
// Type: DunGen.DoorwaySocket
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using UnityEngine;

#nullable disable
namespace DunGen
{
  [CreateAssetMenu(fileName = "New Doorway Socket", menuName = "DunGen/Doorway Socket", order = 700)]
  public class DoorwaySocket : ScriptableObject
  {
    [SerializeField]
    private Vector2 size = new Vector2(1f, 2f);
    [Obsolete("Use DoorwayPairFinder.CustomConnectionRules instead")]
    public static SocketConnectionDelegate CustomSocketConnectionDelegate;

    public Vector2 Size => this.size;

    public static bool CanSocketsConnect(DoorwaySocket a, DoorwaySocket b)
    {
      return DoorwaySocket.CustomSocketConnectionDelegate != null ? DoorwaySocket.CustomSocketConnectionDelegate(a, b) : (UnityEngine.Object) a == (UnityEngine.Object) b;
    }
  }
}
