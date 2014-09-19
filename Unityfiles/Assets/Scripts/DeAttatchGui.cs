using UnityEngine;
using System.Collections;

public class DeAttatchGui : MonoBehaviour
{
	void Start()
	{
		transform.parent=null;
		transform.position=new Vector3(0, 1, 0);
	}
}
