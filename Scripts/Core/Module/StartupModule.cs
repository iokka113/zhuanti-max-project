using UnityEngine;

public static class StartupModule
{
    public static void Init(){
        GameObject.Instantiate(Resources.Load("Modules/LoggingModule") as GameObject);
        GameObject.Instantiate(Resources.Load("Modules/DatabaseModule") as GameObject);
        GameObject.Instantiate(Resources.Load("Modules/NetworkModule") as GameObject);
    }
}
