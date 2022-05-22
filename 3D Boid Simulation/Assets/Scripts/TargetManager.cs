using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TargetManager : MonoBehaviour
{

    public static TargetManager _instance;

    [SerializeField] private Transform defaultTargetTransform;

    public Transform GetDefaultTargeTransform()
    {

        return defaultTargetTransform;

    }


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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
