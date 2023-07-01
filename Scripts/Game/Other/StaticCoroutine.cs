using UnityEngine;
using ProjectMax.Unity;

public class StaticCoroutine : MonoBehaviour
{
	private void Start(){
		Singleton<StaticCoroutine>.SetInstance(this);
		Debug.Log("this obj: " + gameObject.name);
	}

	public static StaticCoroutine ins{
		get{
			if(Singleton<StaticCoroutine>.instance){
				return Singleton<StaticCoroutine>.instance;
			}
			GameObject obj = new GameObject(nameof(StaticCoroutine));
			StaticCoroutine cmpt = obj.AddComponent<StaticCoroutine>();
			return Singleton<StaticCoroutine>.SetInstance(cmpt);
		}
	}
}
