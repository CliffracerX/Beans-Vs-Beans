using UnityEngine;
using System.Collections;

public class RandomTreeRotation : MonoBehaviour
{
	void Start()
	{
		this.transform.rotation = new Quaternion (0, transform.position.x * transform.position.y * transform.position.z, 0, transform.rotation.w);
	}
}
