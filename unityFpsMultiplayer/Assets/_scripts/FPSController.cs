using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FPSController : MonoBehaviour
{

    private Transform firstPerson_View;
    private Transform firstPerson_Camera;
    private Vector3 firstPerson_View_Rotation = Vector3.zero;

    public float walkSpeed=6.75f;
    public float runSpeed=10f;
    public float crouchSpeed = 4f;
    public float jumpSpeed = 8f;
    public float gravity = 20f;

    private float speed;
    private bool is_Moving, is_Grounded, is_Crouching;
    private float inputX, inputY;
    private float inputX_Set, inputY_Set;
    private float inputModifyFactor;
    private bool limitDiagonalSpeed = true;

    private float antiBumpFactor = 0.75f;
    private CharacterController characterController;
    private Vector3 moveDirection = Vector3.zero;


    public LayerMask groundLayer;
    private float rayDistance;
    private float default_ControllerHeight;
    private Vector3 default_CamPos;
    private float camHight;

    private FPSPlayerAnimations playerAnimation;


    // Start is called before the first frame update
    void Start()
    {

        firstPerson_View = transform.Find("FPS View").transform;
        characterController = GetComponent<CharacterController>();
        speed = walkSpeed;
        is_Moving = false;

        rayDistance = characterController.height * 0.5f + characterController.radius;
        default_ControllerHeight = characterController.height;
        default_CamPos = firstPerson_View.localPosition;

        playerAnimation = GetComponent<FPSPlayerAnimations>();

    }

    // Update is called once per frame
    void Update()
    {
        PlayerMovement();
    }

    public void PlayerMovement()
    {
        //checking moving forword
        if(Input.GetKey(KeyCode.W)||Input.GetKey(KeyCode.S))
        {
            if(Input.GetKey(KeyCode.W))
            {
                inputY_Set = 1f;
            }
            else
            {
                inputY_Set = -1f;
            }
        }
        else
        {
            inputY_Set = 0f;
        }

        //checking moveing left or right
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
        {
            if (Input.GetKey(KeyCode.A))
            {
                inputX_Set = -1f;
            }
            else
            {
                inputX_Set = 1f;
            }
        }
        else
        {
            inputX_Set = 0f;
        }

        //move  slowly form 0 to 1
        inputY = Mathf.Lerp(inputY,inputY_Set,Time.deltaTime*19f);
        inputX = Mathf.Lerp(inputX, inputX_Set, Time.deltaTime * 19f);

        inputModifyFactor = Mathf.Lerp(inputModifyFactor,
            (inputY_Set != 0 && inputX_Set != 0 && limitDiagonalSpeed) ? 0.75f : 1f,
            Time.deltaTime * 19f);

        firstPerson_View_Rotation = Vector3.Lerp(firstPerson_View_Rotation, Vector3.zero, Time.deltaTime * 5f);
        firstPerson_View.localEulerAngles = firstPerson_View_Rotation;

        if(is_Grounded)
        {
            //crouch
            PlayerCrouchingANdSprinting();

            moveDirection = new Vector3(inputX*inputModifyFactor,-antiBumpFactor,inputY*inputModifyFactor);
            moveDirection = transform.TransformDirection(moveDirection)*speed;

            //jump
            PlayerJump();


        }

        moveDirection.y -= gravity * Time.deltaTime;

        is_Grounded = (characterController.Move(moveDirection * Time.deltaTime) & CollisionFlags.Below) != 0;
        is_Moving = characterController.velocity.magnitude > 0.15f;
        HandleAnimations();

    }

    public void PlayerCrouchingANdSprinting()
    {
        if(Input.GetKeyDown(KeyCode.C))
        {
            if(!is_Crouching)
            {
                is_Crouching = true;
            }
            else
            {
                if(CanGetUp())
                {
                    is_Crouching = false;
                }
            }

            StopCoroutine(MoveCameraCrouch());
            StartCoroutine(MoveCameraCrouch());
        }

        if(is_Crouching)
        {
            speed = crouchSpeed;
        }
        else
        {
            if(Input.GetKey(KeyCode.LeftShift))
            {
                speed = runSpeed;
            }
            else
            {
                speed = walkSpeed;
            }
        }
        playerAnimation.PlayerCrouch(is_Crouching);

    }

    public bool CanGetUp()
    {
        Ray groundRay = new Ray(transform.position,transform.up);
        RaycastHit grounHit;
        if(Physics.SphereCast(groundRay,characterController.radius+0.05f,out grounHit,rayDistance,groundLayer ))
        {
            if(Vector3.Distance(transform.position,grounHit.point)<2.3f)
            {
                return false;
            }


        }
        return true;

    }

    IEnumerator MoveCameraCrouch()
    {
        characterController.height = is_Crouching ? default_ControllerHeight / 1.5f : default_ControllerHeight;
        characterController.center = new Vector3(0f, characterController.height / 2f, 0f);
        camHight = is_Crouching ? default_CamPos.y / 1.5f : default_CamPos.y;

        while(Mathf.Abs(camHight-firstPerson_View.localPosition.y)>0.01f)
        {
            firstPerson_View.localPosition = Vector3.Lerp(firstPerson_View.localPosition,new Vector3(default_CamPos.x,camHight,default_CamPos.z),Time.deltaTime*11f);
            yield return null;
        }
    }

    public void PlayerJump()
    {
        if(Input.GetKeyDown(KeyCode.Space))
        {
            if(is_Crouching)
            {
                if(CanGetUp())
                {
                    is_Crouching = false;

                    playerAnimation.PlayerCrouch(is_Crouching);

                    StopCoroutine(MoveCameraCrouch());
                    StartCoroutine(MoveCameraCrouch());

                }
            }
            else
            {
                moveDirection.y = jumpSpeed;
            }
        }
    }

    void HandleAnimations()
    {
        playerAnimation.Movement(characterController.velocity.magnitude);
        playerAnimation.PlayerJump(characterController.velocity.y);

        if(is_Crouching&&characterController.velocity.magnitude>0.0f)
        {
            playerAnimation.PlayerCrouchWalk(characterController.velocity.magnitude);
        }


    }

}
