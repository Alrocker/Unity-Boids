using UnityEngine;

public abstract class Singleton : MonoBehaviour
{
    public static Singleton _instance;
    // Start is called before the first frame update
    void Awake()
    {
        //Singelton
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning("Multiple singleton instances; deleting script on " + this.gameObject);
            Destroy(this);
        }
        else
        {
            _instance = this;
        }
    }

}
