using UnityEngine;

// Base class player
public class Player{
    public string playerName;
    public int Health;
    
    public Player(string name, int hp){
        playerName = name;
        Health = hp;
    }
    
    public virtual void PrintStats(){
        Debug.Log(playerName + " has " + Health + " hp.");
    }
}

// Sub class knights
public class Knight : Player{
    public int armor;
    
    public Knight(string name, int hp, int armorValue) : base(name, hp){
        armor = armorValue;
    }
    
    public override void PrintStats(){
        Debug.Log(playerName + " (knight) has " + Health + " hp and " + armor + " armor.");
    }
}

// Sub class wizards
public class Wizard : Player{
    public int mana;
    
    public Wizard(string name, int hp, int manaValue) : base(name, hp){
        mana = manaValue;
    }
    
    public override void PrintStats(){
        Debug.Log(playerName + " (wizard) has " + Health + " hp and " + mana + " mana.");
    }
    
    public void CastSpell(){
        if(mana >= 5){
            mana -= 5;
            Debug.Log(playerName + " casts Bombarda!");
        }
        else{
            Debug.Log(playerName + " doesn't have enough mana to cast Bombarda!");
        }
    }
}

// Sub class archers
public class Archer : Player{
    public int arrows;
    
    public Archer(string name, int hp, int arrowCount) : base(name, hp){
        arrows = arrowCount;
    }
    
    public override void PrintStats(){
        Debug.Log(playerName + " (archer) has " + Health + " hp and " + arrows + " arrows.");
    }
    
    public void FireArrow(){
        if(arrows > 0){
            arrows--;
            Debug.Log("Archer " + playerName + " fired an arrow, remaining arrows: " + arrows);
        }
        else{
            Debug.Log("Archer " + playerName + " is out of arrows!");
        }
    }
}

public class InheritanceDemo : MonoBehaviour
{
    Knight k;
    Wizard w;
    Archer a;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        k = new Knight("Russell", 100, 50);
        k.PrintStats();
        
        w = new Wizard("Malore", 100, 50);
        w.PrintStats();
        
        a = new Archer("Ranulf", 100, 10);
        a.PrintStats();
    }
    
    // Update is called once per frame
    void Update()
    {
        // Press W to cast wizard spell
        if(Input.GetKeyDown(KeyCode.W)){
            w.CastSpell();
            w.PrintStats(); // Show updated mana
        }
        
        // Press A to fire archer arrow
        if(Input.GetKeyDown(KeyCode.A)){
            a.FireArrow();
        }
    }
}
