using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{

    public float Speed = 5.0f; //Player Speed


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        // Move forward 
        if(Input.GetKey(KeyCode.W)){
            transform.Translate(Vector3.forward * Speed * Time.deltaTime);
        }

        // Move backward
        if(Input.GetKey(KeyCode.S)){
            transform.Translate(Vector3.back * Speed * Time.deltaTime);
        }

        // Move right
        if(Input.GetKey(KeyCode.D)){
            transform.Translate(Vector3.right * Speed * Time.deltaTime);
        }
        
        // Move left
        if(Input.GetKey(KeyCode.A)){
            transform.Translate(Vector3.left * Speed * Time.deltaTime);
        }
    }
}
