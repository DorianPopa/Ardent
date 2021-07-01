using System;
using System.IO;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    private NetworkManager networkManager = null;
    private AppController appController = null;
    private LocalProjectManager localProjectManager = null;

    private Session currentSession = null;

    private GameObject Username;

    private GameObject ListElementContainer;
    public GameObject ListElementPrefab;

    public Project[] ProjectList;

    async void Awake()
    {
        print("Main Menu manager Awake");

        networkManager = FindObjectOfType<NetworkManager>();
        appController = FindObjectOfType<AppController>();
        localProjectManager = FindObjectOfType<LocalProjectManager>();

        Username = GameObject.Find("Username");
        ListElementContainer = GameObject.Find("Elements Container");

        currentSession = networkManager.GetSession();

        ProjectList = await networkManager.GetProjectsAsync();
        UpdateProjectListOnScreen();
        print("debug");
    }

    void Start()
    {
        print("Main Menu manager Start");
        Username.GetComponent<TMP_Text>().text = currentSession.username;
    }

    void Update()
    {
        if (Input.GetKey(KeyCode.Escape))
        {
            Logout();
        }
    }

    private void UpdateProjectListOnScreen()
    {
        foreach (Project p in ProjectList)
        {
            GameObject newListItem = Instantiate(ListElementPrefab, ListElementContainer.transform);
            newListItem.name = p.id;

            ListItemController currentItemController = newListItem.GetComponent<ListItemController>();
            currentItemController.SetupListItem(p);
        }
    }

    public void Logout()
    {
        networkManager.Logout();
        appController.LoadScene("LoginMenuScene");
    }

    public async void SetCurrentOpenProject(string projectId, string projectHash, bool updateRequired)
    {
        if (updateRequired)
        {
            try
            {
                Stream projectArchive = await networkManager.GetProjectFilesAsync(projectId);
                string currentObjPath = await localProjectManager.WriteNewFile(projectArchive, projectId, projectHash);

                appController.LoadVisualizerScene(currentObjPath);
            }
            catch (Exception e)
            {
                print(e.Message);
            }
        }
        else
        {
            string currentObjPath = localProjectManager.GetPathToObjProject(projectId);
            appController.LoadVisualizerScene(currentObjPath);
        }
    }
}
