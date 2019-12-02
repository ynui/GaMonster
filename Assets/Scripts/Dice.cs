using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dice : MonoBehaviour
{

    // Array of dice sides sprites to load from Resources folder
    [SerializeField]
    private Sprite[] diceSides = new Sprite[6];
    [SerializeField]
    private gameManager GM;
    [SerializeField]
    private int dieNum;
    // Reference to sprite renderer to change sprites
    private SpriteRenderer rend;
    int value;
    // Use this for initialization
    private void Start()
    {

        // Assign Renderer component
        rend = GetComponent<SpriteRenderer>();

    }

    // If you left click over the dice then RollTheDice coroutine is started
    public void rollTheDice()
    {
        StartCoroutine(RollTheDice());
    }

    // Coroutine that rolls the dice
    private IEnumerator RollTheDice()
    {
        // Variable to contain random dice side number.
        // It needs to be assigned. Let it be 0 initially
        int randomDiceSide = 0;

        // Final side or value that dice reads in the end of coroutine
       int finalSide;

        // Loop to switch dice sides ramdomly
        // before final side appears. 20 itterations here.
        for (int i = 0; i <= 20; i++)
        {
            // Pick up random value from 0 to 5 (All inclusive)
            randomDiceSide = Random.Range(0, 6);

            // Set sprite to upper face of dice from array according to random value
            rend.sprite = diceSides[randomDiceSide];

            // Pause before next itteration
            yield return new WaitForSeconds(0.05f);
        }

        // Assigning final side so you can use this value later in your game
        // for player movement for example
        finalSide = randomDiceSide + 1;
        value = finalSide;
        GM.setRollValue(dieNum, finalSide);

        // Show final dice value in Console
    }
    public int GetValue()
    {
        return value;
    }
}
