using System.Drawing;
using System.Numerics;
using System.Text.Json.Nodes;
using System.Xml.Linq;
using VRConnection.Communication;
using VRConnection.Graphics;

namespace VRConnection;

public class VrSession
{
    private string? _sessionId;
    private string? _tunnelId;

    #region Connectivity
    /// <summary>
    ///     Initialize the connection by receiving the session ID of the NetworkEngine running on the same
    ///     computer that is running the application, and the tunnel ID
    /// </summary>
    /// <returns>Wether a successful connection was established to the server.</returns>
    public async Task Initialize(string serverIpAddress, int serverPort)
    {
        // If the application could not connect to the server
        if (!await VrCommunication.ConnectToServer(serverIpAddress, serverPort))
        {
            throw new CommunicationException(
                $"Could not connect to address {serverIpAddress} on port {serverPort}. Maybe there is an already active session?");
        }

        // Request session list
        await VrCommunication.SendAsJson(Formatting.SessionListGet());

        // Receive and process the session ID of the running NetworkEngine instance
        JsonObject sessionListResponse = await VrCommunication.ReceiveJsonObject();
        _sessionId = Formatting.ValidateAndGetSessionId(sessionListResponse);

        // Request adding a new tunnel
        await VrCommunication.SendAsJson(Formatting.TunnelCreate(_sessionId));

        // Receive and process the tunnel ID of the running NetworkEngine instance
        JsonObject tunnelCreateResponse = await VrCommunication.ReceiveJsonObject();
        _tunnelId = Formatting.ValidateAndGetTunnelId(tunnelCreateResponse);
    }

    /// <summary>
    /// Close the connection with the VR server
    /// </summary>
    public void Close() => VrCommunication.CloseConnection();
    #endregion

    #region Scene
    /// <summary>
    ///     Send a request to VR server and retrieve the VR scene data
    /// </summary>
    public async Task<JsonObject> GetScene()
    {
        var sceneGetCommand = Formatting.SceneGet();
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, sceneGetCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Reset the VR scene to default
    /// </summary>
    public async Task<JsonObject> ResetScene()
    {
        var sceneResetCommand = Formatting.SceneReset();
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, sceneResetCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    #endregion

    #region Nodes
    /// <summary>
    ///     Get uuid from node based on name
    /// </summary>
    /// <param name="name">name of the requested node</param>
    /// <returns>uuid as string</returns>
    public async Task<string> GetNodeId(string name)
    {
        JsonObject response = await RequestNodesWithName(name);
        var responseAsArray = response?["data"]?["data"]?["data"];

        // If there were multiple nodes with the same name, get the first one's id
        if (responseAsArray is JsonArray)
        {
            if (responseAsArray == null)
                throw new CommunicationException($"Could not retrieve nodes with name {name} from response.");
            return GetFirstNode(responseAsArray.AsArray());
        }

        // Response is just a single object, so retrieve value as string
        string? responseAsObject = response?["data"]?["id"]?.GetValue<string>();
        if (responseAsObject == null)
            throw new CommunicationException($"Could not retrieve the node with name {name} from response.");

        return responseAsObject;
    }

    /// <summary>
    /// Queries the VR server to find all nodes with a specific name.
    /// </summary>
    /// <returns>The VR server's response</returns>
    private async Task<JsonObject> RequestNodesWithName(string name)
    {
        object sceneFindNodeCommand = Formatting.SceneNodeFind(name);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, sceneFindNodeCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Helper method to get the first node's UUID, or else an empty string.
    ///     It is assumed that the first node in the JsonArray is always the target node.
    /// </summary>
    private static string GetFirstNode(JsonArray nodes)
    {
        var node = nodes.First(); // only need one node with the name needed
        string? nodeId = node?["uuid"]?.ToString();

        if (nodeId == null)
            throw new CommunicationException($"Could not retrieve this node from the response.");

        return nodeId;
    }

    /// <summary>
    /// Removes a node from the VR environment.
    /// </summary>
    public async Task<JsonObject> RemoveNode(string nodeName)
    {
        string uuid = await GetNodeId(nodeName);

        object removeNodeCommand = Formatting.RemoveNode(uuid);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, removeNodeCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    #endregion

    #region Skybox
    /// <summary>
    /// Sets the time of the skybox, to be able to simulate day/night cycle
    /// </summary>
    public async Task<JsonObject> SetSkyTime(double time)
    {
        object setSkyCommand = Formatting.SetSkyboxTime(time);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, setSkyCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    #endregion

    #region Terrain
    /// <summary>
    /// Adds a grassy hills type terrain to the scene,
    /// with the height map generated using Perlin noise.
    /// </summary>
    /// <returns>A string concatenation of all responses sent by the server while creating the terrain.</returns>
    public async Task<string> AddHillTerrain(int length, int width, Vector3 position, Vector3 rotation)
    {
        float[] heightMap = PerlinNoiseGenerator.GenerateHeightMap(20);

        JsonObject terrainData = await AddTerrainData(length, width, heightMap);
        JsonObject terrainNode = await AddTerrainNode(position, rotation);
        JsonObject terrainLayer = await AddTerrainGrassLayer();

        return terrainData.ToString() + terrainNode.ToString() + terrainLayer.ToString();
    }

    /// <summary>
    ///     Add terrain data to VR scene
    ///     Since its only positional data, no visual component is rendered in the scene
    /// </summary>
    /// <param name="heightMap">Height data of the terrain</param>
    private async Task<JsonObject> AddTerrainData(int length, int width, float[] heightMap)
    {
        var terrainAddCommand = Formatting.TerrainAdd(length, width, heightMap);
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, terrainAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Add terrain node to VR scene
    ///     Visual component (layers) can be added after this
    /// </summary>
    private async Task<JsonObject> AddTerrainNode(Vector3 position, Vector3 rotation)
    {
        var addTerrainNodeCommand = Formatting.TerrainAddNode(position, rotation);
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, addTerrainNodeCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Add visual component to terrain
    ///     Requires actual node to add visual component
    /// </summary>
    private async Task<JsonObject> AddTerrainGrassLayer()
    {
        // Get the node ID of the terrain
        var terrainId = await GetNodeId("terrain");

        // Get the textures of the grass for the layer
        var diffuseFilePath = @"data\NetworkEngine\textures\grass_diffuse.png";
        var normalFilePath = @"data\NetworkEngine\textures\grass_normal.png";

        var addTerrainLayerCommand = Formatting.TerrainAddLayer(terrainId, diffuseFilePath, normalFilePath);
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, addTerrainLayerCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Get height of terrain at position
    /// </summary>
    /// <param name="position"> contains x, y, z position of model </param>
    private async Task<float> GetTerrainHeight(Vector3 position)
    {
        var heightGetCommand = Formatting.GetHeight(position);
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, heightGetCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        var heightJson = await VrCommunication.ReceiveJsonObject();

        var height = heightJson["data"]["data"]["data"]["height"].GetValue<float>(); //TODO error handling in case terrain is not added
        return height;
    }
    #endregion

    #region Models
    /// <summary>
    ///     Add a model in VR scene at the height of the terrain on a location
    /// </summary>
    /// <param name="name">name of node</param>
    /// <param name="position">position array containing x, y, z</param>
    /// <param name="scale">ses scaling of model</param>
    /// <param name="fileName"> filepath of the obj file of the model</param>
    public async Task<JsonObject> AddModelOnTerrain(string name, Vector3 position, double scale, string fileName)
    {
        // Get height of terrain at position
        var heightJson = await GetTerrainHeight(position);
        position.Y = heightJson; // set height of model to height of terrain

        var modelAddCommand = Formatting.Add3DObject(name, position, scale, fileName);
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, modelAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Add an animated model in VR scene
    /// </summary>
    /// <param name="name">name of node</param>
    /// <param name="position">position array containing x, y, z</param>
    /// <param name="scale">sets scaling of model</param>
    /// <param name="rotation"> rotates model in x-, y-, z-ax </param>
    /// <param name="fileName"> filepath of the obj file of the model</param>
    /// <param name="animationName"> filepath of the animation file</param>
    public async Task<JsonObject> AddAnimatedModel(string name, Vector3 position, double scale, Vector3 rotation,
        string fileName, string animationName)
    {
        var modelAddCommand = Formatting.AddAnimatedObject(
            name, position, scale, rotation, fileName, animationName
        );
        var tunnelMessage = Formatting.TunnelSend(_tunnelId, modelAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    /// <summary>
    ///     Add a number of trees randomly in VR scene placed on terrain
    /// </summary>
    /// <param name="amount">amount of spawned trees</param>
    /// <returns>json responses of the tree placements</returns>
    public async Task<JsonObject[]> RandomlyPlaceTrees(int amount)
    {
        var jsonResponses = new JsonObject[amount]; // array to save json responses
        var random = new Random();

        // iterate over amount of trees and add them to the scene
        for (var i = 0; i < amount; i++)
        {
            // generate random x,z position
            float x = random.Next(-128, 128);
            float z = random.Next(-128, 128);
            var position = new Vector3(x, 0, z);
            position.Y = await GetTerrainHeight(position); // set height of model to height of terrain

            var modelAddCommand = Formatting.Add3DObject(
                $"tree{i}",
                position,
                1,
                @"data\NetworkEngine\models\trees\fantasy\tree7.obj"
            );

            var tunnelMessage = Formatting.TunnelSend(_tunnelId, modelAddCommand);
            await VrCommunication.SendAsJson(tunnelMessage);

            jsonResponses[i] = await VrCommunication.ReceiveJsonObject(); // save response in array
        }

        return jsonResponses; // return array of json responses
    }
    #endregion

    #region Routes
    /// <summary>
    /// Add route to VR scene
    /// Nodes are added in the order they are given
    /// </summary>
    public async Task<JsonObject> AddRoute(PosVector[] nodes)
    {
        object routeAddCommand = Formatting.RouteAdd(nodes);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, routeAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }

    public string GetRouteId(JsonObject routeResponse)
    {
        string? routeId = routeResponse?["data"]?["data"]?["data"]?["uuid"]?.GetValue<String>();
        if (routeId == null)
            throw new CommunicationException("Could not find route from route response.");

        return routeId;
    }

    /// <summary>
    /// Let an object follow the route
    /// </summary>
    public async Task<JsonObject> FollowRoute(string route, string node, double speed)
    {
        object routeFollowCommand = Formatting.RouteFollow(route, node, speed);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, routeFollowCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    #endregion

    #region Roads
    /// <summary>
    /// Add road to VR scene
    /// Road follows route
    /// The diffuse, normal and specular are textures used for the road
    /// </summary>
    public async Task<JsonObject> AddRoad(string routeId)
    {
        // command data
        string normal = @"data\NetworkEngine\textures\terrain\ground_cracked_n.jpg";
        string diffuse = @"data\NetworkEngine\textures\terrain\ground_mud2_d.jpg";
        string specular = @"data\NetworkEngine\textures\terrain\ground_mud2_s.jpg";

        // create command and send to VR
        object roadAddCommand = Formatting.RoadAdd(routeId, diffuse, normal, specular);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, roadAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    
    public async Task<JsonObject> AddPanel(string name, Transform transform, int sizeX, int sizeY, int resolutionX, int resolutionY, Color color, bool castShadow)
    {
        object panelAddCommand = Formatting.PanelAdd(name, transform, sizeX, sizeY, resolutionX, resolutionY, color, castShadow);
        object tunnelMessage = Formatting.TunnelSend(_tunnelId, panelAddCommand);

        await VrCommunication.SendAsJson(tunnelMessage);
        return await VrCommunication.ReceiveJsonObject();
    }
    
    #endregion
}
