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
    [HideInInspector] public List<Tile> AllTiles = new List<Tile>();
    private int NumberOfTilesInLastIndex;

    public delegate void TileDelegate();
    private TileDelegate SafeTileDelegate;
    private TileDelegate DangerTileDelegate;

    public void Awake()     // each player needs this.
    {
        foreach (var TileGroup in TileLayers)
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
        if (SafeTileDelegate == null) { return; }        // no tiles to set safe
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

        for (int i = 0; i < AllTiles.Count; i++)
        {
            if (SafeIndices.Contains(i)) { continue; }      // if a safe index is detected, move to the next index;

            AllTiles[i].SetDanger();        // otherwise, alert the player that something bad will happen.
            SafeTileDelegate += AllTiles[i].SetSafe;
            DangerTileDelegate += AllTiles[i].SpawnDanger;
        }
    }

    public string DeleteTileSelection()        //
    {
        if (AllTiles.Count == 1) { return ""; }        // never remove block 0
        int NumberOfIndicesToReturn = GetNumberOfIndicesToDelete();
        string IndicesToSearch = "";
        List<int> ChosenIndices = new List<int>();                                  // the layer and index of each tile.
        for (int i = 0; i < NumberOfIndicesToReturn; i++)
        {
            var newNum = GetRandomTileToDestroy();
            Tile currentTile = Tiles[newNum[0]][newNum[1]];
            int nextNum = AllTiles.IndexOf(currentTile);
            int j = 0;
            while (ChosenIndices.Contains(nextNum) /*| nextNum == -1*/)              // if the number is observed, iterate through the list until you find a new number.
            {
                newNum = GetRandomTileToDestroy();
                currentTile = Tiles[newNum[0]][newNum[1]];
                nextNum = AllTiles.IndexOf(currentTile);
                j++;
                if (j > 100)
                {
                    Debug.LogError("Failed to find another index to destroy");
                    break;
                }
            }
            RemoveTile(newNum);
            ChosenIndices.Add(nextNum);
        }
        IndicesToSearch = string.Join(":", ChosenIndices);          // put a delimiter of ":" in between each number
        return IndicesToSearch;
    }

    private int GetNumberOfIndicesToDelete()
    {
        int NumberOfIndicesToReturn = AllTiles.Count / 64 + 1;// add one to ensure this number is never 0.
        if (AllTiles.Count >= 10)
        {
            NumberOfIndicesToReturn += Random.Range(0, 3);        // random chance for more spots if there are plenty of blocks
        }
        if (AllTiles.Count >= 50)
        {
            NumberOfIndicesToReturn += 1;
        }
        NumberOfIndicesToReturn = Mathf.Clamp(NumberOfIndicesToReturn, 1, AllTiles.Count);
        return NumberOfIndicesToReturn;
    }

    private void RemoveTile(int[] tile)
    {
        Tiles[tile[0]].RemoveAt(tile[1]);
        if(Tiles[tile[0]].Count == 0)
        {
            Tiles.RemoveAt(tile[0]);
        }
    }

    private int[] GetRandomTileToDestroy()
    {
        int LayerIndex;
        int TileIndex;
        if (Tiles.Count == 5)        // only possible with enough players.
        {
            int TargetIndex = Random.Range(Tiles.Count - 2, Tiles.Count);           // destroy any tile from the 3rd or 4th index.
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            LayerIndex = TargetIndex; TileIndex = RandomIndex;
        }
        else
        {
            int TargetIndex = Random.Range(1, Tiles.Count);         // never discard index 0.
            int RandomIndex = Random.Range(0, Tiles[TargetIndex].Count);
            LayerIndex = TargetIndex; TileIndex = RandomIndex;
        }
        int[] newTile = new int[] { LayerIndex, TileIndex };
        return newTile;
    }
    public void SpawnDanger()
    {
        if(DangerTileDelegate != null)
            DangerTileDelegate.Invoke();
        DangerTileDelegate = null;
    }

    public void DeleteTile(string fallingIndices)           // called by the client and server
    {
        string[] DangerChar = fallingIndices.Split(':');
        List<Tile> FallingIndices = new List<Tile>();
        foreach (string val in DangerChar)
        {
            int.TryParse(val, out int intValue);
            FallingIndices.Add(AllTiles[intValue]);
        }

        foreach (Tile tile in FallingIndices)              // clients only have control over their own local version of all tiles.
        {
            tile.DiscardTile();
            AllTiles.Remove(tile);
        }
    }

    public void TryToSpawnGuardedTile()
    {
        if(AllTiles.Count == 1) { return; }             // do not spawn guarded tiles anymore

        for(int i = 0; i < 5; i++)      // try to a guarded tile at least 5 times. if no successes, fail to spawn a guarded tile.
        {
            var GuardedTileIndex = Random.Range(0, AllTiles.Count);
            bool Occupied = AllTiles[GuardedTileIndex].GetComponentInChildren<AreaEffector>().CheckForPlacements();
            if (!Occupied)      // if the space is not occupied, spawn a guarded tile.
            {
                AllTiles[GuardedTileIndex].GetComponentInChildren<AreaEffector>().PlaceDownGuardedTile(BoltPrefabs.GuardedTile);       // can input any generic guarded tile in the future.
                break;
            }
        }
    }

    public void SpawnItems(Bolt.PrefabId[] ItemsToSpawn)            // spawn items during game
    {
        foreach(Bolt.PrefabId item in ItemsToSpawn)
        {
            Transform tile = AllTiles[Random.Range(0, AllTiles.Count)].transform;
            BoltNetwork.Instantiate(item, tile.position + new Vector3(0, 2, 0), Quaternion.identity);
        }
    }

}
