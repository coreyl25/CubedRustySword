using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{

    public string playerName = "Russell";
    public int health = 100;
    public int score = 0;

    public void GameOver(){
            Debug.Log(playerName + " has taken critical damage and fainted! Game over.");
        }

public void takeDamage(int amount){
    health = health - amount;
    Debug.Log(playerName + " took " + amount + " damage and has " + health + " remaining.");
}

public void newScore(int updateScore){
    score = score + updateScore;
    Debug.Log("New score is: " + score);
}
 // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log(playerName + " spawned with " + health + " hp! The current score is: " + score);
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Return)){
            takeDamage(20);
            if(health <= 0){
                GameOver();
            }
        }

        if(Input.GetKeyDown(KeyCode.C)){
            newScore(10);
        }
    }
}


   

