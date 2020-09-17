using UnityEngine;
using System;

public static class DateTimeExtensions
{
    public static DateTime StartOfWeek(this DateTime dt, DayOfWeek startOfWeek)
    {
        int diff = (7 + (dt.DayOfWeek - startOfWeek)) % 7;
        return dt.AddDays(-1 * diff).Date;
    }
}

public static class ImageHelpers
{
    public static Color overrideColor = Color.red;

    public static Texture2D AlphaBlend(this Texture2D aBottom,
        Texture2D aTop,
        bool customColor = false)
    {
        if (aBottom.width != aTop.width || aBottom.height != aTop.height)
            throw new System.InvalidOperationException("AlphaBlend only works with two equal sized images");
        var bData = aBottom.GetPixels();
        var tData = aTop.GetPixels();
        int count = bData.Length;
        var rData = new Color[count];
        for (int i = 0; i < count; i++)
        {
            Color B = bData[i];
            Color T = tData[i];

            if(customColor)
            {
                if (T.a > 0.5f)
                {
                    T = overrideColor;
                }
            }

            float srcF = T.a;
            float destF = 1f - T.a;
            float alpha = srcF + destF * B.a;
            Color R = (T * srcF + B * B.a * destF) / alpha;
            R.a = alpha;
            rData[i] = R;
        }
        var res = new Texture2D(aTop.width, aTop.height);
        res.SetPixels(rData);
        res.Apply();
        return res;
    }

    public static Texture2D CroppedTextureFromSprite(Sprite sprite)
    {
        Texture2D croppedTexture = new Texture2D((int)sprite.rect.width, (int)sprite.rect.height);

        Color[] pixels = sprite.texture.GetPixels((int)sprite.rect.x,
                                         (int)sprite.rect.y,
                                         (int)sprite.rect.width,
                                         (int)sprite.rect.height);

        croppedTexture.SetPixels(pixels);
        croppedTexture.Apply();

        return croppedTexture;
    }

    public static bool IsTransparent(Texture2D tex)
    {
        for (int x = 0; x < tex.width; x++)
            for (int y = 0; y < tex.height; y++)
                if (tex.GetPixel(x, y).a != 0)
                    return false;
        return true;
    }
}