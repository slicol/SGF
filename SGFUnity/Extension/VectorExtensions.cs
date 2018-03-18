using UnityEngine;


public static class VectorExtensions
{
    public static Vector3 To3DXZ(this Vector2 vector, float y)
    {
        return new Vector3(vector.x, y, vector.y);
    }

    public static Vector3 To3DXZ(this Vector2 vector)
    {
        return vector.To3DXZ(0);
    }

    public static Vector3 To3DXY(this Vector2 vector, float z)
    {
        return new Vector3(vector.x, vector.y, z);
    }

    public static Vector3 To3DXY(this Vector2 vector)
    {
        return vector.To3DXY(0);
    }

    public static Vector3 To3DYZ(this Vector2 vector, float x)
    {
        return new Vector3(x, vector.x, vector.y);
    }

    public static Vector3 To3DYZ(this Vector2 vector)
    {
        return vector.To3DYZ(0);
    }

    public static Vector2 To2DXZ(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.z);
    }

    public static Vector2 To2DXY(this Vector3 vector)
    {
        return new Vector2(vector.x, vector.y);
    }

    public static Vector2 To2DYZ(this Vector3 vector)
    {
        return new Vector2(vector.y, vector.z);
    }

    /**
        Returns the vector rotated 90 degrees counter-clockwise. This vector is
        always perpendicular to the given vector.

        The perp dot product can be caluclted using this:
            var perpDotPorpduct = Vector2.Dot(v1.Perp(), v2);
    */
    public static Vector2 Perp(this Vector2 vector)
    {
        return new Vector2(-vector.y, vector.x);
    }

    /**
        Returns the projection of this vector onto the given base.
    */
    public static Vector2 Proj(this Vector2 vector, Vector2 baseVector)
    {
        var direction = baseVector.normalized;
        var magnitude = Vector2.Dot(vector, direction);

        return direction * magnitude;
    }

    /**
        Returns the rejection of this vector onto the given base.

        The sum of a vector's projection and rejection on a base is
        equal to the original vector.
    */
    public static Vector2 Rej(this Vector2 vector, Vector2 baseVector)
    {
        return vector - vector.Proj(baseVector);
    }

    /**
        Returns the projection of this vector onto the given base.
    */
    public static Vector3 Proj(this Vector3 vector, Vector3 baseVector)
    {
        var direction = baseVector.normalized;
        var magnitude = Vector2.Dot(vector, direction);

        return direction * magnitude;
    }

    /**
        Returns the rejection of this vector onto the given base.

        The sum of a vector's projection and rejection on a base is
        equal to the original vector.
    */
    public static Vector3 Rej(this Vector3 vector, Vector3 baseVector)
    {
        return vector - vector.Proj(baseVector);
    }

    /**
        Returns the projection of this vector onto the given base.
    */
    public static Vector4 Proj(this Vector4 vector, Vector4 baseVector)
    {
        var direction = baseVector.normalized;
        var magnitude = Vector2.Dot(vector, direction);

        return direction * magnitude;
    }

    /**
        Returns the rejection of this vector onto the given base.

        The sum of a vector's projection and rejection on a base is
        equal to the original vector.
    */
    public static Vector4 Rej(this Vector4 vector, Vector4 baseVector)
    {
        return vector - vector.Proj(baseVector);
    }

    public static Vector3 PerpXZ(this Vector3 v)
    {
        return new Vector3(-v.z, v.y, v.x);
    }

    public static Vector3 PerpXY(this Vector3 v)
    {
        return new Vector3(-v.y, v.x, v.z);
    }



    public static string GetStringValue(this Vector3 v)
    {
        return v.x + "," + v.y + "," + v.z;
    }

    public static void SetStringValue(this Vector3 v, string strValue)
    {
        string[] tmp = strValue.Split(',');
        if (tmp.Length >= 3)
        {
            float x = v.x;
            float y = v.y;
            float z = v.z;
            float.TryParse(tmp[0], out x);
            float.TryParse(tmp[1], out y);
            float.TryParse(tmp[2], out z);

            v.x = x;
            v.y = y;
            v.z = z;
        }
    }
}
