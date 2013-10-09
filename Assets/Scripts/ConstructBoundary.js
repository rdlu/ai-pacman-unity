var mapAsset : TextAsset;
var blockPrefab : Transform;
var pelletPrefab : Transform;
var superPrefab : Transform;

function Awake () {
    var map = mapAsset.text.Split ("\n"[0]);
    var v = new Vector3 ();
    v.y = 1.0;
    var j_off = map.length / 2.0;
    for (var j = 0; j < map.length; j ++) {
        v.z = (map.length - j - j_off - 1) * 2;
        var i_off = map[j].length / 2.0;
        for (var i = 0; i < map[j].length; i ++) {
            v.x = (i - i_off) * 2 + 1;
            if (map[j][i] == "X") {
                var inst = Instantiate (blockPrefab, v, Quaternion.identity);
                inst.transform.parent = transform;
            } else if (map[j][i] == ".") {
                Instantiate (pelletPrefab, v, Quaternion.identity);
            } else if (map[j][i] == "O") {
                Instantiate (superPrefab, v, Quaternion.identity);
            }
        }
    }
}
