using CodeMonkey.Utils;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class MainMenuHandler : MonoBehaviour
{
    void Start()
    {
        GameObject.Find ("Play Game Button").GetComponent<Button_UI>().ClickFunc = ()=>{
		SceneManager.LoadScene ("Main Game");
		};
		GameObject.Find ("Options Button").GetComponent<Button_UI>().ClickFunc = ()=>{
		
		};
		GameObject.Find ("Room Editor Button").GetComponent<Button_UI>().ClickFunc = ()=>{
		SceneManager.LoadScene ("Room Editor");
		};
		GameObject.Find ("Exit Game Button").GetComponent<Button_UI>().ClickFunc = ()=>{
		Application.Quit ();
		};
    }
}
