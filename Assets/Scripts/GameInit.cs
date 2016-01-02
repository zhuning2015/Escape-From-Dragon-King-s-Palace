using UnityEngine;
using System.Collections;

public class GameInit : MonoBehaviour
{
    public GameObject scoresListCanvas;
    public GameObject helpCanvas;
	
    // Use this for initialization
	void Start ()
    {
        scoresListCanvas.SetActive(false);
        helpCanvas.SetActive(false);
	}
}
