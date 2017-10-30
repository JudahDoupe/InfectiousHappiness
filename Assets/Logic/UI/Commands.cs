using System.Collections;
using System.Collections.Generic;
using Assets.Logic.World;
using UnityEngine;
using UnityEngine.UI;

public class Commands : MonoBehaviour
{

    public static Commands Instance;
    public CommandLine input;
    public Text NewText;
    public GameObject obj;

	public GameObject Forward;
    public GameObject Back;
    public GameObject Arrows;
    public GameObject Jump;
    public GameObject Leap;
    public GameObject Vault;
    public GameObject Climb;
    public GameObject Push;
    public GameObject Lift;
    public GameObject Drop;

    void Awake()
    {
        if(Instance == null)
            Instance = this;
        Unlock(Forward);
        Unlock(Back);
        Unlock(Arrows);
        Close();
    }

    void Update()
    {
        switch (Map.InfectionLevel)
        {
            case 22:
                Unlock(Climb);
                break;
            case 42:
                Unlock(Push);
                break;
            case 93:
                Unlock(Lift);
                Unlock(Drop);
                break;
            default:
                break;
        }

    }


    public void Open()
    {
        obj.SetActive(true);
        StartCoroutine(PromptClose());
    }

    private IEnumerator PromptClose()
    {
        yield return new WaitForSeconds( 0.5f);
        input.Placeholder.text = "\"close\" to resume";
    }
    public void Close()
    {
        obj.SetActive(false);
    }

    public void Unlock(GameObject image)
    {
        if (image == null) return;
        if (image.activeSelf) return;

        image.SetActive(true);
        StartCoroutine(DisplayNewText());

    }
    private IEnumerator DisplayNewText()
    {
        NewText.gameObject.SetActive(true);
        yield return new WaitForSeconds(1);
        NewText.gameObject.SetActive(false);
    }
}
