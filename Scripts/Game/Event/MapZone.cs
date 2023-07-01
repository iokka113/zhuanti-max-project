using UnityEngine;

public static class MapZone
{
    public static Vector2[,] pivots { get; private set; }

    /// <summary>設定地圖格的每個軸心到陣列中</summary>
    public static void Init(){
        int x = DatabaseModule.game.mapCount.x;
        int y = DatabaseModule.game.mapCount.y;
        pivots = new Vector2[x,y];
        for(int i = 0; i < pivots.GetLength(0); i++){
            for(int j = 0; j < pivots.GetLength(1); j++){
                Vector2 pivot = new Vector2(
                    i * DatabaseModule.game.mapSize.x + DatabaseModule.game.mapOrig.x,
                    j * DatabaseModule.game.mapSize.y + DatabaseModule.game.mapOrig.y);
                pivots[i,j] = pivot;
                // Debug.Log($"map grid {i}:{j} at {pivot}");
            }
        }
    }

    /// <summary>定位給定點到某個地圖格軸心</summary>
    /// <remarks>
    /// 傳回被定位到的軸心在地圖格內的座標(二維陣列的索引)<br/>
    /// 若給定點超出地圖格定義的範圍則傳回 -1<br/>
    /// 水平位置溢位 x 為 -1, 垂直位置溢位 y 為 -1<br/>
    /// </remarks>
    public static Vector2Int Locate(Vector2 position){
        Vector2Int index = -Vector2Int.one;
        if(pivots == null) return index;
        for(int i = 0; i < pivots.GetLength(0); i++){
            for(int j = 0; j < pivots.GetLength(1); j++){
                if(position.x >= pivots[i,j].x && position.x < pivots[i,j].x + DatabaseModule.game.mapSize.x){
                    index.x = i;
                }
                if(position.y >= pivots[i,j].y && position.y < pivots[i,j].y + DatabaseModule.game.mapSize.y){
                    index.y = j;
                }
                if(index.x >= 0 && index.y >= 0){
                    return index;
                }
            }
        }
        return index;
    }
}
