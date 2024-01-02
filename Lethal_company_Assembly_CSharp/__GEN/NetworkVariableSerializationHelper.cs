// Decompiled with JetBrains decompiler
// Type: __GEN.NetworkVariableSerializationHelper
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

#nullable disable
namespace __GEN
{
  internal class NetworkVariableSerializationHelper
  {
    [RuntimeInitializeOnLoadMethod]
    internal static void InitializeSerialization()
    {
      NetworkVariableSerializationTypes.InitializeSerializer_FixedString<FixedString128Bytes>();
      NetworkVariableSerializationTypes.InitializeEqualityChecker_UnmanagedIEquatable<FixedString128Bytes>();
    }
  }
}
