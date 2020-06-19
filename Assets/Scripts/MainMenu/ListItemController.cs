using System;
using TMPro;
using UnityEngine;

public class ListItemController : MonoBehaviour
{
    public TMP_Text ProjectName;
    public TMP_Text UpdatedAt;
    public TMP_Text DesignerUsername;
    public TMP_Text Status;

    public Project project = null;
    public bool updateRequired = false;

    private MainMenuManager mainMenuManager = null;
    private LocalProjectManager localProjectManager = null;

    void Awake()
    {
        mainMenuManager = FindObjectOfType<MainMenuManager>();
        localProjectManager = FindObjectOfType<LocalProjectManager>();
    }

    public void SetupListItem(Project p)
    {
        project = p;

        if (ProjectName != null)
        {
            ProjectName.text = project.name;
        }
        if (DesignerUsername != null)
        {
            DesignerUsername.text = project.designer.username;
        }
        if (UpdatedAt != null)
        {
            string formattedDate = Convert.ToDateTime(project.updatedAt).ToString("yyyy-MM-dd HH:mm tt");
            UpdatedAt.text = formattedDate;
        }

        if (!localProjectManager.ProjectExistsLocally(project.id))
        {
            updateRequired = true;
            Status.text = "Download Required";
            return;
        }
        if(!localProjectManager.HashMatchesWithLocalFile(project.id, project.projectHash))
        {
            updateRequired = true;
            Status.text = "Update Required";
            return;
        }

        Status.text = "Project ready";
    }


    // The on click function
    public void SelectProject()
    {
        mainMenuManager.SetCurrentOpenProject(project.id, project.projectHash, updateRequired);
    }
}
