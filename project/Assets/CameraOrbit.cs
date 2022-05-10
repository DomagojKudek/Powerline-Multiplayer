using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraOrbit : MonoBehaviour
{
    private Transform Xform_Camera;
    private Transform Xform_Parent;

    private Vector3 LocalRotation;
    private float CameraDistance = 100f;

    public float MouseSensitivity = 4f;
    public float ScrollSensitivity = 2f;
    public float OrbitDampening = 10f;
    public float ScrollDampening = 6f;

    public bool CameraDisabled = false;


    void Start()
    {
        this.Xform_Camera = this.transform;
        this.Xform_Parent = this.transform.parent;
    }

    void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Mouse1))
            CameraDisabled = !CameraDisabled;



        if (!CameraDisabled)
        {
            if(Input.GetAxis("Mouse X") != 0 || Input.GetAxis("Mouse Y") != 0)
            {
                LocalRotation.x += Input.GetAxis("Mouse X") * MouseSensitivity;
                LocalRotation.y -= Input.GetAxis("Mouse Y") * MouseSensitivity;

                if (LocalRotation.y < 0f)
                    LocalRotation.y = 0f;
                else if (LocalRotation.y > 90f)
                    LocalRotation.y = 90f;
            }


            if(Input.GetAxis("Mouse ScrollWheel") != 0f)
            {
                float ScrollAmount = Input.GetAxis("Mouse ScrollWheel") * ScrollSensitivity;

                ScrollAmount *= (this.CameraDistance * 0.3f);

                this.CameraDistance += ScrollAmount * -1f;

                this.CameraDistance = Mathf.Clamp(this.CameraDistance, 1.5f, 100f);
            }



            Quaternion QT = Quaternion.Euler(LocalRotation.y, LocalRotation.x, 0);
            this.Xform_Parent.rotation = Quaternion.Lerp(this.Xform_Parent.rotation, QT, Time.deltaTime * OrbitDampening);

            if(this.Xform_Camera.localPosition.z != this.CameraDistance * -1f)
            {
                this.Xform_Camera.localPosition = new Vector3(0f, 0f, Mathf.Lerp(this.Xform_Camera.localPosition.z, this.CameraDistance * -1f, Time.deltaTime * ScrollDampening));
            }
        }

    }

}
