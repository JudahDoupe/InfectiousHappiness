using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

namespace Assets.Logic.Commands
{
    public class CommandLine : MonoBehaviour
    {
        public InputField Input;
        public Text Placeholder;
        public string LastCommand;
        public Character Character;
	
        void Update () {
            if (Input.isFocused == false)
            {
                EventSystem.current.SetSelectedGameObject(Input.gameObject, null);
                Input.OnPointerClick(new PointerEventData(EventSystem.current));
            }

            if (UnityEngine.Input.GetButtonDown("Submit"))
            {
                var command = Input.text;
                Input.text = "";

                if (command == "")
                {
                    ExecuteCommand(LastCommand);
                }
                else
                {
                    Placeholder.text = command;
                    ExecuteCommand(command);
                    LastCommand = command;
                }
            }
        }

        void ExecuteCommand(string input)
        {
            input = input.ToLower();

            switch (input)
            {
                //movement
                case "f":
                case "forward":
                    Character.Forward();
                    return;
                case "b":
                case "back":
                    Character.Back();
                    return;
                case "r":
                case "right":
                    Character.Right();
                    return;
                case "l":
                case "left":
                    Character.Left();
                    return;

                //specical
                case "jump":
                    Character.Jump();
                    return;
                case "bounce":
                    Character.Bounce();
                    return;
                case "leap":
                    Character.Leap();
                    return;

                case "climb":
                    Character.Climb();
                    return;
                case "vault":
                    Character.Vault();
                    return;
                case "switch":
                    Character.Switch();
                    return;

                //block interactions
                case "push":
                    Character.Push();
                    return;
                case "pull":
                    Character.Pull();
                    return;
                case "punch":
                    Character.Punch();
                    return;

                case "lift":
                    Character.Lift();
                    return;
                case "drop":
                    Character.Drop();
                    return;

                //misc
                case "ls":
                    //open menu
                    return;
                default:
                    StartCoroutine(CommandFail());
                    return;
            }
        }

        private IEnumerator CommandFail()
        {
            Placeholder.text = "Invalid Command";
            Placeholder.color = new Color(1,0,0,1f);
            yield return new WaitForSeconds(1);
            Placeholder.text = "Enter command...";
            Placeholder.color = new Color(1, 1, 1, 0.5f);
        }

    }
}
