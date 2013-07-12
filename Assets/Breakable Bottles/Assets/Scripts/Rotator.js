var rightSpeed:float = 10;
var upSpeed:float = 10;

function Update () {
transform.Rotate(Vector3.right*rightSpeed*Time.deltaTime);
transform.Rotate(Vector3.up*upSpeed*Time.deltaTime);
}