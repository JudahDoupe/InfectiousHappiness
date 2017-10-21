using Assets.Logic.World;
using UnityEngine;
using UnityEngine.UI;

namespace Assets.Logic.UI
{
    public class Score : MonoBehaviour
    {
        private Text _text;

        void Start ()
        {
            _text = gameObject.GetComponent<Text>();
        }
	
        void Update ()
        {
            _text.text = Map.Score.ToString();
        }
    }
}
