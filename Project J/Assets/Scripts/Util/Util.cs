using System.IO;
using UnityEngine;

public partial class Util
{
    public static char[] PATH_SEPERATOR = new char[] { '/', '\\' };

    public static void UpdateResolutionToCamera(Camera cam, float nPerWidth, float nPerHeight)
    {
        float resolutionX = Screen.width / nPerWidth;
        float resolutionY = Screen.height / nPerHeight;

        if (resolutionX < resolutionY)
        {
            if (null == cam.targetTexture)
            {
                float origin = cam.orthographicSize;
                cam.orthographicSize = (resolutionY / resolutionX) * origin;
            }
        }
    }

    //특정 Tag name object를 picking 하는 함수
    public static GameObject RayCastTagObject(string tagName, float rayLength)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayLength))
        {
            if (hit.collider.tag == tagName)
                return hit.collider.gameObject;
        }

        return null;
    }

    //특정 Layer이름을 가지는 Object를 picking하는 함수
    public static bool RayCastLayerObject(string LayerName, float rayLength, ref GameObject pickedObject)
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, rayLength, LayerMask.NameToLayer(LayerName)))
        {
            pickedObject = hit.collider.gameObject;
            return true;
        }

        return false;
    }

    public static string GetPriceStringFromValue(int value)
    {
        return string.Format("{0:#,###0}", value);
    }

    public static void DeleteFilesByExtension(string dirPath, string extension)
    {
        DirectoryInfo dirInfo = new DirectoryInfo(dirPath);
        FileInfo[] fileInfos;

        string targetExt = string.Format("*.{0}", extension);
        fileInfos = dirInfo.GetFiles(targetExt, SearchOption.AllDirectories);
        if (fileInfos.Length <= 0)
            return;

        for (int i = 0; i < fileInfos.Length; ++i)
        {
            //만약 ReadOnly 속성이 있는 파일이 있다면 지울때 에러가 나므로 속성을 Normal로 바꿔 놓는다.
            if (fileInfos[i].Attributes == FileAttributes.ReadOnly)
                fileInfos[i].Attributes = FileAttributes.Normal;

            fileInfos[i].Delete();
        }
    }
}