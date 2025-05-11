using System.Collections.Generic;
using UnityEngine;
using UnityEngine.U2D;

public class ClosedShapeBuilder
{
    public static void AddLineAsChild(Vector3[] vertices, Component parentObj)
    {
        GameObject shapeObject = new GameObject("test New Shape");
        SpriteShapeController newShapeController = shapeObject.AddComponent<SpriteShapeController>();
        SpriteShape spriteShape = ScriptableObject.CreateInstance<SpriteShape>();

        for (int i = 0; i < vertices.Length; i++)
        {
            newShapeController.spline.InsertPointAt(i, vertices[i]);
        }

        spriteShape.fillTexture = Texture2D.whiteTexture;
        newShapeController.spriteShape = spriteShape;
        newShapeController.splineDetail = 1;
        newShapeController.colliderDetail = 1;

        // add game object to canvas(?
        shapeObject.transform.parent = parentObj.transform;
    }

    public static GameObject AddShapeAsChild(Vector3[] vertices, Component parentObj, SpriteShape spriteShape = null)
    {

        GameObject shapeObject = new GameObject("test New Shape");

        SpriteShapeController newShapeController = shapeObject.AddComponent<SpriteShapeController>();

        if (spriteShape == null)
        {
            spriteShape = ScriptableObject.CreateInstance<SpriteShape>();
            spriteShape.fillTexture = Texture2D.whiteTexture;
        }

        for (int i = 0; i < vertices.Length; i++)
        {
            newShapeController.spline.InsertPointAt(i, vertices[i]);
        }
        newShapeController.spriteShape = spriteShape;
        newShapeController.splineDetail = 4;

        // add game object to parent object(?
        shapeObject.transform.parent = parentObj.transform;
        return shapeObject;
    }
}
