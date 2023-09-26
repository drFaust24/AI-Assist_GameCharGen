using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Threading;
using OpenAI_API;
using OpenAI_API.Chat;
using OpenAI_API.Models;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;


public class AIControl : MonoBehaviour
{
    // AI Fields
    public TMP_InputField textField;
    public TMP_InputField inputField;
    public TMP_InputField AIKey;
    public TMP_Text CurKey;    
    
    public Button AskButton;

    private OpenAIAPI api;
    private List<ChatMessage> messages;
    private string aiKey;
   
    // Start is called before the first frame update
    void Start()
    {
        // This line gets your API key (and could be slightly different on Mac/Linux)

        aiKey = SaveSystem.GetString("ChatGptKEY");
        AskButton.onClick.AddListener(() => GetResponse());
        CurKey.text = aiKey;

        StartConversation();
    }

    // AI Start
    private void StartConversation()
    {
        messages = new List<ChatMessage> {
            new ChatMessage(ChatMessageRole.System, "You are the personal assistent")
        };

        inputField.text = "";
        string startString = ">> Assistant online. Ask your question";
        textField.text = startString;
        Debug.Log(startString);

     
    }

    // AI Response
    private async void GetResponse()
    {

        // Fill the user message from the input field
        ChatMessage userMessage = new ChatMessage();
        userMessage.Role = ChatMessageRole.User;
        userMessage.Content = inputField.text;
        
        // Add the message to the list
        messages.Add(userMessage);

        // Update the text field with the user message
        textField.text = string.Format(">> You: {0}", userMessage.Content);

        // Clear the input field
        inputField.text = "";

        // Send the entire chat to OpenAI to get the next message
        var chatResult = await api.Chat.CreateChatCompletionAsync(new ChatRequest()
        {
            Model = Model.ChatGPTTurbo,
            Temperature = 0.9,
            MaxTokens = 2500,
            Messages = messages
        });

        // Get the response message
        ChatMessage responseMessage = new ChatMessage();
        responseMessage.Role = chatResult.Choices[0].Message.Role;
        responseMessage.Content = chatResult.Choices[0].Message.Content;
        Debug.Log(string.Format("{0}: {1}", responseMessage.rawRole, responseMessage.Content));
        
        // Add the response to the list of messages
        messages.Add(responseMessage);

        // Update the text field with the response
        textField.text = string.Format(">> You:\n {0}\n\n>> Assistant:\n {1}", userMessage.Content, responseMessage.Content);

    }
    void Update()
    {
        api = new OpenAIAPI(aiKey);        
        if (Input.GetKeyUp(KeyCode.Return)) { GetResponse(); }

    }

    public void Exit()
    {
        SceneManager.LoadScene(0);
    }

    public void NewRequest()
    {
        messages.Clear();
    }

    public void SaveKey()
    {
        aiKey = AIKey.text;
        SaveSystem.SetString("ChatGptKEY", aiKey);
        SaveSystem.SaveToDisk();
        CurKey.text = aiKey;
    }
}