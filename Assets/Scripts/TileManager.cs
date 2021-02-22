using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileManager : MonoBehaviour
{
    /// <summary>
    /// This script manages the warning and danger phase for each tile.
    /// </summary>
    /// 
    public static TileManager instance;
    public List<GameObject> TileLayers = new List<GameObject>();
    private List<List<Tile>> Tiles = new List<List<Tile>>();
    private List<Tile> AllTiles = new List<Tile>();
    private int NumberOfTilesInLastIndex;

    public delegate void TileDelegate();
    private TileDelegate SafeTileDelegate;
    private TileDelegate DangerTileDelegate;
    
    public void Awake()     // each player needs this.
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
        instance = this;
    }

    public void SetTilesSafe()
    {
        if(SafeTileDelegate == null) { return; }        // no tiles to set safe
        SafeTileDelegate.Invoke();
        SafeTileDelegate = null;
    }


    public string SelectIndicesToMarkSafe()               // selects indices fo keep Safe.
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

    public void WarnPlayers(string SafePieces)
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
            SafeTileDelegate += AllTiles[i].SetSafe;
            DangerTileDelegate += AllTiles[i].SpawnDanger;
        }
    }

    public int[] DeleteTileSelection()        //
    {
        int[] TileSelection = new int[2] { 0, 0 };
        if(Tiles.Count == 1) { return TileSelection; }
        if(Tiles.Count == 5)        // only possible with enough players.
        {
            int TargetIndex = Random.Range(Tiles.Count-2, Tiles.Count);           // destroy any tile from the 3rd or 4th index.
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            TileSelection[0] = TargetIndex; TileSelection[1] = RandomIndex; return TileSelection;
        }
        else
        {
            int TargetIndex = Random.Range(1, Tiles.Count);         // never discard index 0.
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            TileSelection[0] = TargetIndex; TileSelection[1] = RandomIndex; return TileSelection;
        }

    }

    public void SpawnDanger()
    {
        if(DangerTileDelegate != null)
            DangerTileDelegate.Invoke();
        DangerTileDelegate = null;
    }

    public void DeleteTile(int LayerIndex, int PlatformIndex)
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
