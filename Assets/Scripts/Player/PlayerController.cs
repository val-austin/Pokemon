using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlayerController : MonoBehaviour
{
    public float moveSpeed;
    public LayerMask solidObjectLayer;
    public LayerMask grassLayer;

    public event Action onEncountered;

    private bool isMoving;
    private Vector2 input;

    //Animation
    private Animator animator;
    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    // Start is called before the first frame update
    

    // Update is called once per frame
    public void HandleUpdate()
    {
        if (!isMoving)
        {
            input.x = Input.GetAxisRaw("Horizontal");
            input.y = Input.GetAxisRaw("Vertical");

            //Prevents Diagonal Movementt
            if (input.x != 0) input.y = 0;

            if(input != Vector2.zero)
            {

                animator.SetFloat("moveX", input.x);
                animator.SetFloat("moveY", input.y);

                var targetPos = transform.position;
                targetPos.x += input.x;
                targetPos.y += input.y;

                if (isWalkable(targetPos))
                {
                    StartCoroutine(Move(targetPos));
                }
            }
        }
        animator.SetBool("isMoving", isMoving);
    }

    IEnumerator Move(Vector3 targetPos)
    {
        isMoving = true;
        while((targetPos - transform.position).sqrMagnitude > Mathf.Epsilon)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = targetPos;
        isMoving = false;

        checkForEncounters();
    }

    private bool isWalkable(Vector3 targetPos)
    {
        //There is an object there
        if(Physics2D.OverlapCircle(targetPos, 0.2f, solidObjectLayer) != null){
            return false;
        }
        return true;
    }
    private void checkForEncounters()
    {
        if(Physics2D.OverlapCircle(transform.position, .2f, grassLayer) != null)
        {
            if(UnityEngine.Random.Range(1, 101) <= 10)
            {
                animator.SetBool("isMoving", false);
                Debug.Log("Encountered wild Pokemon.");
                onEncountered();
            }
        }
    }
}
