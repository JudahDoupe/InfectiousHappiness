using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Logic.Commands
{
    public class CommandReader : MonoBehaviour
    {
        public InputField CommandLine;
        public string LastCommand;
        public Character character;
	
        void Update () {
            if (CommandLine.isFocused == false)
            {
                EventSystem.current.SetSelectedGameObject(CommandLine.gameObject, null);
                CommandLine.OnPointerClick(new PointerEventData(EventSystem.current));
            }

            if (Input.GetButtonDown("Submit"))
            {
                var command = CommandLine.text;

                if (command == "")
                {
                    ExecuteCommand(LastCommand);
                }
                else
                {
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
                case "f":
                case "forward":
                    character.Forward();
                    return;
                case "r":
                case "right":
                    character.Right();
                    return;
                case "l":
                case "left":
                    character.Left();
                    return;
                default:
                    return;
            }
        }

    }
}
