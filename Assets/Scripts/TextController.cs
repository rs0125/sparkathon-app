using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;

public class UPCChatManager : MonoBehaviour
{
    [Header("Backend Settings")]
    public string apiBaseUrl = "https://upc-backend-go.onrender.com";
    public string testUPC = "8901023010415"; // Hardcoded test UPC
    
    [Header("UPC Input")]
    public TMP_InputField upcInputField; // Drag your UPC input field here
    

    [Header("UI References")]
    public TMP_InputField inputField;
    public Transform contentArea;
    public GameObject userMessagePrefab;
    public GameObject aiMessagePrefab;

    private string sessionId = "";

    public void OnUPCSubmit()
    {
        StartCoroutine(ScanProductAndStartSession(testUPC));
    }


    public void OnSendClicked()
    {
        string userMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(userMessage) || string.IsNullOrEmpty(sessionId)) return;

        AddUserMessage(userMessage);
        inputField.text = "";
        inputField.ActivateInputField();

        StartCoroutine(SendChatToBackend(userMessage));
    }

    void AddUserMessage(string message)
    {
        GameObject go = Instantiate(userMessagePrefab, contentArea);
        go.GetComponentInChildren<TMP_Text>().text = message;
        StartCoroutine(FixLayout());
    }

    void AddAIMessage(string message)
    {
        GameObject go = Instantiate(aiMessagePrefab, contentArea);
        go.GetComponentInChildren<TMP_Text>().text = message;
        StartCoroutine(FixLayout());
    }

    IEnumerator ScanProductAndStartSession(string upc)
    {
        string url = apiBaseUrl + "/products/scan";
        string bodyJson = "{\"upc\":\"" + upc + "\"}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                UPCScanResponse res = JsonUtility.FromJson<UPCScanResponse>(req.downloadHandler.text);
                sessionId = res.session_id;
                AddAIMessage($" Scanned Product:\n<b>{res.product.title}</b>");
            }
            else
            {
                Debug.LogError("Scan API Error: " + req.error);
                AddAIMessage(" Failed to scan product.");
            }
        }
    }

    IEnumerator SendChatToBackend(string message)
    {
        string url = apiBaseUrl + "/products/chat";
        string bodyJson = "{\"session_id\":\"" + sessionId + "\",\"message\":\"" + EscapeJson(message) + "\"}";

        using (UnityWebRequest req = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJson);
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                UPCChatResponse res = JsonUtility.FromJson<UPCChatResponse>(req.downloadHandler.text);
                AddAIMessage(res.response);
            }
            else
            {
                Debug.LogError("Chat API Error: " + req.error);
                AddAIMessage("❌ AI failed to respond.");
            }
        }
    }

    IEnumerator FixLayout()
    {
        yield return null;
        LayoutRebuilder.ForceRebuildLayoutImmediate(contentArea as RectTransform);
    }

    string EscapeJson(string input)
    {
        return input.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "");
    }

    // Data Models
    [System.Serializable]
    public class UPCScanResponse
    {
        public string session_id;
        public Product product;
    }

    [System.Serializable]
    public class Product
    {
        public int id;
        public string ean;
        public string title;
    }

    [System.Serializable]
    public class UPCChatResponse
    {
        public string response;
        public string session_id;
    }
}
