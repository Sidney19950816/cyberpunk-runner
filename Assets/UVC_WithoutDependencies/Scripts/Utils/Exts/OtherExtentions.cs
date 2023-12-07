using UnityEngine;

public static class OtherExtentions
{

	public static void SetActive (this Component obj, bool value)
	{
		obj.gameObject.SetActive (value);
	}

    public static void SetLayerRecursively (this GameObject obj, int layer)
    {
        obj.layer = layer;

        for (var i = 0; i < obj.transform.childCount; i++)
        {
            SetLayerRecursively (obj.transform.GetChild(i).gameObject, layer);
        }
    }
}
