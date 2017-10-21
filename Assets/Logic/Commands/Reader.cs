using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Assets.Commands
{
    public class Reader : MonoBehaviour
    {
        public InputField CommandLine;
        public Command LastCommand;
        public Character character;
	
        void Update () {
            if (CommandLine.isFocused == false)
            {
                EventSystem.current.SetSelectedGameObject(CommandLine.gameObject, null);
                CommandLine.OnPointerClick(new PointerEventData(EventSystem.current));
            }

            if (Input.GetButtonDown("Submit"))
            {
                var command = ParseWord(CommandLine.text);
                if (command != Command.Unknown)
                {
                    LastCommand = command;
                    Debug.Log(command);
                }
                else
                {
                    Debug.Log("Command Not Known");
                }
                CommandLine.text = "";
            }
        }

        Command ParseWord(string input)
        {
            input = input.ToLower();

            switch (input)
            {
                case "f":
                case "forward":
                    character.Forward();
                    return Command.Forward;
                case "right":
                    return Command.Right;
                case "left":
                    return Command.Left;
                default:
                    return Command.Unknown;
            }
        }

    }
}
