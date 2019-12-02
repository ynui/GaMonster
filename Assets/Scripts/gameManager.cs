using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class gameManager : MonoBehaviour
{
    [SerializeField]
    private Tile[] tiles = new Tile[26];
    [SerializeField]
    private Tile[] eaten = new Tile[2];
    [SerializeField]
    private Tile curTile = null;
    [SerializeField]
    private Tile aboutToBeDeleted;
    [SerializeField]
    private List<Tile> origin, destination, eatenOrigin;
    [SerializeField]
    private List<char> whichDie = null;
    float clickTime = 0;

    [SerializeField]
    private GameObject[] pieces = new GameObject[2];
    [SerializeField]
    private GameObject rollButton, undoButton, doneButton;
    [SerializeField]
    private GameObject BG;

    [SerializeField]
    private Dice[] dice = new Dice[2];


    [SerializeField]
    private int player;
    public int[] curMoves = new int[4];
    private int[] startPositions = { 2, 0, 0, 0, 0, 5, 0, 3, 0, 0, 0, 5, 5, 0, 0, 0, 3, 0, 5, 0, 0, 0, 0, 2 };
    private int[] startColors = { 0, -1, -1, -1, -1, 1, -1, 1, -1, -1, -1, 0, 1, -1, -1, -1, 0, -1, 0, -1, -1, -1, -1, 1 };
    private int[] removePositions = { 2, 3, 0, 1, 2, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 5, 0, 0, 5, 3, 2 };
    private int[] removeColors = { 1, 1, -1, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, -1, -1, 0, 0, 0 };
    private int[] stuckPositions = { 1, 2, 2, 2, 2, 2, 2, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 0, 0, 0, 0 };
    private int[] stuckColors = { 0, 1, 1, 1, 1, 1, 1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, -1, 0, -1, -1, -1, -1 };
    [SerializeField]
    private int BCount = 0, MCount = 0, mCount = 0;
    [SerializeField]
    int notInHousePieces;

    [SerializeField]
    private char[] movesMap = new char[24];
    [SerializeField]
    private char eatenMovesMap;

    [SerializeField]
    private bool alreadyRolled = false;
    private bool alreadyMoved;
    private bool canDeletePieces;
    [SerializeField]
    private bool bigDieWasUsed;
    [SerializeField]
    private bool smallDieWasUsed;
    [SerializeField]
    private int[] piecesStillAlive = { 15, 15 };
    [SerializeField]
    private int checkIfCanRemove;

    // Use this for initialization
    void Start()
    {

        for (int i = 0; i < 24; i++)
        {
            for (int j = 0; j < startPositions[i]; j++)
            {
                tiles[i].addPiece(Instantiate(pieces[startColors[i]], new Vector3(0, 0, 0), Quaternion.identity).GetComponent<Piece>());
            }
        }
        player = Random.Range(-1, 1);
        prepareTurn();
    }

    // Update is called once per frame
    void Update()
    {
        setDiceColor();
        if (piecesStillAlive[0] == 0 || piecesStillAlive[1] == 0)
            Destroy(BG);
        if (curMoves[0] == -1 && curMoves[1] == -1)
        {

        }
        else if (!alreadyRolled)
        {
            adjustDice();
            movesMap = makeMovesMap();
            if (eaten[player].howManyPieces() > 0)
            {
                eatenMovesMap = makeEatenMovesMap();
                makeAutomaticMovesForEaten();
            }
            if (notInHousePieces == 0)
                makeAutomaticRemovalOfPieces();
            organizeMakeAutomaticMoves();
            alreadyRolled = true;
            if (notInHousePieces == 0)
                checkIfCanRemove = canRemoveAPiece();
            if ((smallDieWasUsed && bigDieWasUsed) || (BCount + mCount + MCount == 0 && checkIfCanRemove == -1))
            {
                endTurn();
            }
        }
        else
        {
            if (curMoves[0] == 0 && curMoves[1] == 0 && curMoves[2] == 0 && curMoves[3] == 0)
            {
                prepareTurn();
            }
            else if (Input.GetMouseButtonDown(0))
            {

                clickTime = Time.time;
                alreadyMoved = false;

            }
            else if (Input.GetMouseButton(0))
            {
                if (alreadyMoved == false)
                {
                    if (Time.time - clickTime > 0.5f)
                    {
                        makeMove(makeTargetIndx(false));
                        alreadyMoved = true;
                    }
                }
            }
            else if (Input.GetMouseButtonUp(0))
            {
                if (alreadyMoved == false)
                {
                    makeMove(makeTargetIndx(true));
                    alreadyMoved = true;
                }
            }
        }

    }



    //prepare board for moving
    public void prepareTurn()
    {
        alreadyRolled = false;
        player = (player + 1) % 2;
        rollButton.SetActive(true);
        undoButton.SetActive(false);
        doneButton.SetActive(false);
        curMoves[0] = -1;
        curMoves[1] = -1;
        checkIfCanRemove = -1;
        bigDieWasUsed = false;
        smallDieWasUsed = false;
        BCount = MCount = mCount = 0;
        while (origin.Count > 0)
            origin.RemoveAt(origin.Count - 1);
        while (destination.Count > 0)
            destination.RemoveAt(destination.Count - 1);
        while (whichDie.Count > 0)
            whichDie.RemoveAt(whichDie.Count - 1);
        while (eatenOrigin.Count > 0)
            eatenOrigin.RemoveAt(eatenOrigin.Count - 1);
    }

    public void adjustDice()
    {
        if (curMoves[0] == curMoves[1])
            curMoves[2] = curMoves[3] = curMoves[0];
        if (curMoves[0] > curMoves[1])
        {
            int temp = curMoves[0];
            curMoves[0] = curMoves[1];
            curMoves[1] = temp;
        }
    }

    public void makeAutomaticMovesForEaten()
    {
        if (eatenMovesMap == 'n' && eaten[player].howManyPieces() > 0)
            smallDieWasUsed = bigDieWasUsed = true;
        if (bigDieWasUsed && smallDieWasUsed)
        {

        }
        else
        {
            int adjust = 0;
            if (player == 1)
                adjust = -23;
            if (eaten[player].howManyPieces() > 0 && eatenMovesMap != 'n' && curMoves[0] == curMoves[1])
            {
                curMoves[3] = 0;
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[1] + adjust - 1));
            }
            if (eaten[player].howManyPieces() > 0 && eatenMovesMap != 'n' && curMoves[0] == curMoves[1])
            {
                curMoves[2] = 0;
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[1] + adjust - 1));
            }
            if (eaten[player].howManyPieces() > 1 && eatenMovesMap == 'B')
            {
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[1] + adjust - 1));
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[0] + adjust - 1));
                smallDieWasUsed = bigDieWasUsed = true;
            }
            else if (eaten[player].howManyPieces() == 1 && eatenMovesMap == 'B' && curMoves[0] == curMoves[1])
            {
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[0] + adjust - 1));
                bigDieWasUsed = true;
            }
            else if (eaten[player].howManyPieces() > 0 && eatenMovesMap == 'M')
            {
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[1] + adjust - 1));
                bigDieWasUsed = true;
                if (eaten[player].howManyPieces() > 0)
                    smallDieWasUsed = true;
            }
            else if (eaten[player].howManyPieces() > 0 && eatenMovesMap == 'm')
            {
                curTile = eaten[player];
                makeMove(Mathf.Abs(curMoves[0] + adjust - 1));
                smallDieWasUsed = true;
                if (eaten[player].howManyPieces() > 0)
                    bigDieWasUsed = true;
            }
        }
    }

    private void makeAutomaticRemovalOfPieces()
    {
        int i = canRemoveAPiece();
        while (i != -1)
        {
            makeRemoveAPiece(i);
            i = canRemoveAPiece();
        }
    }
    private int canRemoveAPiece()
    {
        int targetIndx, adjust = 0;
        if (player == 0)
            adjust = 25;
        if (!smallDieWasUsed)
        {
            targetIndx = Mathf.Abs(adjust - curMoves[0]) - 1;
            if (canMoveToAboutToBeDeleted(targetIndx))
                return 0;
        }
        if(!bigDieWasUsed)
        {
        targetIndx = Mathf.Abs(adjust - curMoves[1]) - 1;
        if (canMoveToAboutToBeDeleted(targetIndx))
            return 1;
        }
        return -1;
    }
    private void makeRemoveAPiece(int move)
    {
        if (move != -1)
        {
            int adjust = 0, startTileIndx;
            if (player == 0)
                adjust = 25;
            startTileIndx = Mathf.Abs(adjust - curMoves[move]) - 1;
            curTile = findRightIndxToRemove(startTileIndx);
            if (move == 0)
            {
                updateRolls(curMoves, 'm');
                if (alreadyRolled)
                    whichDie.Add('m');
            }
            else
            {
                updateRolls(curMoves, 'M');
                if (alreadyRolled)
                    whichDie.Add('M');
            }

            makeMove(24);
        }
    }
    private Tile findRightIndxToRemove(int startTileIndx)
    {
        int adjust = 1, pl = startTileIndx;
        if (player == 1)
            adjust = -1;
        while (pl > -1 && pl < 24)
        {
            if (tiles[pl].getColor() == player)
                return tiles[pl];
            pl += adjust;
        }
        return null;
    }

    public void organizeMakeAutomaticMoves()
    {
        if (smallDieWasUsed && bigDieWasUsed)
        {

        }
        else
        {
            int t = 0;
            if (!smallDieWasUsed)
                t++;
            if (!bigDieWasUsed)
                t++;
            if (curMoves[2] != 0)
                t++;
            if (curMoves[3] != 0)
                t++;
            makeAutomaticMoves(t);
        }
    }
    private void makeAutomaticMoves(int times)
    {
        alreadyRolled = true;
        if (curMoves[0] == curMoves[1])
            makeAutomaticMovesForDouble(times);
        else if (times == 2)
            makeAutomaticMovesForTwoDice();
        else if (times == 1)
            makeAutomaticMovesForOneDie();
    }

    public void makeAutomaticMovesForTwoDice()
    {
        int adjust = 1, check = -1, solutionCount = 0, whatMoveToMake = 0;
        if (player == 1)
            adjust = -1;
        if (BCount == 0 && MCount == 1 && mCount == 1)
        {
            whatMoveToMake = 32;
            solutionCount++;
            curTile = tiles[getIndxFromMoveMap('m')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            whichDie.Add('m');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() != 1)
                {
                    solutionCount++;
                    whatMoveToMake = 41;
                }
            if (BCount == 1 || MCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 42;
            }
            undo();
            smallDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('M')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            whichDie.Add('M');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() == 0)
                {
                    solutionCount++;
                    whatMoveToMake = 31;
                }
            if (BCount == 1 || mCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 32;
            }
            undo();
            bigDieWasUsed = false;
            if (solutionCount == 1)
                executeMove(whatMoveToMake);
        }
        else if (BCount == 1 && MCount == 0 && mCount == 1)
        {
            whatMoveToMake = 22;
            solutionCount++;
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            whichDie.Add('m');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() != -1)
                {
                    solutionCount++;
                    whatMoveToMake = 11;
                }
            if (BCount == 1 || MCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 12;
            }
            undo();
            smallDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            whichDie.Add('M');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() == 0)
                {
                    solutionCount++;
                    whatMoveToMake = 21;
                }
            if (BCount == 1 || mCount == 2)
            {
                solutionCount++;
                whatMoveToMake = 22;
            }
            undo();
            bigDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('m')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            whichDie.Add('m');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() != 1)
                {
                    solutionCount++;
                    whatMoveToMake = 41;
                }
            if (BCount == 1 || MCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 42;
            }
            undo();
            smallDieWasUsed = false;
            if (solutionCount == 1)
                executeMove(whatMoveToMake);
        }
        else if (BCount == 1 && MCount == 1 && mCount == 0)
        {
            whatMoveToMake = 12;
            solutionCount++;
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            whichDie.Add('m');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() != -1)
                {
                    solutionCount++;
                    whatMoveToMake = 11;
                }
            if (BCount == 1 || MCount == 2)
            {
                solutionCount++;
                whatMoveToMake = 12;
            }
            undo();
            smallDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            whichDie.Add('M');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() == 0)
                {
                    solutionCount++;
                    whatMoveToMake = 21;
                }
            if (BCount == 1 || mCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 22;
            }
            undo();
            bigDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('M')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            whichDie.Add('M');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() == 0)
                {
                    solutionCount++;
                    whatMoveToMake = 31;
                }
            if (BCount == 1 || mCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 32;
            }
            undo();
            bigDieWasUsed = false;
            if (solutionCount == 1)
                executeMove(whatMoveToMake);
        }
        else if (BCount == 1 && MCount == 0 && mCount == 0)
        {
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            whichDie.Add('m');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() != -1)
                {
                    solutionCount++;
                    whatMoveToMake = 11;
                }
            if (BCount == 1 || MCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 12;
            }
            undo();
            smallDieWasUsed = false;
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            whichDie.Add('M');
            if (notInHousePieces == 0)
                if (canRemoveAPiece() == 0)
                {
                    solutionCount++;
                    whatMoveToMake = 21;
                }
            if (BCount == 1 || mCount == 1)
            {
                solutionCount++;
                whatMoveToMake = 22;
            }
            undo();
            bigDieWasUsed = false;
            if(solutionCount == 1)
                executeMove(whatMoveToMake);
            return;

        }
        else if (BCount == 0 && MCount == 1 && mCount == 0)
        {
            alreadyRolled = false;
            curTile = tiles[getIndxFromMoveMap('M')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            alreadyRolled = true;
            if (notInHousePieces == 0)
                check = canRemoveAPiece();
            if ((mCount == 1 || BCount == 1) && check != -1)
            {
                return;
            }
            else if (mCount == 1)
            {
                curTile = tiles[getIndxFromMoveMap('m')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            }
            else if(BCount == 1)
            {
                curTile = tiles[getIndxFromMoveMap('B')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            }
            else
                makeRemoveAPiece(check);
            smallDieWasUsed = true;
        }
        else if (BCount == 0 && mCount == 1 && MCount == 0)
        {
            alreadyRolled = false;
            curTile = tiles[getIndxFromMoveMap('m')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            alreadyRolled = true;
            if(notInHousePieces == 0)
                check = canRemoveAPiece();
            if ((MCount == 1 || BCount == 1) && check != -1)
            {
                return;
            }
            else if (MCount == 1)
            {
                curTile = tiles[getIndxFromMoveMap('M')];
                makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            }
            else if(BCount == 1)
            {
                curTile = tiles[getIndxFromMoveMap('B')];
                makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            }
            else
                makeRemoveAPiece(check);
            bigDieWasUsed = true;
        }

    }

    public void executeMove(int moveCode)
    {
        int adjust = 1;
        if (player == 1)
            adjust = -1;
        if (moveCode / 10 == 1)
        {
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = true;
            
            if(moveCode % 10 == 1)
                makeRemoveAPiece(canRemoveAPiece());
            else if(moveCode % 10 == 2)
            {
                curTile = tiles[getIndxFromMoveMap('M')];
                makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            }
            bigDieWasUsed = true;
        }
        if (moveCode / 10 == 2)
        {
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            if (moveCode % 10 == 1)
                makeRemoveAPiece(canRemoveAPiece());
            else if (moveCode % 10 == 2)
            {
                curTile = tiles[getIndxFromMoveMap('m')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            }
            smallDieWasUsed = true;
        }
        if (moveCode / 10 == 3)
        {
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            bigDieWasUsed = true;
            if (moveCode % 10 == 1)
                makeRemoveAPiece(canRemoveAPiece());
            else if (moveCode % 10 == 2)
            {
                curTile = tiles[getIndxFromMoveMap('m')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            }
            smallDieWasUsed = true;
        }
        if(moveCode == 32)
        {
            curTile = tiles[getIndxFromMoveMap('M')];
            makeMove(curTile.getIndx() + (curMoves[1] * adjust));
            curTile = tiles[getIndxFromMoveMap('m')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            smallDieWasUsed = bigDieWasUsed = true;
        }
    }

    public void makeAutomaticMovesForOneDie()
    {
        int adjust = 1;
        if (player == 1)
            adjust = -1;
        if (!bigDieWasUsed)
        {
            if (BCount == 1 && MCount == 0 && mCount == 0)
            {
                curTile = tiles[getIndxFromMoveMap('B')];
                makeMove(curTile.getIndx() + (curMoves[1] * adjust));
                bigDieWasUsed = true;
            }
            if (BCount == 0 && MCount == 1 && mCount == 0)
            {
                curTile = tiles[getIndxFromMoveMap('M')];
                makeMove(curTile.getIndx() + (curMoves[1] * adjust));
                bigDieWasUsed = true;
            }

        }
        if (!smallDieWasUsed)
        {
            if (BCount == 1 && mCount == 0 && MCount == 0)
            {
                curTile = tiles[getIndxFromMoveMap('B')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
                smallDieWasUsed = true;
            }
            if (BCount == 0 && mCount == 1 && MCount == 0)
            {
                curTile = tiles[getIndxFromMoveMap('m')];
                makeMove(curTile.getIndx() + (curMoves[0] * adjust));
                smallDieWasUsed = true;
            }
        }
    }

    public void makeAutomaticMovesForDouble(int times)
    {
        int adjust = 1;
        if (player == 1)
            adjust = -1;
        int counter = 0, t = times;
        while (BCount <= t && BCount != 0)
        {
            curTile = tiles[getIndxFromMoveMap('B')];
            makeMove(curTile.getIndx() + (curMoves[0] * adjust));
            counter++;
            t--;
        }
        if (BCount != 0)
        {
            for (int i = 0; i < counter; i++)
                undo();
            return;
        }
        bigDieWasUsed = smallDieWasUsed = true;
    }












    //information functions
    public Tile getTile(int player)
    {
        Tile t;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 mousePos2D = new Vector2(mousePos.x, mousePos.y);

        RaycastHit2D hit = Physics2D.Raycast(mousePos2D, Vector2.zero);
        if (hit.collider != null)
        {
            t = hit.transform.GetComponent<Tile>();
            return t;
        }
        return null;
    }

    public void rollDice()
    {
        rollButton.SetActive(false);
        dice[0].rollTheDice();
        dice[1].rollTheDice();
    }

    public void setRollValue(int die, int rollValue)
    {
        curMoves[die] = rollValue;
    }







    //making movements map
    public char[] makeMovesMap()
    {
        notInHousePieces = eaten[player].howManyPieces();
        char[] output = new char[24];
        for (int i = 0; i < 24; i++)
        {
            output[i] = availableMoves(tiles[i]);
            if (output[i] == 'B')
                BCount += tiles[i].howManyPieces();
            else if (output[i] == 'M')
                MCount += tiles[i].howManyPieces();
            else if (output[i] == 'm')
                mCount += tiles[i].howManyPieces();
            if (player == 0 && i < 18 && tiles[i].getColor() == player)
                notInHousePieces += tiles[i].howManyPieces();
            if (player == 1 && i > 5 && tiles[i].getColor() == player)
                notInHousePieces += tiles[i].howManyPieces();
        }
        return output;
    }

    public char makeEatenMovesMap()
    {
        curTile = getTile(player);
        int adjust = 0;
        if (player == 1)
            adjust = -23;
        if (isAvailable(tiles[Mathf.Abs(curMoves[1] + adjust - 1)]))
        {
            if (isAvailable(tiles[Mathf.Abs(curMoves[0] + adjust - 1)]))
                return 'B';
            else
                return 'M';
        }
        else
        {
            if (isAvailable(tiles[Mathf.Abs(curMoves[0] + adjust - 1)]))
                return 'm';
        }
        return 'n';
    }

    public char availableMoves(Tile t)
    {
        if (t.getColor() != player)
            return 'n';
        int bigTileIndx = makeTileIndx(t.getIndx(), Mathf.Max(curMoves[1], curMoves[0])), smallTileIndx = makeTileIndx(t.getIndx(), Mathf.Min(curMoves[1], curMoves[0]));

        if (bigTileIndx > 23 || bigTileIndx < 0)
            bigTileIndx = -1;
        if (smallTileIndx > 23 || smallTileIndx < 0)
            smallTileIndx = -1;
        if (bigTileIndx == -1 && smallTileIndx == -1)
            return 'n';
        if (bigTileIndx == -1)
        {
            if (isAvailable(tiles[smallTileIndx]))
                return 'm';
            else
                return 'n';
        }
        else
        {
            if (isAvailable(tiles[bigTileIndx]))
            {
                if (isAvailable(tiles[smallTileIndx]))
                    return 'B';
                else
                    return 'M';
            }
            if (isAvailable(tiles[smallTileIndx]))
                return 'm';
            return 'n';
        }
    }

    public bool isAvailable(Tile t)
    {
        if (t.getColor() == player || t.getColor() == -1)
            return true;
        if (t.getColor() != player && t.howManyPieces() == 1)
            return true;
        return false;
    }

    public int makeTileIndx(int cur, int die)
    {
        if (player == 0)
        {
            return cur + die;
        }
        else
        {
            return cur - die;
        }
    }

    public int getIndxFromMoveMap(char target)
    {
        int counter = 0;
        while (movesMap[counter] != target)
            counter++;
        return counter;
    }

    public void updateTheCount(char change, int sign)
    {

        if (change == 'm')
            mCount += sign;
        else if (change == 'M')
            MCount += sign;
        else if (change == 'B')
            BCount += sign;
    }








    //moving pieces on the board

    public int makeTargetIndxWhenAllInHouse(bool onlyBig, Tile curTile)
    {
        bool canMoveBigDie = false, canMoveSmallDie = false;
        int longIndx = makeTileIndx(curTile.getIndx(), Mathf.Max(curMoves[1], curMoves[0]));
        int shortIndx = makeTileIndx(curTile.getIndx(), Mathf.Min(curMoves[1], curMoves[0]));
        int adjust = 0;
        if (player == 0)
            adjust = 23;
        if (longIndx > 23 || longIndx < 0)
            canMoveBigDie = true;
        if (shortIndx > 23 || shortIndx < 0)
            canMoveSmallDie = true;
        if (!onlyBig && canMoveSmallDie && !smallDieWasUsed && (curMoves[0] == Mathf.Abs(adjust - curTile.getIndx()) + 1 || canMoveToAboutToBeDeleted(curTile.getIndx())))
        {
            updateRolls(curMoves, 'm');
            if (alreadyRolled)
                whichDie.Add('m');
            return 24;
        }
        else if (canMoveBigDie && !bigDieWasUsed && (curMoves[1] == Mathf.Abs(adjust - curTile.getIndx()) + 1 || canMoveToAboutToBeDeleted(curTile.getIndx())))
        {
            updateRolls(curMoves, 'M');
            if (alreadyRolled)
                whichDie.Add('M');
            return 24;
        }
        return -1;
    }

    public bool canMoveToAboutToBeDeleted(int curTileIndx)
    {
        int i = 0;
        if (player == 0)
        {
            while (18 + i < curTileIndx)
            {
                if (tiles[18 + i].howManyPieces() > 0 && tiles[18 + i].getColor() == 0)
                    return false;
                i++;
            }
        }
        else
        {
            while (5 - i > curTileIndx)
            {
                if (tiles[5 - i].howManyPieces() > 0 && tiles[5 - i].getColor() == 1)
                    return false;
                i++;
            }
        }

        return true;
    }

    public int makeTargetIndx(bool shortClick)
    {
        curTile = getTile(player);
        if (!curTile || (eaten[player].howManyPieces() > 0 && curTile != eaten[player]) || (bigDieWasUsed && smallDieWasUsed) || curTile.getColor() != player)
            return -1;
        int longIndx, shortIndx;
        char currMoveMap;

        if (eaten[player].howManyPieces() > 0)
        {
            currMoveMap = eatenMovesMap;
            int adjust = 0;
            if (player == 1)
                adjust = -23;
            longIndx = Mathf.Abs(curMoves[1] + adjust - 1);
            shortIndx = Mathf.Abs(curMoves[0] + adjust - 1);
        }
        else
        {
            currMoveMap = movesMap[curTile.getIndx()];
            longIndx = makeTileIndx(curTile.getIndx(), Mathf.Max(curMoves[1], curMoves[0]));
            shortIndx = makeTileIndx(curTile.getIndx(), Mathf.Min(curMoves[1], curMoves[0]));
        }
        if (notInHousePieces == 0 && shortClick)
            if (makeTargetIndxWhenAllInHouse(true, curTile) == 24)
                return 24;
        switch (currMoveMap)
        {
            case 'B':
                if ((shortClick && !bigDieWasUsed) || (!shortClick && smallDieWasUsed))
                {
                    updateRolls(curMoves, 'M');
                    if (alreadyRolled)
                        whichDie.Add('M');
                    return longIndx;
                }
                else if (!smallDieWasUsed)
                {
                    updateRolls(curMoves, 'm');
                    if (alreadyRolled)
                        whichDie.Add('m');
                    return shortIndx;
                }
                break;
            case 'M':
                if (!bigDieWasUsed)
                {
                    updateRolls(curMoves, 'M');
                    if (alreadyRolled)
                        whichDie.Add('M');
                    return longIndx;
                }
                break;
            case 'm':
                if (!smallDieWasUsed)
                {
                    updateRolls(curMoves, 'm');
                    if (alreadyRolled)
                        whichDie.Add('m');
                    return shortIndx;
                }
                break;
        }
        if (notInHousePieces == 0)
            if (makeTargetIndxWhenAllInHouse(false, curTile) == 24)
                return 24;
        return -1; // case 'n'
    }



    public void makeMove(int targetIndx)
    {
        int otherPlayer = ((player + 1) % 2), check = -1;
        if (targetIndx != -1)
        {
            if (tiles[targetIndx].getColor() == otherPlayer)
            {
                eaten[otherPlayer].addPiece(tiles[targetIndx].removePiece());
                if (alreadyRolled)
                    eatenOrigin.Add(tiles[targetIndx]);
            }
            else
                eatenOrigin.Add(null);
            tiles[targetIndx].addPiece(curTile.removePiece());
            if (alreadyRolled)
            {
                undoButton.SetActive(true);
                origin.Add(curTile);
                destination.Add(tiles[targetIndx]);
            }
            if (player == 0 && targetIndx > 17 && curTile != eaten[0] && curTile.getIndx() < 18)
                notInHousePieces--;
            if (player == 1 && targetIndx < 6 && curTile != eaten[1] && curTile.getIndx() > 5)
                notInHousePieces--;
            if (targetIndx != 24)
            {
                movesMap[targetIndx] = availableMoves(tiles[targetIndx]);
                updateTheCount(movesMap[targetIndx], 1);
            }
            if (curTile.getIndx() < 24)
            {
                updateTheCount(movesMap[curTile.getIndx()], -1);
                movesMap[curTile.getIndx()] = availableMoves(curTile);
            }
            if(notInHousePieces == 0)
                check = canRemoveAPiece();
            if ((mCount + BCount == 0 && bigDieWasUsed && check != 0) || (MCount + BCount == 0 && smallDieWasUsed && check == -1) || (mCount + MCount + BCount == 0 && check == -1))
                doneButton.SetActive(true);
        }
        curTile = null;
    }

    public void updateRolls(int[] rolls, char tar)
    {
        if (rolls[3] != 0)
            rolls[3] = 0;
        else if (rolls[2] != 0)
            rolls[2] = 0;
        else if (tar == 'M')
            bigDieWasUsed = true;
        else
            smallDieWasUsed = true;
        if (bigDieWasUsed && smallDieWasUsed)
            doneButton.SetActive(true);
    }


    public void undo()
    {
        if (curMoves[0] == curMoves[1])
        {
            if (whichDie.Count == 1)
                curMoves[3] = curMoves[0];
            else if (whichDie.Count == 2)
                curMoves[2] = curMoves[0];
            else if (whichDie.Count == 3)
                smallDieWasUsed = false;
            else
                bigDieWasUsed = false;
        }
        else
        {
            if (whichDie[whichDie.Count - 1] == 'm')
                smallDieWasUsed = false;
            else
                bigDieWasUsed = false;
        }


        origin[origin.Count - 1].addPiece(destination[destination.Count - 1].removePiece());
        if (eatenOrigin[eatenOrigin.Count - 1])
            eatenOrigin[eatenOrigin.Count - 1].addPiece(eaten[(player + 1) % 2].removePiece());

        if (destination[destination.Count - 1].getIndx() != 24)
        {
            updateTheCount(movesMap[destination[destination.Count - 1].getIndx()], -1);
            movesMap[destination[destination.Count - 1].getIndx()] = availableMoves(destination[destination.Count - 1]);
        }
        if (origin[origin.Count - 1].getIndx() < 24)
        {
            movesMap[origin[origin.Count - 1].getIndx()] = availableMoves(origin[origin.Count - 1]);
            updateTheCount(movesMap[origin[origin.Count - 1].getIndx()], 1);
        }

        if (player == 0 && origin[origin.Count - 1].getIndx() < 18 && destination[destination.Count - 1] != eaten[0] && destination[destination.Count - 1].getIndx() > 17)
            notInHousePieces++;
        if (player == 1 && origin[origin.Count - 1].getIndx() > 5 && destination[destination.Count - 1] != eaten[1] && destination[destination.Count - 1].getIndx() < 6)
            notInHousePieces++;

        whichDie.RemoveAt(whichDie.Count - 1);
        origin.RemoveAt(origin.Count - 1);
        destination.RemoveAt(destination.Count - 1);
        eatenOrigin.RemoveAt(eatenOrigin.Count - 1);

        doneButton.SetActive(false);
        if (whichDie.Count == 0)
            undoButton.SetActive(false);
    }
    public void setDiceColor()
    {
        int bigger = dice[0].GetValue() > dice[1].GetValue() ? 0 : 1;
        int smaller = (bigger + 1) % 2;
        if (curMoves[0] != -1 && curMoves[0] == curMoves[1])
        {

        }
        if (bigDieWasUsed)
            dice[bigger].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        else
            dice[bigger].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
        if (smallDieWasUsed)
            dice[smaller].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
        else
            dice[smaller].GetComponent<SpriteRenderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
    }
    public void endTurn()
    {
        curMoves[0] = curMoves[1] = curMoves[2] = curMoves[3] = 0;
        if (aboutToBeDeleted.howManyPieces() > 0)
        {
            piecesStillAlive[player] -= aboutToBeDeleted.howManyPieces();
            aboutToBeDeleted.deletePiecesInTile();
        }
    }
}