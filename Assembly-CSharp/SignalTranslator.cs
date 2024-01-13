// Decompiled with JetBrains decompiler
// Type: SignalTranslator
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using UnityEngine;

#nullable disable
public class SignalTranslator : MonoBehaviour
{
  public float timeLastUsingSignalTranslator;
  public Coroutine signalTranslatorCoroutine;
  public int timesSendingMessage;
  public AudioClip[] typeTextClips;
  public AudioClip finishTypingSFX;
  public AudioClip startTransmissionSFX;
  public AudioSource localAudio;
}
