// Decompiled with JetBrains decompiler
// Type: InitializeGame
// Assembly: Assembly-CSharp, Version=0.0.0.0, Culture=neutral, PublicKeyToken=null
// MVID: AF9B1EEC-498A-45AE-BD42-601D6AB85015
// Assembly location: C:\Program Files (x86)\Steam\steamapps\common\Lethal Company\Lethal Company_Data\Managed\Assembly-CSharp.dll

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

#nullable disable
public class InitializeGame : MonoBehaviour
{
  public bool runBootUpScreen = true;
  public Animator bootUpAnimation;
  public AudioSource bootUpAudio;
  public PlayerActions playerActions;
  private bool canSkip;
  private bool hasSkipped;

  private void OnEnable()
  {
    this.playerActions.Movement.OpenMenu.performed += new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
    this.playerActions.Movement.Enable();
  }

  private void OnDisable()
  {
    this.playerActions.Movement.OpenMenu.performed -= new Action<InputAction.CallbackContext>(this.OpenMenu_performed);
    this.playerActions.Movement.Disable();
  }

  private void Awake()
  {
    this.playerActions = new PlayerActions();
    Application.backgroundLoadingPriority = ThreadPriority.Normal;
  }

  public void OpenMenu_performed(InputAction.CallbackContext context)
  {
    if (!context.performed || !this.canSkip || this.hasSkipped)
      return;
    this.hasSkipped = true;
    SceneManager.LoadScene("MainMenu");
  }

  private IEnumerator SendToNextScene()
  {
    if (this.runBootUpScreen)
    {
      this.bootUpAudio.Play();
      yield return (object) new WaitForSeconds(0.2f);
      this.canSkip = true;
      this.bootUpAnimation.SetTrigger("playAnim");
      yield return (object) new WaitForSeconds(3f);
    }
    yield return (object) new WaitForSeconds(0.2f);
    SceneManager.LoadScene("MainMenu");
  }

  private void Start() => this.StartCoroutine(this.SendToNextScene());
}
