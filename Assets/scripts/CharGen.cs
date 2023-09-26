using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using TMPro;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using OpenAI;

[System.Serializable]
public class Character {
/*
    [SerializeField] public string Name;
    [SerializeField] public string Race;
    [SerializeField] public string Age;
    [SerializeField] public string Gender;
    [SerializeField] public string Class;
    [SerializeField] private string Story;
*/
    public TMP_InputField CharName;
    public TMP_InputField CharRace;
    public TMP_InputField CharAge;
    public TMP_InputField CharGender;
    public TMP_InputField CharClass;
    public TMP_InputField AdditionalParams;
    public Toggle Classical;
    public TMP_Dropdown MainParam;

}


public class CharGen : MonoBehaviour
{
    // UI
    public TMP_InputField CharStory;
    public Image CharAva;
    public TMP_Text StatusBar;
    public Button toStoryBttn;
    public Button toAvaBttn;
    public TMP_Text DescrLength;
    // save panel
    public TMP_Text CurFilePath;
    public TMP_InputField filePath;
    public TMP_InputField fileName;
    
    public Character character;    

    // private var
    private OpenAIAPI api;
    private OpenAIApi apiDl; 
    private List<OpenAI_API.Chat.ChatMessage> messages;
    private string aiKey;
    private string FPath;
    private string FName;
    private string status;
    private string charparam;
    private string dnd = ". Give dispersion of classic DnD parameters: Strength, Agility, Dextrenity, Intelligence, Charisma, Luck. Basic value 5 points. As dominate +1-3 point parameter use ";

    // Scene start parameters, load data
    void Start()
    {
        FPath = SaveSystem.GetString("PathToSave");
        aiKey = SaveSystem.GetString("ChatGptKEY");
        api = new OpenAIAPI(aiKey);
        apiDl = new OpenAIApi(aiKey);
        CurFilePath.text = FPath;
       
        toStoryBttn.onClick.AddListener(() => MakeStory());
        toAvaBttn.onClick.AddListener(() => MakeAva());

    }

// ----- Generating Character storyline -----
    private async void MakeStory()
    {
        status = "Generating story...";
        toAvaBttn.interactable = false;
        
        // request params to string
        string charzone = "Character name " + character.CharName.text + ", this race: " + character.CharRace.text + ", he is " + character.CharAge.text + " years old and this gender: " + character.CharGender.text + ", characters class is: " + character.CharClass.text + " also consider this: " + character.AdditionalParams.text;

        if (character.Classical == true)
        {
            charparam = charzone + dnd + character.MainParam.options[character.MainParam.value].text;
        }
        else { 
            charparam = charzone; 
        }
        Debug.Log(charparam);

        // AI preset
        messages = new List<OpenAI_API.Chat.ChatMessage> {
            new OpenAI_API.Chat.ChatMessage(ChatMessageRole.System, "You are a gamemaster. Generate character storyline to be used in RPG game and game parameters. Maximum answer length 1000 characters. If user gives some parameter as ? - you must generate them. Don't ask about more specifications.")
        };


        // Fill the user message from the input field
        OpenAI_API.Chat.ChatMessage userMessage = new OpenAI_API.Chat.ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = charparam;
        messages.Add(userMessage);

        // request
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 1000,
            Messages = messages
        });

        // response
        OpenAI_API.Chat.ChatMessage responseMessage = new OpenAI_API.Chat.ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
        messages.Add(responseMessage);

        CharStory.text = string.Format(responseMessage.Content);

        status = "Character story ready";
    }

// ----- Generating Character Avatar -----
    private async void MakeAva()
    {
        status = "Generating avatar...";

        var response = await apiDl.CreateImage(new CreateImageRequest
        {
            Prompt = CharStory.text,
            Size = ImageSize.Size1024
        });

        if (response.Data != null && response.Data.Count > 0)
        {

            using (var request = new UnityWebRequest(response.Data[0].Url))
            {
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                request.SendWebRequest();

                while (!request.isDone) await Task.Yield();

                Texture2D texture = new Texture2D(2, 2);
                texture.LoadImage(request.downloadHandler.data);
                var sprite = Sprite.Create(texture, new Rect(0, 0, 1024, 1024), Vector2.zero, 1f);
                CharAva.sprite = sprite;
                status = "Done! Check it out!";
            }
        }
        else
        {
            status = "Sorry, it can't be done...";
            Debug.LogWarning("No image was created from this prompt.");
        }


        status = "Character avatar updated"; 
    }


    // Update. Story length counter
    void Update()
    {
        StatusBar.text = status;
        int content = CharStory.text.Length;
        DescrLength.text = content.ToString();
        if (content <= 1000)
        {
            toAvaBttn.interactable = true;
        }
    }

    // save avatar
    public void WriteImageOnDisk()
    {
        FName = fileName.text;
        CurFilePath.text = FPath;
        string pathsToSave = FPath + FName + ".png";
        if (File.Exists(pathsToSave))
        {
            status = "NOT SAVED. This filename exists";
            return;
        }

        byte[] textureBytes = CharAva.sprite.texture.EncodeToPNG();
        File.WriteAllBytes(pathsToSave, textureBytes);
        status = "Picture saved!";
        Debug.Log(pathsToSave);
    }

    // new save dir
    public void newPathToSave()
    {
        FPath = filePath.text;
        SaveSystem.SetString("PathToSave", FPath);
        SaveSystem.SaveToDisk();
        CurFilePath.text = FPath;
    }

    // exit bttn
    public void Exit()
    {
        SceneManager.LoadScene(0);
    }
}
