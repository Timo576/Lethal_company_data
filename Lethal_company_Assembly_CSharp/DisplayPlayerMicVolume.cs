// Decompiled with JetBrains decompiler
// Type: DisplayPlayerMicVolume
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using Dissonance;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

#nullable disable
public class DisplayPlayerMicVolume : MonoBehaviour
{
  public bool useDissonanceForMicDetection;
  [Space(3f)]
  private DissonanceComms comms;
  public Image volumeMeterImage;
  public float detectedVolumeAmplitude;
  private VoicePlayerState playerState;
  public float MicLoudness;
  private string _device;
  private AudioClip _clipRecord;
  private int _sampleWindow = 128;
  private bool _isInitialized;

  private void InitMic()
  {
    IngamePlayerSettings.Instance.RefreshAndDisplayCurrentMicrophone(false);
    if (IngamePlayerSettings.Instance.settings.micDevice == "none")
    {
      Debug.Log((object) "No devices connected");
    }
    else
    {
      if (this._device != null && Microphone.IsRecording(this._device))
        this.StopMicrophone();
      this._device = IngamePlayerSettings.Instance.unsavedSettings.micDevice;
      int minFreq;
      int maxFreq;
      Microphone.GetDeviceCaps(this._device, out minFreq, out maxFreq);
      this._clipRecord = Microphone.Start(this._device, true, 1, Mathf.Clamp(5000, minFreq, maxFreq));
    }
  }

  private void StopMicrophone() => Microphone.End(this._device);

  public void SwitchMicrophone()
  {
    if (this._isInitialized)
      Microphone.End(this._device);
    this.InitMic();
  }

  private float LevelMax()
  {
    float num1 = 0.0f;
    float[] data = new float[this._sampleWindow];
    int offsetSamples = Microphone.GetPosition(IngamePlayerSettings.Instance.unsavedSettings.micDevice) - (this._sampleWindow + 1);
    if (offsetSamples < 0)
      return 0.0f;
    this._clipRecord.GetData(data, offsetSamples);
    for (int index = 0; index < this._sampleWindow; ++index)
    {
      float num2 = data[index] * data[index];
      if ((double) num1 < (double) num2)
        num1 = num2;
    }
    return num1;
  }

  private void Update()
  {
    this.volumeMeterImage.fillAmount = Mathf.Lerp(this.volumeMeterImage.fillAmount, this.detectedVolumeAmplitude, 25f * Time.deltaTime);
    this.detectedVolumeAmplitude = 0.0f;
    if (!IngamePlayerSettings.Instance.unsavedSettings.micEnabled)
      return;
    if (this.useDissonanceForMicDetection && (Object) NetworkManager.Singleton != (Object) null)
    {
      if ((Object) this.comms == (Object) null)
        this.comms = Object.FindObjectOfType<DissonanceComms>();
      this.detectedVolumeAmplitude = Mathf.Clamp(this.comms.FindPlayer(this.comms.LocalPlayerName).Amplitude * 35f, 0.0f, 1f);
    }
    else
      this.detectedVolumeAmplitude = Mathf.Clamp(this.LevelMax() * 300f, 0.0f, 1f);
    if ((double) this.detectedVolumeAmplitude >= 0.25)
      return;
    this.detectedVolumeAmplitude = 0.0f;
  }

  private void OnEnable()
  {
    if (this.useDissonanceForMicDetection || !Application.isFocused)
      return;
    this.InitMic();
    this._isInitialized = true;
  }

  private void Awake()
  {
    if (this.useDissonanceForMicDetection)
      return;
    this._clipRecord = AudioClip.Create("newClip", 44100, 1, 2000, true);
  }

  private void OnDisable()
  {
    if (this.useDissonanceForMicDetection)
      return;
    this.StopMicrophone();
    this._isInitialized = false;
  }

  private void OnDestroy()
  {
    if (this.useDissonanceForMicDetection)
      return;
    this.StopMicrophone();
  }

  private void OnApplicationFocus(bool focus)
  {
    if (this.useDissonanceForMicDetection)
      return;
    if (focus && !this._isInitialized)
    {
      this.InitMic();
      this._isInitialized = true;
    }
    if (focus)
      return;
    this.StopMicrophone();
    this._isInitialized = false;
  }
}
