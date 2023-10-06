using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using Cinemachine;




[System.Serializable]
public class VCamPesist
{
    [SerializeField] public float distance = 1f;        // 相机距离 centerGroundPos 的 xz平面 的距离
    [SerializeField] public float cameraH = 0f;         // 相机距离 centerGroundPos 的相对高度
    [SerializeField] public float fov = 40f;            // 相机 fov
    [SerializeField] public float pitchDegree = 0f;     // 相机 俯仰角
    //---
    [SerializeField] public string pathInScene = "";    // editor 中使用 


    public static VCamPesist VCam2Data( CinemachineVirtualCamera vcam_, Vector3 centerGroundPos_ ) 
    {
        Transform vcamTF = vcam_.transform;
        Vector3 posOffset = vcamTF.position - centerGroundPos_;
        var dirHorizon = Vector3.ProjectOnPlane(vcamTF.forward, Vector3.up).normalized;

        var newData = new VCamPesist(){
            distance = Vector3.ProjectOnPlane(posOffset,Vector3.up).magnitude,
            cameraH = posOffset.y,
            fov = vcam_.m_Lens.FieldOfView,
            pitchDegree = Mathf.Acos( Vector3.Dot( dirHorizon, vcamTF.forward ) ) * ( vcamTF.forward.y >= 0.0f ? -1.0f : 1.0f ) * Mathf.Rad2Deg,
            pathInScene = TprIO.GetGameObjectPathInScene(vcamTF)
        };
        return newData;
    }


    public override string ToString()
    {
        return "distance:" + distance + 
                "\n cameraH:" + cameraH +
                "\n fov:" + fov +
                "\n pitchDegree:" + pitchDegree +
                "\n pathInScene:" + pathInScene;
    }
} 



public class VCamPesistList
{
    [SerializeField] public List<VCamPesist> vcamPesists = new List<VCamPesist>();
}





