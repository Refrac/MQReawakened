﻿using AssetStudio;
using Server.Reawakened.BundleHost.Models;
using System.Text;
using System.Xml;

namespace Server.Reawakened.BundleHost.Extensions;

public static class GetData
{
    public static string GetXmlData(this InternalAssetInfo asset)
    {
        var file = File.ReadAllText(asset.Path);

        if (file.FirstOrDefault() == '<')
            try
            {
                new XmlDocument().LoadXml(file);
                return file;
            }
            catch (XmlException)
            {
            }

        var manager = new AssetsManager();

        manager.LoadFiles(asset.Path);

        var textAsset = manager.assetsFileList.First().ObjectsDic.Values.ToArray().GetText(asset.Name);

        var text = Encoding.UTF8.GetString(textAsset.m_Script);

        return text;
    }
}
