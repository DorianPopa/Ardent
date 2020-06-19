using System;

[Serializable]
public class Project
{
    public string id;
    public string name;
    public string updatedAt;
    public string projectHash;
    public User designer;
    public User client;
}
