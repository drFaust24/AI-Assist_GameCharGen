using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System;
using System.IO;
using System.Threading.Tasks;
using TMPro;

namespace OpenAI
{
    public class DallE : MonoBehaviour
    {
        public TMP_InputField inputField;
        public Button sendBttn;
        public Image image;
        public TMP_Text StatusBar;
        public TMP_InputField fileName;
        public TMP_InputField filePath;
        public TMP_Text CurFilePath;

        private string aiKey;
        private OpenAIApi openai;
        private string FName;
        private string FPath;
        private string status;

        void Start()
        {
            FPath = SaveSystem.GetString("PathToSave");
            aiKey = SaveSystem.GetString("ChatGptKEY");
            openai = new OpenAIApi(aiKey);
            Debug.Log(aiKey);
            sendBttn.onClick.AddListener(() => SendImageRequest());
            status = "Waiting for your request";
            CurFilePath.text = FPath;

        }

        private async void SendImageRequest()
        {
            status = "Generating...";
            var response = await openai.CreateImage(new CreateImageRequest
            {
                Prompt = inputField.text,
                Size = ImageSize.Size1024
            });
           
            if (response.Data != null && response.Data.Count > 0)
            {
                
                using(var request = new UnityWebRequest(response.Data[0].Url))
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                    request.SetRequestHeader("Access-Control-Allow-Origin", "*");
                    request.SendWebRequest();

                    while (!request.isDone) await Task.Yield();

                    Texture2D texture = new Texture2D(2, 2);
                    texture.LoadImage(request.downloadHandler.data);
                    var sprite = Sprite.Create(texture, new Rect(0, 0, 1024, 1024), Vector2.zero, 1f);
                    image.sprite = sprite;
                    status = "Done! Check it out!";
                }
            }
            else
            {
                status = "Sorry, it can't be done...";
                Debug.LogWarning("No image was created from this prompt.");
            }

            
        }

        void Update()
        {
            
            if (Input.GetKeyUp(KeyCode.Return)) { SendImageRequest(); }
            StatusBar.text = status;

        }

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
            
            byte[] textureBytes = image.sprite.texture.EncodeToPNG();
            File.WriteAllBytes(pathsToSave, textureBytes);
            status = "Picture saved!";
            Debug.Log(pathsToSave);
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
            SceneManager.LoadScene(0);
        }

    }
}
