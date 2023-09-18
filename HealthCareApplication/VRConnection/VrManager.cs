﻿using System.Text.Json;
using System.Text.Json.Nodes;

namespace VRConnection;

/// <summary>
/// Manage the VR scene through JSON messages
/// Messages are created through Formatting.cs and sends through TunnelHandler.cs
/// </summary>
public class VrManager
{
    /// <summary>
    /// Attempt to create a tunnel/connection through which VR commands are send through
    /// </summary>
    /// <param name="tunnelHandler"> handles socket connection with VR server </param>
    public VrManager(TunnelHandler tunnelHandler)
    {
        _tunnelHandler = tunnelHandler;
        tunnelHandler.CreateTunnel();
        if (tunnelHandler.TunnelId == "")
        {
            Console.WriteLine("Failed to create tunnel.");
        }
    }

    private readonly TunnelHandler _tunnelHandler;

    /// <summary>
    /// Send a request to VR server and retrieve the VR scene data
    /// </summary>
    public void GetScene()
    {
        object sceneGetCommand = Formatting.SceneGet();
        // TODO create separate tunnel message
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, sceneGetCommand);
        _tunnelHandler.SendMessage(tunnelMessage);
    }

    /// <summary>
    ///  Add a model in VR scene
    /// </summary>
    /// <param name="name">name of node</param>
    /// <param name="position">position array containing x, y, z</param>
    /// <param name="scale">ses scaling of model</param>
    /// <param name="fileName"> filepath of the obj file of the model</param>
    public void AddModel(string name, int[] position, double scale, string fileName)
    {
        // TODO add position data
        object modelAddCommand = Formatting.Add3DObject(name, position, scale, fileName);
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, modelAddCommand);
        Console.WriteLine(tunnelMessage);

        _tunnelHandler.SendMessage(tunnelMessage);
    }


    /// <summary>
    ///  Add a model in VR scene
    /// </summary>
    /// <param name="name">name of node</param>
    /// <param name="position">position array containing x, y, z</param>
    /// <param name="scale">ses scaling of model</param>
    /// <param name="fileName"> filepath of the obj file of the model</param>
    /// <param name="animationName"> filepath of the animation file</param>
    public void AddAnimatedModel(string name, int[] position, double scale, string fileName, string animationName)
    {
        // TODO add position data
        // TODO add position data
        object modelAddCommand = Formatting.AddAnimatedObject(name, position, scale, fileName, animationName);
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, modelAddCommand);

        _tunnelHandler.SendMessage(tunnelMessage);
    }

    /// <summary>
    /// Add terrain data to VR scene
    /// Since its only positional data, no visual component is rendered in the scene
    /// </summary>
    /// <param name="size"></param>
    /// <param name="heightMap"></param>
    public void AddTerrain(int[] size, float[] heightMap)
    {
        object terrainAddCommand = Formatting.TerrainAdd(size, heightMap);
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, terrainAddCommand);

        _tunnelHandler.SendMessage(tunnelMessage);
    }

    /// <summary>
    /// Add terrain node to VR scene
    /// Visual component (layers) can be added after this
    /// </summary>
    public void AddTerrainNode()
    {
        object addTerrainNodeCommand = Formatting.TerrainAddNode();
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, addTerrainNodeCommand);
        Console.WriteLine(tunnelMessage);
        _tunnelHandler.SendMessage(tunnelMessage);
    }

    /// <summary>
    /// Add visual component to terrain
    /// Requires actual node to add visual component
    /// </summary>
    public void AddTerrainLayer()
    {
        // command data
        string terrainId = GetNodeId("terrain");
        string diffuseFilePath = "data\\NetworkEngine\\textures\\grass_diffuse.png";
        string normalFilePath = "data\\NetworkEngine\\textures\\grass_normal.png";

        // create command and send to VR
        object addTerrainLayerCommand = Formatting.TerrainAddLayer(terrainId, diffuseFilePath, normalFilePath);
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, addTerrainLayerCommand);
        _tunnelHandler.SendMessage(tunnelMessage);
    }

    /// <summary>
    /// Get uuid from node based on name
    /// </summary>
    /// <param name="name">name of the requested node</param>
    /// <returns>uuid as string</returns>
    public string GetNodeId(string name)
    {
        object sceneFindNodeCommand = Formatting.SceneNodeFind(name);
        object tunnelMessage = Formatting.TunnelSend(_tunnelHandler.TunnelId, sceneFindNodeCommand);

        _tunnelHandler.SendMessage(tunnelMessage);
        var response = _tunnelHandler.ReadJsonObject(); // can also use ReadString()
        // when no parsing is required

        // TODO move to separate method (call it getNode())
        // <--
        var nodes = response?["data"]?["data"]?["data"]?.AsArray();

        string uuid = string.Empty;
        if (nodes != null)
        {
            var node = nodes.First(); // only need one node with the name needed
            uuid = node?["uuid"]?.ToString() ?? string.Empty;
        }
        // -->

        return uuid;
    }
}