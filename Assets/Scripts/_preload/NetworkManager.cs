using System;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using System.Net.Http;
using System.IO;
using System.Collections.Generic;

public class NetworkManager : MonoBehaviour
{
    public const string API_Base =      @"http://79.112.14.227:5000/";
    public const string API_Login =     API_Base + @"login";
    public const string API_Projects =  API_Base + @"projects";
    public const string API_Users =     API_Base + @"users";

    readonly HttpClient client = new HttpClient();

    private Session currentUserSession;

    void Awake()
    {
        currentUserSession = null;
    }

    void Start()
    {
        
    }

    public Session GetSession()
    {
        return currentUserSession;
    }

    public void Logout()
    {
        currentUserSession = null;
        if (client.DefaultRequestHeaders.Contains("Bearer"))
            client.DefaultRequestHeaders.Remove("Bearer");
    }

    public async Task<Session> Login(LoginUserModel loginData)
    {
        string jsonData = JsonUtility.ToJson(loginData);
        if (jsonData == null)
            throw new Exception("Invalid loginData object");

        try
        {
            byte[] responseBody = await PostRequestAsync(API_Login, jsonData);
            string jsonResponse = Encoding.UTF8.GetString(responseBody);
            currentUserSession = JsonUtility.FromJson<Session>(jsonResponse);
            if (currentUserSession == null)
                throw new Exception("Invalid server response for Session");
            
            return currentUserSession;
        }
        catch(Exception e)
        {
            throw e;
        }
    }

    public async Task<Project[]> GetProjectsAsync()
    {
        try
        {
            byte[] responseBody = await GetRequestAsync(API_Projects);
            string jsonResponse = Encoding.UTF8.GetString(responseBody);

            jsonResponse = "{\"projectArray\":" + jsonResponse + "}";
            ProjectArray projectArrayRootObject = JsonUtility.FromJson<ProjectArray>(jsonResponse);

            return projectArrayRootObject.projectArray;
        }
        catch(Exception e)
        {
            throw e;
        }
    }

    public async Task<Stream> GetProjectFilesAsync(string projectId)
    {
        try
        {
            string requestPath = API_Projects + @"/" + projectId + @"/files";
            Stream responseStream = await GetRequestAsyncStream(requestPath);

            return responseStream;
        }
        catch(Exception e)
        {
            throw e;
        }
    }

    public async Task<byte[]> PostRequestAsync(string url, string bodyJsonString)
    {
        try
        {
            if (currentUserSession != null && !client.DefaultRequestHeaders.Contains("Bearer"))
                client.DefaultRequestHeaders.Add("Bearer", currentUserSession.token);

            HttpResponseMessage response = await client.PostAsync(url, new StringContent(bodyJsonString, Encoding.UTF8, "application/json"));
            response.EnsureSuccessStatusCode();
            byte[] responseBody = await response.Content.ReadAsByteArrayAsync();

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            print("Exception Caught!");
            print($"Message :{e.Message} ");
            throw new Exception(e.Message);
        }
    }

    public async Task<byte[]> GetRequestAsync(string url) 
    {
        try
        {
            if (currentUserSession != null && !client.DefaultRequestHeaders.Contains("Bearer"))
                client.DefaultRequestHeaders.Add("Bearer", currentUserSession.token);

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            byte[] responseBody = await response.Content.ReadAsByteArrayAsync();

            return responseBody;
        }
        catch(HttpRequestException e)
        {
            print("Exception Caught!");
            print($"Message :{e.Message} ");
            throw new Exception(e.Message);
        }
    }

    public async Task<Stream> GetRequestAsyncStream(string url)
    {
        try
        {
            if (currentUserSession != null && !client.DefaultRequestHeaders.Contains("Bearer"))
                client.DefaultRequestHeaders.Add("Bearer", currentUserSession.token);

            HttpResponseMessage response = await client.GetAsync(url);
            response.EnsureSuccessStatusCode();
            Stream responseBody = await response.Content.ReadAsStreamAsync();

            return responseBody;
        }
        catch (HttpRequestException e)
        {
            print("Exception Caught!");
            print($"Message :{e.Message} ");
            throw new Exception(e.Message);
        }
    }

    /*
    public void Login(LoginUserModel loginData)
    {
        string jsonData = JsonUtility.ToJson(loginData);
        if (jsonData == null)
            return;

        StartCoroutine(PostRequestCoroutine(API_Login, jsonData, LoginCallback));
    }

    private void LoginCallback(long responseCode, byte[] responseBody)
    {
        print("Coroutine finished");
        if (responseCode != 200)
        {
            Debug.Log("Login Failed");
            return;
        }

        string jsonResponse = Encoding.UTF8.GetString(responseBody);
        currentUserSession = JsonUtility.FromJson<Session>(jsonResponse);
    }

    IEnumerator GetRequestCoroutine(string url, Action<long, byte[]> callback)
    {
        var request = new UnityWebRequest(url, "GET");
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        Debug.Log("GET request status Code: " + request.responseCode);
        callback(request.responseCode, request.downloadHandler.data);
    }

    IEnumerator PostRequestCoroutine(string url, string bodyJsonString, Action<long, byte[]> callback)
    {
        var request = new UnityWebRequest(url, "POST");
        byte[] bodyRaw = Encoding.UTF8.GetBytes(bodyJsonString);
        request.uploadHandler = (UploadHandler)new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = (DownloadHandler)new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");
        yield return request.SendWebRequest();

        Debug.Log("POST request status Code: " + request.responseCode);
        callback(request.responseCode, request.downloadHandler.data);
    }
    */
}
