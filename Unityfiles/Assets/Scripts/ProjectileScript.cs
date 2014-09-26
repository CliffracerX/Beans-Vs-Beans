using UnityEngine;
using System.Collections;

public class ProjectileScript : MonoBehaviour
{
	public int fadeTime = 500;
	public int damageDone = 5;
	void FixedUpdate()
	{
		if(fadeTime>0)
			fadeTime--;
		if(fadeTime<=0)
			Destroy(this.gameObject);
	}
	
	[RPC] void DestroyO()
	{
		Destroy(this.gameObject);
	}

	void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 col = Vector3.down;
		int damageD = 0;
		if(stream.isWriting)
		{
			col.x=gameObject.transform.GetChild(0).renderer.material.color.r;
			col.y=gameObject.transform.GetChild(0).renderer.material.color.g;
			col.z=gameObject.transform.GetChild(0).renderer.material.color.b;
			stream.Serialize(ref col);
			damageD = damageDone;
			stream.Serialize(ref damageD);
		}
		else
		{
			stream.Serialize(ref col);
			gameObject.transform.GetChild(0).renderer.material.color=new Color(col.x, col.y, col.z);
			stream.Serialize(ref damageD);
			damageDone=damageD;
		}
	}
}
