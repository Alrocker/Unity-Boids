using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class LookAtActiveCamera : MonoBehaviour
{

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void LateUpdate()
    {
        //Debug.Log(UnityEditor.SceneView.lastActiveSceneView.camera.name);
        if (Camera.current != null && !Camera.current.name.Equals("SceneCamera"))
        {
           this.transform.LookAt(Camera.current.transform);
        }
    }
}
