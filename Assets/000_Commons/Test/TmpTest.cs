using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class TmpTest : MonoBehaviour
{

    public TmpData tData;

    
    void Start()
    {
        Debug.Assert( tData );

        string ss = JsonUtility.ToJson(tData);

        print( "tmpData:\n" + ss  );


    }

    // Update is called once per frame
    void Update()
    {
        


    }
}
