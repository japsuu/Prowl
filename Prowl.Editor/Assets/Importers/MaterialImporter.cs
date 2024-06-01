﻿using Hexa.NET.ImGui;
using Prowl.Editor.ScriptedEditors;
using Prowl.Runtime;
using Prowl.Runtime.Utils;

namespace Prowl.Editor.Assets
{

    [Importer("FileIcon.png", typeof(Material), ".mat")]
    public class MaterialImporter : ScriptedImporter
    {
        public override void Import(SerializedAsset ctx, FileInfo assetPath)
        {
            // Load the Texture into a TextureData Object and serialize to Asset Folder
            Material? mat;
            try
            {
                string json = File.ReadAllText(assetPath.FullName);
                var tag = StringTagConverter.Read(json);
                mat = Serializer.Deserialize<Material>(tag);
            }
            catch
            {
                // something went wrong, lets just create a new material and save it
                mat = new Material();
                string json = StringTagConverter.Write(Serializer.Serialize(mat));
                File.WriteAllText(assetPath.FullName, json);
            }

            ctx.SetMainObject(mat);
        }
    }

    [CustomEditor(typeof(MaterialImporter))]
    public class MaterialImporterEditor : ScriptedEditor
    {
        public override void OnInspectorGUI()
        {
            var importer = (MaterialImporter)(target as MetaFile).importer;

            try
            {
                var tag = StringTagConverter.ReadFromFile((target as MetaFile).AssetPath);
                Material mat = Serializer.Deserialize<Material>(tag);

                MaterialEditor editor = new MaterialEditor(mat, () => {
                    StringTagConverter.WriteToFile(Serializer.Serialize(mat), (target as MetaFile).AssetPath);
                    AssetDatabase.Reimport((target as MetaFile).AssetPath);
                });
                editor.OnInspectorGUI();
            }
            catch (Exception e)
            {
                g.Node("DummyForText").ExpandWidth().Height(GuiStyle.ItemHeight * 10);
                g.DrawText("Failed to Deserialize Material: " + e.Message + "\n" + e.StackTrace, g.CurrentNode.LayoutData.Rect);
            }
        }
    }

}
