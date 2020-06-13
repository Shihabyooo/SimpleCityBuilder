using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//TODO after implementing the Region class replacement, update this class
public class CameraControl : MonoBehaviour
{
    public static CameraControl mainCam;
    Transform focus;
    //public Region minZoomRegion;
    //public Region maxZoomRegion;
    //Region regionCurrent;
    public float zoomSpeed = 65.0f; 
    public float cameraSpeed = 5.0f;

    void Awake()
    {
        if (mainCam == null)
            mainCam = this;
        else
            Destroy(this.gameObject);

        focus = this.transform.Find("Focus");
        //regionCurrent = this.gameObject.GetComponent<Region>();
    }

    // public void Zoom(float zoomRate)
    // {
    //     float zLevel = this.transform.position.z;
    //     zLevel += zoomRate * zoomSpeed * Time.deltaTime;
    //     //zLevel = Mathf.Clamp (zLevel, minZoomRegion.transform.position.z, maxZoomRegion.transform.position.z);

    //     Vector3 newPos = this.transform.position;
    //     newPos.z = zLevel;

    //     this.transform.position = newPos;
    //     Move(Vector3.zero); //Move() function has an OOB check and clamps camera location to within the regionCurrent. Since zooming out reduces the currentRegion, we call with
    //                         //a zero-valued Vector to force clamping. This avoids an unpleasant jump that would happen if one zooms out with camera at edge of region, then moves.
    // }

    // public void Move(Vector3 direction)
    // {
    //     Vector3 newPos = this.transform.position;
    //     newPos += direction * cameraSpeed * Time.deltaTime;

    //     //compute the region based on the current zLevel from the min/maxZoomRegion
    //     //regionCurrent.Average(maxZoomRegion, minZoomRegion, this.transform.position.z);
    //     //newPos.x = Mathf.Clamp(newPos.x, regionCurrent.cornerSW.x, regionCurrent.cornerNE.x);
    //     //newPos.y = Mathf.Clamp(newPos.y, regionCurrent.cornerSW.y, regionCurrent.cornerNE.y);
        
    //     this.transform.position = newPos;
    // }

    [SerializeField] float maxCameraAngle = 70.0f;
    [SerializeField] float minCameraAngle = 10.0f;
    [SerializeField] float degreesPerTick = 5.0f;
    public void RotateView(Vector3 rotation, Vector3 rotationOrigin)
    {
        //Update focus position to the rotationOrigin, and update its axes so the rotation about the horizontal axes works properly.
        focus.position = rotationOrigin;
        focus.LookAt(this.transform.position);

        //limit the rotation about the horizontal axes to the camera angle limits specified.
        if ((rotation.y <= -0.01f && this.transform.eulerAngles.x >= maxCameraAngle)
            || (rotation.y >= 0.01f && this.transform.eulerAngles.x <= minCameraAngle))
            rotation.y = 0.0f;

        this.transform.RotateAround(rotationOrigin, Vector3.up, rotation.x * degreesPerTick); //rotation around the vertical axes is done to world up.
        this.transform.RotateAround(rotationOrigin, focus.right, rotation.y * degreesPerTick);

    }


}