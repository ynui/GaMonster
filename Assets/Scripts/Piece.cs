using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Piece : MonoBehaviour {
    public int color;
	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    public void move(Vector3 newPos)
    {
        transform.position = newPos;
    }

    public int getPieceColor()
    {
        return color;
    }

    public void deletePiece()
    {
        Destroy(this.gameObject);
    }
}
