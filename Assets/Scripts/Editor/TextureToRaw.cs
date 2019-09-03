using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using UnityEditor;
using System.IO;

public class TextureToRaw : EditorWindow
{
    Texture2D texture = null;

    [MenuItem("Tools/Texture2Raw")]
    static void Init()
    {
        TextureToRaw window = (TextureToRaw)GetWindow(typeof(TextureToRaw));
        window.Show();
    }

    void OnGUI()
    {
        GUI.enabled = true;

        GUILayout.Label("Input Texture", EditorStyles.boldLabel);
        texture = EditorGUILayout.ObjectField("Input Texture", texture, typeof(Texture2D), false) as Texture2D;

        GUI.enabled = texture == null ? false : true;

        if (GUILayout.Button("Convert to Raw and Save"))
        {
            var path = EditorUtility.SaveFilePanel(
                "Save texture as RAW",
                "",
                texture.name + ".raw",
                "raw");

            if (path.Length != 0)
            {
                Color[] pixels = texture.GetPixels();
                byte[] bytes = new byte[pixels.Length*2];
                int index = 0;
				for (int u = texture.width - 1; u >= 0 ; --u)
				{
					for (int v = 0; v < texture.height ; ++v)
					{
						int i = u * texture.height + v;

						float buffer = (pixels[i].r * 65535);
						System.Int16 i16Buffer = (System.Int16)buffer;
						byte[] byteBuffer = shortToByte(i16Buffer);
						bytes[index++] = byteBuffer[0];
						bytes[index++] = byteBuffer[1];
					}
				}

                File.WriteAllBytes(path, bytes);
            }
        }

        GUI.enabled = true;
    }

	public byte[] shortToByte(short a)
	{
		byte[] shortToByte = new byte[2];
		shortToByte[0] |= (byte)((a & 0xFF00) >> 8);
		shortToByte[1] |= (byte)(a & 0xFF & 0xff);
		return shortToByte;

	}
}
