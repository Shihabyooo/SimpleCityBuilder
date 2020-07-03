using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [SerializeField] Transform focus; //assigned in editor, double checked on Awake().
    [SerializeField] float zoomSpeed = 65.0f; 
    [SerializeField] float cameraSpeed = 5.0f;
    [SerializeField] LayerMask cameraConstrainingLayers; //layers that the camera won't zoom past.
    [SerializeField] float maxCameraAngle = 70.0f;
    [SerializeField] float minCameraAngle = 10.0f;
    [SerializeField] float degreesPerTick = 5.0f;
    [SerializeField] CameraBoundary cameraBoundary = new CameraBoundary();

    public delegate void PostRender();
    public static PostRender postRender;

    void Awake()
    {
        if (focus == null)
        {
            if (this.transform.Find("Focus") != null)
                focus =this.transform.Find("Focus");
            else
                print ("ERROR! No focus transform is assigned to main Camera, and no child transform named \"Focus\" was found");
        }
    }

    public void Zoom(float zoomRate)
    {
        float hitDist = zoomSpeed * Time.deltaTime + 0.5f;
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        bool isHit = Physics.Raycast(ray, out hit, hitDist, cameraConstrainingLayers);

        if (isHit && zoomRate > 0.0f)
            return;
        
        Vector3 moveDist = zoomRate * this.transform.forward * zoomSpeed * Time.deltaTime;
        //print (moveDist);
        this.transform.position += zoomRate * this.transform.forward * zoomSpeed * Time.deltaTime;

        //Clam the position to within the boundaries
        this.transform.position = cameraBoundary.Clamp(this.transform.position);
    }

    public void Move(Vector3 direction)
    {
        this.transform.Translate(direction * cameraSpeed * Time.deltaTime, Space.Self);
    }

    public void Pan(Vector2 direction)
    {
        //We pan over the X-Z plane. The problem is that we can't do this relative the the raw state of the object's space, because the rotation about the X-axis (camera tilt)
        //does similarily to our plane, but we do need to maintain the rotation about the Y-axis as well, so we can't use the world space.
        //A stupid hack: rotate set rotation over the X-axis to zero before translating, then reset it again. Stupid, but works...

        //Convert the recieved X and Y values to X and Z values (with the new Y being zero).
        Vector3 panDirection = Vector3.zero;
        panDirection.x = direction.x;
        panDirection.z = direction.y;
        
        //Backup the rotation.
        Quaternion originalRot = this.transform.rotation;
        //Set rotation about X-axis to zero.
        this.transform.eulerAngles = new Vector3(0.0f, this.transform.eulerAngles.y, this.transform.eulerAngles.z); 
        //translate (pan) the camera.
        this.transform.Translate(panDirection * cameraSpeed * Time.deltaTime, Space.Self );
        //Clam the position to within the boundaries
        this.transform.position = cameraBoundary.Clamp(this.transform.position);
        //rest rotation.
        this.transform.rotation = originalRot;
    }

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

        this.transform.eulerAngles = new Vector3(this.transform.eulerAngles.x, this.transform.eulerAngles.y, 0.0f);
        
        //Clam the position to within the boundaries
        this.transform.position = cameraBoundary.Clamp(this.transform.position);
    }

    public RaycastHit CastRay(LayerMask layerMask, float distance)
    {
        Ray ray = new Ray(this.transform.position, this.transform.forward);
        RaycastHit hit;
        Physics.Raycast(ray, out hit, distance, cameraConstrainingLayers);

        return hit;
    }

    void OnPostRender()
    {
        if (postRender != null)
            postRender.Invoke();
    }

    //testing visualization
    void OnDrawGizmos()
    {        
        Gizmos.color = Color.red;

        Vector3 size = new Vector3(cameraBoundary.maxCornerXYZ.x - cameraBoundary.minCornerXYZ.x, cameraBoundary.maxCornerXYZ.y - cameraBoundary.minCornerXYZ.y, cameraBoundary.maxCornerXYZ.z - cameraBoundary.minCornerXYZ.z);
        Vector3 centre = new Vector3((cameraBoundary.maxCornerXYZ.x + cameraBoundary.minCornerXYZ.x) / 2.0f, (cameraBoundary.maxCornerXYZ.y + cameraBoundary.minCornerXYZ.y)/ 2.0f, (cameraBoundary.maxCornerXYZ.z + cameraBoundary.minCornerXYZ.z)/ 2.0f);

        Gizmos.DrawWireCube(centre ,size);
        
        if (UnityEditor.EditorApplication.isPlaying)
        {
            Gizmos.DrawCube(focus.position, new Vector3(0.5f, 0.5f, 0.5f));
        }
    }
}

[System.Serializable]
public class CameraBoundary 
{
    [SerializeField] public Vector3 minCornerXYZ = new Vector3();
    [SerializeField] public Vector3 maxCornerXYZ = new Vector3();


    public bool isOOB(Vector3 position)
    {
        if (position.x <= maxCornerXYZ.x && position.x >= minCornerXYZ.x
            && position.y <= maxCornerXYZ.y && position.y >= minCornerXYZ.y
            && position.z <= maxCornerXYZ.z && position.z >= minCornerXYZ.z)
            return true;

        return false;
    }

    public Vector3 Clamp(Vector3 position)
    {
        return new Vector3(Mathf.Clamp(position.x, minCornerXYZ.x, maxCornerXYZ.x),
                            Mathf.Clamp(position.y, minCornerXYZ.y, maxCornerXYZ.y),
                            Mathf.Clamp(position.z, minCornerXYZ.z, maxCornerXYZ.z));
    }



    
}