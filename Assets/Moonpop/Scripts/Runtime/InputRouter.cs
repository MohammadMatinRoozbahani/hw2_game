using Moonpop.Core;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.UI;

namespace Moonpop
{
    public sealed class InputRouter : MonoBehaviour
    {
        private InputActionAsset actions;
        private InputActionMap gameplay;
        private InputAction navigate,submit,cancel;
        private EventSystem eventSystem;
        private GameManager game;
        public void Initialize(GameManager gm)
        {
            game=gm;
            var asset=Resources.Load<InputActionAsset>("Input/DrivingControls");
            if(!asset)throw new System.InvalidOperationException("DrivingControls.inputactions is missing.");
            actions=InputActionAsset.FromJson(asset.ToJson());
            gameplay=actions.FindActionMap("Gameplay",true);
            navigate=actions.FindAction("UI/Navigate",true);
            submit=actions.FindAction("UI/Submit",true);
            cancel=actions.FindAction("UI/Cancel",true);
            gameplay.FindAction("Left",true).performed+=OnLeft;
            gameplay.FindAction("Right",true).performed+=OnRight;
            gameplay.FindAction("Boost",true).performed+=OnBoost;
            gameplay.FindAction("Boost",true).canceled+=OnBoostReleased;
            var system=actions.FindActionMap("System",true);
            system.FindAction("Pause",true).performed+=OnPause;
            system.FindAction("Restart",true).performed+=OnRestart;
            system.FindAction("Menu",true).performed+=OnMenu;
            system.FindAction("Mute",true).performed+=OnMute;
            system.FindAction("Camera",true).performed+=OnCamera;
            var eventGo=new GameObject("EventSystem (New Input System only)");eventGo.SetActive(false);
            eventGo.transform.SetParent(transform,false);
            eventSystem=eventGo.AddComponent<EventSystem>();
            var module=eventGo.AddComponent<InputSystemUIInputModule>();
            module.actionsAsset=actions;
            module.point=Reference("UI/Point");module.leftClick=Reference("UI/Click");
            module.scrollWheel=Reference("UI/Scroll");module.move=Reference("UI/Navigate");
            module.submit=Reference("UI/Submit");module.cancel=Reference("UI/Cancel");
            eventGo.SetActive(true);
            actions.FindActionMap("UI",true).Enable();system.Enable();SetState(game.Session.State);
        }
        private InputActionReference Reference(string path)=>InputActionReference.Create(actions.FindAction(path,true));
        public void SetState(GameState state)
        {
            bool driving=state==GameState.Playing;
            if(!driving){gameplay.Disable();game.Session.SetBoostHeld(false);}
            SetActionEnabled(navigate,!driving);
            SetActionEnabled(submit,!driving);
            SetActionEnabled(cancel,!driving);
            eventSystem.sendNavigationEvents=!driving;
            if(driving){eventSystem.SetSelectedGameObject(null);gameplay.Enable();}
        }
        private static void SetActionEnabled(InputAction action,bool enabled){if(enabled)action.Enable();else action.Disable();}
        private void OnLeft(InputAction.CallbackContext c){game.Player.ChangeLane(-1);}
        private void OnRight(InputAction.CallbackContext c){game.Player.ChangeLane(1);}
        private void OnBoost(InputAction.CallbackContext c){game.Session.SetBoostHeld(true);}
        private void OnBoostReleased(InputAction.CallbackContext c){game.Session.SetBoostHeld(false);}
        private void OnCamera(InputAction.CallbackContext c){game.CycleCamera();}
        private void OnPause(InputAction.CallbackContext c){game.TogglePause();}
        private void OnRestart(InputAction.CallbackContext c){if(game.Session.State!=GameState.Menu)game.StartRun();}
        private void OnMenu(InputAction.CallbackContext c){if(game.Session.State!=GameState.Menu)game.ReturnToMenu();}
        private void OnMute(InputAction.CallbackContext c){game.ToggleMute();}
        private void OnDestroy(){if(actions){actions.Disable();Destroy(actions);}}
    }
}
