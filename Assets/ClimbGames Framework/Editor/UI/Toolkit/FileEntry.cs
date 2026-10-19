using System;
using System.IO;
using UnityEngine;

namespace ClimbGames.Editor.UI
{
    public class FileEntry
    {
        public int id;
        public Texture icon;
        public string name;
        public string extension;
        public string path;
        public bool isDirectory;
        public long size;
        public DateTime modified;

        public FileEntry(int id, string path)
        {
            this.id = id;
            this.path = path;
            isDirectory = Directory.Exists(path);

            try
            {
                if (isDirectory)
                {
                    icon = GUIContents.Folder_Icon.image;
                    name = Path.GetFileName(path);
                    modified = Directory.GetLastWriteTime(path);
                }
                else
                {
                    var info = new FileInfo(path);

                    name = info.Name;
                    extension = info.Extension;
                    size = info.Length;
                    modified = info.LastWriteTime;

                    switch (extension)
                    {
                        case ".prefab": icon = GUIContents.d_Prefab_Icon?.image; break;
                        case ".asset": icon = GUIContents.ScriptableObject_Icon?.image; break;
                        case ".xlsx":
                        case ".xls": icon = GUIContents.UxmlScript_Icon?.image; break;
                        default: icon = GUIContents.DefaultAsset_Icon?.image; ; break;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"[FileEntry] {ex}");
            }
        }
    }
}