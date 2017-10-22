using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;


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
                case "ahead":
                case "lead":
                case "onward":
                case "forth":
                case "progress":
                case "advance":
                case "front":
                case "move":
                case "act":
                case "action":
                case "maneuver":
                case "step":
                case "stride":
                case "roll":
                case "go":
                    Character.Forward();
                    Commands.Instance.Unlock(Commands.Instance.Forward);
                    return;

                case "b":
                case "back":
                case "backward":
                case "behind":
                case "hind":
                case "find":
                case "post":
                case "rear":
                case "rearward":
                case "tail":
                case "abaft":
                case "wake":
                case "aback":
                case "about-face":
                case "face-about":
                    Character.Back();
                    Commands.Instance.Unlock(Commands.Instance.Back);
                return;

                case "r":
                case "right":
                case "turn":
                case "spin":
                case "swing":
                case "bend":
                case "cycle":
                case "wheel":
                case "wind":
                case "deviation":
                case "hook":
                case "gyre":
                case "right-about":
                case "about-right":
                    Character.Right();
                return;

                case "l":
                case "left":
                case "shift":
                case "spiral":
                case "twist":
                case "about":
                case "pivot":
                case "whirl":
                case "deviate":
                case "pirouette":
                case "rotate":
                case "left-about":
                case "about-left":
                    Character.Left();
                return;

                //specical
                case "j":
                case "jump":
                case "dance":
                case "leapfrog":
                case "leapfrogging":
                case "dive":
                case "skip":
                case "rebound":
                case "pep":
                    Character.Jump();
                    Commands.Instance.Unlock(Commands.Instance.Jump);
                return;

                case "leap":
                case "surge":
                case "hurdle":
                case "spring":
                case "upspring":
                case "caper":
                    Character.Leap();
                    Commands.Instance.Unlock(Commands.Instance.Leap);
                    return;

                case "c":
                case "climb":
                case "rise":
                case "ascend":
                case "goup":
                case "mount":
                case "scale":
                case "top":
                case "atop":
                case "escalate":
                    Character.Climb();
                    Commands.Instance.Unlock(Commands.Instance.Climb);
                return;

                case "vault":
                    Character.Vault();
                    Commands.Instance.Unlock(Commands.Instance.Vault);
                return;

                case "switch":
                case "toggle":
                case "alter":
                    Character.Switch();
                    return;

                //block interactions
                case "pu":
                case "push":

                case "drive":
                case "effort":
                case "offense":
                case "thrust":
                case "butt":
                case "exert":
                case "nudge":
                case "poke":
                    Commands.Instance.Unlock(Commands.Instance.Push);
                Character.Push();
                    return;

                case "pun":
                case "punch":
                case "assault":
                case "attack":
                case "impact":
                case "propel":
                case "shove":
                case "propulse":
                    Character.Punch();
                    return;

                case "li":
                case "lift":
                case "pick-up":
                case "carry":
                case "bear":
                case "bring":
                case "ferry":
                case "haul":
                case "import":
                case "lug":
                case "pack":
                case "take":
                case "tote":
                case "transfer":
                case "transmit":
                case "cart":
                case "channel":
                case "convoy":
                case "displace":
                case "fetch":
                case "freight":
                case "relay":
                case "relocate":
                case "shoulder":
                case "transplant":
                case "truck":
                    Commands.Instance.Unlock(Commands.Instance.Lift);
                Character.Lift();
                    return;

                case "drop":
                case "throw":
                case "bunt":
                case "deliver":
                case "fling":
                case "flip":
                case "heave":
                case "hurl":
                case "lob":
                case "pitch":
                case "put":
                case "send":
                case "barrage":
                case "buck":
                case "cast":
                case "chuck":
                case "catapult":
                case "discharge":
                case "flick":
                case "impel":
                case "launch":
                case "overturn":
                case "pelt":
                case "pellet":
                case "project":
                case "scatter":
                case "shower":
                case "sling":
                case "spray":
                case "sprinkle":
                case "stone":
                case "strew":
                case "toss":
                case "tumble":
                case "unhorse":
                case "unseat":
                case "volley":
                case "waft":
                    Commands.Instance.Unlock(Commands.Instance.Drop);
                Character.Drop();
                    return;

                //misc
                case "ls":
                case "help":
                    Commands.Instance.Open();
                    return;
                case "close":
                case "exit":
                Commands.Instance.Close();
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
