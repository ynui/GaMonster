using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tile : MonoBehaviour
{
    float distance = 0.68f;
    [SerializeField]
    List<Piece> pieces = new List<Piece>();
    public int up;
    public int indx;
    // Use this for initialization
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {


    }

    public Piece removePiece()
    {
        Piece p = pieces[pieces.Count - 1];
        pieces.RemoveAt(pieces.Count - 1);
        return p;
    }
    public void addPiece(Piece piece)
    {
        double add = -(0.1 * this.howManyPieces() + 1);
        piece.move(new Vector3(transform.position.x, transform.position.y + ((pieces.Count * distance) * up), (float)add));
        pieces.Add(piece);
    }

    public int getIndx()
    {
        return indx;
    }

    public void setIndx(int newIndx)
    {
        indx = newIndx;
    }
    public int getColor()
    {
        if (pieces.Count == 0)
            return -1;
        return pieces[0].getPieceColor();
    }

    public int howManyPieces()
    {
        return pieces.Count;
    }

    public void deletePiecesInTile()
    {
        
        while (pieces.Count > 0)  
            this.removePiece().deletePiece();
    }

}
