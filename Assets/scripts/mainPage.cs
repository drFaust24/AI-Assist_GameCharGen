using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class mainPage : MonoBehaviour
{

    public TMP_InputField filePath;
    public TMP_Text CurFilePath;
    public TMP_InputField AIKey;
    public TMP_Text CurKey;
    public TMP_Text CurKey2;

    private string aiKey;
    private string FPath;
    private string api;

    // Start is called before the first frame update
    void Start()
    {
        FPath = SaveSystem.GetString("PathToSave");
        aiKey = SaveSystem.GetString("ChatGptKEY");
        CurKey2.text = aiKey;
        CurKey.text = aiKey;
        CurFilePath.text = FPath;
    }

    // Update is called once per frame
    public void GoToChat()
    {
        SceneManager.LoadScene(1);
    }

    public void GoToDALLE()
    {
        SceneManager.LoadScene(2);
    }

    public void GoToCharGen()
    {
        SceneManager.LoadScene(3);
    }

    public void SaveKey()
    {
        aiKey = AIKey.text;
        SaveSystem.SetString("ChatGptKEY", aiKey);
        SaveSystem.SaveToDisk();
        CurKey.text = aiKey;
    }

    public void newPathToSave()
    {
        FPath = filePath.text;
        SaveSystem.SetString("PathToSave", FPath);
        SaveSystem.SaveToDisk();
        CurFilePath.text = FPath;
    }

    public void Exit()
    {
        Application.Quit();
    }
}
