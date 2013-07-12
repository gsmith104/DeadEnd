/* 	Respawn script 1.01
	(C) 2012 Unluck Software
	http://www.chemicalbliss.com 

Changelog:
v1.01
Added scale buffer

*/
var target:Transform;
var replace:Transform;
private var posBuffer:Vector3;
private var rotBuffer:Quaternion;
private var scaleBuffer:Vector3;


function Start (){
	scaleBuffer=target.localScale;
	posBuffer=target.position;
	rotBuffer=target.rotation;
}

function Update () {
	if (target == null){
   		var pos:Vector3 = posBuffer;
    	if(replace!=null)
  		target= gameObject.Instantiate(replace, pos, rotBuffer);
 		target.localScale = scaleBuffer;
	}
}