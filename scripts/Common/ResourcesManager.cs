using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResourcesManager
{
    //�ֵ�
    private static Dictionary<string,Sprite> nameSpriteDic = new Dictionary<string,Sprite>();
    /// <summary>
    /// ��ȡͼ��
    /// </summary>
    public static Sprite GetSprite(string iconName)
    {
        if (nameSpriteDic.ContainsKey(iconName))
        {
            return nameSpriteDic[iconName];
        }
        else
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("headIcon");
            string[] nameArr = iconName.Split('_');
            Sprite temp = sprites[int.Parse(nameArr[1])];
            nameSpriteDic.Add(iconName, temp);
            return temp;
        }
        
    }
    /// <summary>
    /// �����Ƶ�ͼ��
    /// </summary>
    /// <param name="cardName"></param>
    /// <returns></returns>
    public static Sprite LoadCardSprite(string cardName)
    {
        if (nameSpriteDic.ContainsKey(cardName))
        {
            return nameSpriteDic[cardName];
        }
        else
        {
            Sprite temp = Resources.Load<Sprite>("poke/"+ cardName);
            nameSpriteDic.Add(cardName, temp);
            return temp;
        }
    }
}
