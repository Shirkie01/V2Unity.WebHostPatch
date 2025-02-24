# V2Unity.WebHostPatch
V2Unity multiplayer has been effectively shut down by the original developer choosing to not renew the domain host. This repo provides both a client patch to redirect the URLs and a web api to allow anyone to serve as host.

## Client Patch Installation
Extract [BepInEx](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) into the v8so root folder.

![image](https://github.com/user-attachments/assets/04302574-5aa3-40c5-828e-86fb43831b1d)


Double-click the BepInEx folder, and create a `plugins` folder.

Unzip `V2Unity.WebHostPatch.zip` into the `plugins` folder. There should be three files: 

```
Newtonsoft.Json.dll
V2Unity.Model.dll
V2Unity.WebHostPatch.dll
```

Run the game!

### Configuration
The default URL is `https://v2unityapi20250224101209.azurewebsites.net/`. If this site no longer is able to host the server to allow V2Unity multiplayer, clients can change which host to use by changing the URL value in the `v8so/BepInEx/config/V2Unity.WebHostPatch.cfg` file.

![image](https://github.com/user-attachments/assets/4c139425-e1d7-41bc-9bc0-d232e0c5c649)


# Server installation
The V2Unity.API project is an ASP.NET Core Web API project and can be hosted on any .NET-compatible server.
