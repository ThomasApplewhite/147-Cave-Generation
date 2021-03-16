using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovementController : MonoBehaviour
{
    public float walkSpeed;
    public float jumpHeight;
    public float gravity = 2f;
    bool moving = false;
    protected CharacterController playerController;
    private float moveDirY = 0;
    private Quaternion rotation = Quaternion.identity;
    private CameraController camController;

    private void Awake()
    {
        playerController = this.GetComponent<CharacterController>();
        camController = (CameraController)FindObjectOfType(typeof(CameraController));
    }

    void Update()
    {
        float horizDirection = Input.GetAxis("Horizontal");
        float vertDirection = Input.GetAxis("Vertical");
        RaycastHit hit;
        Debug.DrawRay(transform.position - transform.up, -transform.up, Color.green);
        if(Physics.Raycast(transform.position - transform.up, -transform.up, out hit, .25f) && Input.GetKeyDown(KeyCode.Space))
        {
            moveDirY = jumpHeight;
        }
        if(!playerController.isGrounded)
        {
            moveDirY -= gravity * Time.deltaTime;
        }
        Debug.Log("hit: " + hit.transform);
        Vector3 moveDirection = new Vector3(-horizDirection, moveDirY, -vertDirection);
        moveDirection = transform.TransformDirection(moveDirection);
        playerController.Move(moveDirection * walkSpeed * Time.deltaTime);
        Vector3 camRotation = camController.GetCameraRotation();
        playerController.gameObject.transform.eulerAngles = (new Vector3(0, camRotation.y + 180, 0));
        
    }

}
