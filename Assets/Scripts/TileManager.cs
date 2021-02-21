using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    public static TileManager instance;
    public List<GameObject> TileLayers = new List<GameObject>();
    private List<List<Tile>> Tiles = new List<List<Tile>>();
    private List<Tile> AllTiles = new List<Tile>();
    private int NumberOfTilesInLastIndex;

    public void Awake()
    {
        foreach(var TileGroup in TileLayers)
        {
            List<Tile> newTiles = new List<Tile>();
            foreach (Transform tile in TileGroup.transform)
            {
                NumberOfTilesInLastIndex++;
                newTiles.Add(tile.GetComponent<Tile>());
                AllTiles.Add(tile.GetComponent<Tile>());
            }
            Tiles.Add(newTiles);
        }
        CallDanger();
        // StartCoroutine(Tester());
        instance = this;
    }

    private void CallDanger()
    {
        string newList = SelectIndices();
        print(newList);
        Tester2(newList);
        Invoke("CancelDanger", 3f);
    }

    private void CancelDanger()
    {
        for(int i = 0; i < AllTiles.Count; i++)
        {
            AllTiles[i].SetSafe();
        }
        Invoke("Tester", 2f);
    }
    public string SelectIndices()
    {

        int NumberOfIndicesToReturn = AllTiles.Count / 7 + 1;// add one to ensure this number is never 0.
        if (AllTiles.Count >= 8)
        {
            NumberOfIndicesToReturn += Random.Range(0, 3);        // random chance for more spots if there are plenty of blocks
        }

        NumberOfIndicesToReturn = Mathf.Clamp(NumberOfIndicesToReturn, 1, AllTiles.Count);
        string IndicesToSearch = "";
        List<int> ChosenIndices = new List<int>();
        for (int i = 0; i < NumberOfIndicesToReturn; i++)
        {
            var newNum = Random.Range(0, AllTiles.Count);

            while (ChosenIndices.Contains(newNum))              // if the number is observed, iterate through the list until you find a new number.
            {
                newNum = Random.Range(0, AllTiles.Count);
            }
            ChosenIndices.Add(newNum);
        }
        IndicesToSearch = string.Join(":", ChosenIndices);          // put a delimiter of ":" in between each number
        return IndicesToSearch;
    }
    private void Tester2(string SafePieces = "1:2:3:4:5:6:7:8:9:10")
    {
        string[] SafeChar = SafePieces.Split(':');
        List<int> SafeIndices = new List<int>();
        foreach (string val in SafeChar)
        {
            int.TryParse(val, out int intValue);
            SafeIndices.Add(intValue);
        }

        for(int i = 0; i < AllTiles.Count; i++)
        {
            if (SafeIndices.Contains(i)) { continue; }      // if a safe index is detected, move to the next index;

            AllTiles[i].SetDanger();        // otherwise, alert the player that something bad will happen.
        }
    }
    private void Tester()        //
    {

        if(Tiles.Count == 1) { return; }
        if(Tiles.Count == 5)        // only possible with enough players.
        {
            int TargetIndex = Random.Range(Tiles.Count-2, Tiles.Count);           // destroy any tile from the 3rd or 4th index.
            Debug.LogWarning(TargetIndex);
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            DeleteTile(TargetIndex, RandomIndex);
        }
        else
        {
            int TargetIndex = Random.Range(1, Tiles.Count);         // never discard index 0.
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            DeleteTile(TargetIndex, RandomIndex);
        }

        Invoke("CallDanger", 2f);
    }

    private void DeleteTile(int LayerIndex, int PlatformIndex)
    {
        Tiles[LayerIndex][PlatformIndex].DiscardTile();
        AllTiles.Remove(Tiles[LayerIndex][PlatformIndex]);
        Tiles[LayerIndex].RemoveAt(PlatformIndex);
        if (Tiles[LayerIndex].Count == 0)
        {
            Tiles.RemoveAt(LayerIndex);     // remove any layers that have no elements left.
        }
    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
