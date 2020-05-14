using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletPooling : MonoBehaviour {
	public static BulletPooling instance;

	public GameObject[] prefabBullets;

	public List<GameObject> list25;
	public List<GameObject> list40;
	public List<GameObject> list120;
	public List<GameObject> listMissile;
	public List<GameObject> listBullet;

	GameObject item;

	GameObject parent;

	void Awake()
	{
		if (instance == null) {
			instance = this;
		}

		parent = new GameObject("BulletParent");
	}
	// Use this for initialization
	void Start () {
		

		for (int i = 0; i<10; i++) {
			item = Instantiate (prefabBullets[0]) as GameObject;
			item.transform.SetParent(parent.transform);
			item.gameObject.SetActive (false);
			list25.Add (item);
		}


		for (int i = 0; i<5; i++) {
			item = Instantiate (prefabBullets[1]) as GameObject;
			item.transform.SetParent(parent.transform);
			item.gameObject.SetActive (false);
			list40.Add (item);
		}


		for (int i = 0; i<2; i++) {
			item = Instantiate (prefabBullets[2]) as GameObject;
			item.transform.SetParent(parent.transform);
			item.gameObject.SetActive (false);
			list120.Add (item);
		}

		for (int i = 0; i<2; i++) {
			item = Instantiate (prefabBullets[3]) as GameObject;
			item.transform.SetParent(parent.transform);
			item.gameObject.SetActive (false);
			listMissile.Add (item);
		}

		for (int i = 0; i < 10; i++)
		{
			item = Instantiate(prefabBullets[4]) as GameObject;
			item.transform.SetParent(parent.transform);
			item.gameObject.SetActive(false);
			listBullet.Add(item);
		}

	}
	
	public GameObject GetBullet(int index)
	{
		if (index == 0) {
			for (int i = 0; i < list25.Count; i++) {
				if (!list25 [i].activeInHierarchy)
					return list25 [i];
			}
		} else if (index == 1) {
			for (int i = 0; i < list40.Count; i++) {
				if (!list40 [i].activeInHierarchy)
					return list40 [i];
			}
		}else if (index == 2) {
			for (int i = 0; i < list120.Count; i++) {
				if (!list120 [i].activeInHierarchy)
					return list120 [i];
			}
		}else if (index == 3) {
			for (int i = 0; i < listMissile.Count; i++) {
				if (!listMissile [i].activeInHierarchy)
					return listMissile [i];
			}
		}
		else if (index == 4)
		{
			for (int i = 0; i < listBullet.Count; i++)
			{
				if (!listBullet[i].activeInHierarchy)
					return listBullet[i];
			}
		}

		item = Instantiate (prefabBullets[index]) as GameObject;
		item.transform.SetParent(parent.transform);
		item.SetActive(false);

		if (index == 0) {
			list25.Add (item);
		} else if (index == 1) {
			list40.Add (item);
		} else if (index == 2) {
			list120.Add (item);
		}else if (index == 3) {
			listMissile.Add (item);
		}
		else if (index == 4)
		{
			listBullet.Add(item);
		}

		return item;
	}
}
