// Decompiled with JetBrains decompiler
// Type: ElevatorAnimationEvents
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: 44743D94-7478-4365-A095-189C76175301
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System.Collections;
using UnityEngine;

#nullable disable
public class ElevatorAnimationEvents : MonoBehaviour
{
  public RoundManager roundManager;
  public AudioSource audioToPlay;
  public AudioSource audioToPlay2;
  private Coroutine fadeCoroutine;

  public void PlayAudio(AudioClip SFXclip)
  {
    if (!this.roundManager.ElevatorLowering && !this.roundManager.ElevatorRunning)
      return;
    this.audioToPlay.clip = SFXclip;
    this.audioToPlay.Play();
  }

  public void PlayAudio2(AudioClip SFXclip)
  {
    if (!this.roundManager.ElevatorLowering && !this.roundManager.ElevatorRunning)
      return;
    this.audioToPlay2.clip = SFXclip;
    this.audioToPlay2.Play();
  }

  public void PlayAudioOneshot(AudioClip SFXclip)
  {
    Debug.Log((object) string.Format("elevator running? : {0}", (object) this.roundManager.ElevatorRunning));
    if (!this.roundManager.ElevatorLowering && !this.roundManager.ElevatorRunning)
      return;
    this.audioToPlay.PlayOneShot(SFXclip);
  }

  public void PlayAudio2Oneshot(AudioClip SFXclip)
  {
    if (!this.roundManager.ElevatorLowering && !this.roundManager.ElevatorRunning)
      return;
    this.audioToPlay2.PlayOneShot(SFXclip);
  }

  public void StopAudio(AudioSource audio) => audio.Stop();

  public void FadeAudioOut(AudioSource audio)
  {
    if (this.fadeCoroutine != null)
      this.StopCoroutine(this.fadeCoroutine);
    this.fadeCoroutine = this.StartCoroutine(this.fadeAudioIn(false));
  }

  public void FadeAudioIn(AudioSource audio)
  {
    if (this.fadeCoroutine != null)
      this.StopCoroutine(this.fadeCoroutine);
    this.fadeCoroutine = this.StartCoroutine(this.fadeAudioIn(true));
  }

  private IEnumerator fadeAudioIn(bool fadeIn)
  {
    if (fadeIn)
    {
      this.audioToPlay2.volume = 0.0f;
      for (int i = 0; i < 20; ++i)
      {
        yield return (object) null;
        this.audioToPlay2.volume += 0.05f;
      }
    }
    else
    {
      for (int index = 0; index < 20; ++index)
        this.audioToPlay2.volume -= 0.05f;
      this.audioToPlay2.Stop();
    }
  }

  public void LoadNewFloor()
  {
  }

  public void ElevatorFullyRunning()
  {
    this.roundManager.isSpawningEnemies = false;
    this.roundManager.DetectElevatorIsRunning();
    if ((Object) GameNetworkManager.Instance.localPlayerController != (Object) null && !GameNetworkManager.Instance.localPlayerController.isPlayerDead)
    {
      if (!GameNetworkManager.Instance.localPlayerController.isInElevator)
      {
        Debug.Log((object) string.Format("Killing player obj #{0}, they were not in the ship when it left.", (object) GameNetworkManager.Instance.localPlayerController.playerClientId));
        GameNetworkManager.Instance.localPlayerController.KillPlayer(Vector3.zero, false, CauseOfDeath.Abandoned);
        HUDManager.Instance.AddTextToChatOnServer(GameNetworkManager.Instance.localPlayerController.playerUsername + " was left behind.");
      }
      else
        this.roundManager.playersManager.ForcePlayerIntoShip();
    }
    this.roundManager.playersManager.ShipHasLeft();
    this.SetBodiesKinematic();
  }

  private void SetBodiesKinematic()
  {
    DeadBodyInfo[] objectsOfType = Object.FindObjectsOfType<DeadBodyInfo>();
    for (int index = 0; index < objectsOfType.Length; ++index)
    {
      if (StartOfRound.Instance.shipBounds.bounds.Contains(objectsOfType[index].bodyParts[5].position))
        objectsOfType[index].isInShip = true;
      if (objectsOfType[index].isInShip && (Object) objectsOfType[index].grabBodyObject != (Object) null && !objectsOfType[index].grabBodyObject.isHeld)
      {
        objectsOfType[index].grabBodyObject.grabbable = false;
        objectsOfType[index].grabBodyObject.grabbableToEnemies = false;
        objectsOfType[index].SetBodyPartsKinematic();
      }
    }
  }

  public void ElevatorNoLongerRunning() => this.roundManager.ElevatorRunning = false;
}
