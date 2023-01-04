using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

// left is 1, right is 2 and 0 is not in scope
//color true is left blue is false
public class iscararround : MonoBehaviour
{
	[SerializeField] private Transform target;
	[SerializeField] private Transform source;
	[SerializeField] private float controllradius;
	[SerializeField] private UnityEngine.UI.Text text;
	[SerializeField] private bool color;
	
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (wrongturn()){
			int b = int.Parse(text.text)+1;
			text.text = b.ToString();
		}

        
    }
    
    private bool wrongturn(){
		int inside = isinside();
		if ((inside == 1 && color == false) || (inside == 2 && color == true)){
			return true;
		}
		return false;
	}
    
	
	private int isinside(){
		Debug.Log(color);
		if (target.position.z == source.position.z){
		if (target.position.x-source.position.x < 0 &&(Math.Abs(target.position.x-source.position.x) < controllradius)){
				Debug.Log("Is inside first if");
			return 1;
		}else if (target.position.x-source.position.x < controllradius){
				Debug.Log("Is inside first else");
				return 2;
		}else{
			return 0;
		}
		
		}else{
			return 0;
		}
		
	}
}
